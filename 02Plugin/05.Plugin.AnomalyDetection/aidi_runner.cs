using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using aqrose.aidi_vision;

//json解析需要的库
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//计时需要的库
using System.Diagnostics;
using OpenCvSharp;
using HalconDotNet;

//--------------------------------------AidiRunner类---------------------------------------------
namespace aqrose.aidi_vision_client
{
    public class AIDIRunner
    {
        //传入IntPtr需要用到的接口函数
        [System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        //计时函数
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        //一个AidiRunner实例下有一个Client
        private Client aidi_;

        //加密狗
        private string check_code_ = "";

        //batchimage，存放待推理的图片
        private BatchImage batch_image = new BatchImage();

        //图片
        private aqrose.aidi_vision.Image image = new aqrose.aidi_vision.Image();

        //存放结果
        private BatchLabelIO results = new BatchLabelIO();

        //图片格式转换器
        aqrose.aidi_vision_client.ImageConverter converter = new aqrose.aidi_vision_client.ImageConverter();

        //判断runner是否初始化
        public bool IsEmpty
        {
            get
            {
                return aidi_ == null;
            }
        }

        //加密狗创建实例
        public AIDIRunner(string code = "")
        {
            //加密狗赋值
            check_code_ = code;
            //添加算法库目录
            Entry.SetLogFilter("info");
            //初始化日志
            Entry.InitLogFile("./aidi.log");
        }

        //释放资源
        public void Release() {
            aidi_.Dispose();
        }

        //输出日志
        public void LogInfo(string info)
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"./aidi.log", true);
            file.WriteLine(info);
            file.Close();
        }

        //通用初始化runner接口，参数分别为模型地址，使用gpu序号
        //模型目录的格式要求为，根目录可以取任意合法名称，下一级目录为类型+序号的名称，如Detect_0、Classify_1，最多可以组合2个模型，再下级目录存放模型文件model.aqbin和param.json。
        //不同的runner可指定一张显卡进行初始化，加快并行计算的速度。
        public bool InitModel(string work_path, int gpu_number = 0)
        {
            //模型地址列表
            StringVector save_model_path_list = new StringVector();
            //模型类型列表
            StringVector operator_type_list = new StringVector();
            //batchsize列表
            IntVector batch_size = new IntVector();

            //获取路径下的子模型文件夹名，如Detect_0、Classify_1，将模块名（Detect,Classify）与序号（0、1）分别填入字典model_index
            DirectoryInfo root_dir = new DirectoryInfo(work_path);
            DirectoryInfo[] dir = root_dir.GetDirectories();

            for (int want_i = 0; want_i < dir.Length; want_i++)
            {
                for (int d_i = 0; d_i < dir.Length; d_i++)
                {
                    if (dir[d_i].Name.IndexOf('_') > 0)
                    {
                        //temp_type为模块名，如"Detect"
                        string temp_type = dir[d_i].Name.Split('_')[0];
                        //temp_num为模块名后下划线后的序号字符串，如"1"
                        string temp_num = dir[d_i].Name.Split('_')[1];
                        if (temp_type != "Factory")
                        {
                            int temp_index = int.Parse(temp_num);
                            if (temp_index == want_i)
                            {
                                save_model_path_list.Add(work_path + "/" + dir[d_i].Name + "/model/V1");
                                operator_type_list.Add(temp_type);   //"Detection" 检测, "Location" 定位, "Classify" 分类, "Segment" 分割
                                batch_size.Add(1);
                            }
                        }
                    }
                }
            }

            //若指定了gpu，先修改配置文件   
            for (int modelIndex = 0; modelIndex < save_model_path_list.Count(); modelIndex++)
            {
                string testDir = save_model_path_list[modelIndex] + "/test.json";
                string saveDir = save_model_path_list[modelIndex] + "/test-" + gpu_number.ToString() + ".json";
                if (File.Exists(testDir))
                {
                    using (StreamReader r = new StreamReader(testDir, Encoding.UTF8))
                    {

                        string json = r.ReadToEnd();
                        JObject jo = (JObject)JsonConvert.DeserializeObject(json);
                        JArray gpu_num_array = new JArray();
                        gpu_num_array.Add(gpu_number);
                        jo["g_common"]["value"]["gpu_ids"]["value"] = gpu_num_array;
                        string convertString = Convert.ToString(jo);//将json装换为string
                        File.WriteAllText(saveDir, convertString);//将内容写进jon文件中
                    }
                }
                else
                {
                    LogInfo("No test.json file : " + testDir);
                }
            }

            try
            {
                //初始化runner
                aidi_ = new Client(check_code_);
                for (int modelIndex = 0; modelIndex < save_model_path_list.Count(); modelIndex++)
                {
                    aidi_.add_model_engine(operator_type_list[modelIndex], save_model_path_list[modelIndex], save_model_path_list[modelIndex] + "/test-" + gpu_number.ToString() + ".json");
                }

                //aidi_.add_route_engine();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("**************************************\r\n" + "异常信息：" + ex.Message + "\r\n**************************************");
                return false;
            }

        }

        //输入图像数据一维数组，高，宽和通道数的推理接口
        public string Run(byte[] transform_image, int height, int width, int channel)
        {

            //转换图像
            image.from_chars(transform_image, height, width, channel);

            //清空batch_image
            batch_image.Clear();

            //向batch中加入图像
            batch_image.Add(image);

            //将batch加入client
            ulong id = aidi_.add_images(batch_image);

            //推理
            results = aidi_.wait_get_result(ref id);

            //取出结果
            LabelIO result = results[0];

            //转成json
            string s_result = result.to_json();

            //返回的字符串应该是GB2312编码的
            return s_result;

        }

        //输入aqrose.aidi_vision.Image推理的接口
        public string Run(aqrose.aidi_vision.Image image)
        {

            //清空batch_image
            batch_image.Clear();

            //向batch中加入图像
            batch_image.Add(image);

            //将batch加入client
            ulong id = aidi_.add_images(batch_image);

            //推理
            results = aidi_.wait_get_result(ref id);

            //取出结果
            LabelIO result = results[0];

            //转成json
            string s_result = result.to_json();

            //返回的字符串应该是GB2312编码的
            return s_result;

        }

        //输入Bitmap推理的接口
        public string Run(Bitmap bitmap)
        {

            //将bitmap转成byte[]
            int stride, channel_num;
            byte[] transform_image = converter.bitmap_to_byte(ref bitmap, out stride, out channel_num);

            //读成aqimage图像
            image.from_chars(transform_image, bitmap.Height, bitmap.Width, channel_num);

            //清空batch_image
            batch_image.Clear();

            //向batch中加入图像
            batch_image.Add(image);

            //将batch加入client
            ulong id = aidi_.add_images(batch_image);

            //推理
            results = aidi_.wait_get_result(ref id);

            //取出结果
            LabelIO result = results[0];

            //转成json
            string s_result = result.to_json();

            //返回的字符串应该是GB2312编码的
            return s_result;

        }

        //输入IntPtr指针的接口，图像转换效率会提升
        public string Run(IntPtr transform_image, int height, int width, int channel)
        {
            //新建AIDIImage
            aqrose.aidi_vision.Image aidi_image = new aqrose.aidi_vision.Image(1, height, width, channel);

            //快速拷贝图像数据
            CopyMemory(aidi_image.mutable_data(), transform_image, (uint)height * (uint)width * (uint)channel);

            //添加图片
            batch_image.Clear();
            batch_image.Add(image);

            //准备推理
            ulong id = aidi_.add_images(batch_image);

            //推理
            results = aidi_.wait_get_result(ref id);

            //取出结果
            LabelIO result = results[0];

            //转成json
            string s_result = result.to_json();

            //返回的字符串应该是GB2312编码的
            return s_result;

        }

        
        //返回AIDIResult，输入图像数据一维数组，高，宽和通道数的推理接口
        public AIDIResult Run_Parse(byte[] transform_image, int height, int width, int channel)
        {

            //转换图像
            image.from_chars(transform_image, height, width, channel);

            //清空batch_image
            batch_image.Clear();

            //向batch中加入图像----批处理图像
            batch_image.Add(image);

            //将batch加入client
            ulong id = aidi_.add_images(batch_image);

            //推理
            results = aidi_.wait_get_result(ref id);

            //取出结果
            LabelIO result = results[0];

            //转成json
            string s_result = result.to_json();

            //一行代码转换成AidiResult
            AIDIResult aidi_result = ResultParser.Parse(s_result);

            //返回
            return aidi_result;

        }

        //返回AIDIResult，输入aqrose.aidi_vision.Image推理的接口
        public AIDIResult Run_Parse(aqrose.aidi_vision.Image image)
        {

            //清空batch_image
            batch_image.Clear();

            //向batch中加入图像
            batch_image.Add(image);

            //将batch加入client
            ulong id = aidi_.add_images(batch_image);

            //推理
            results = aidi_.wait_get_result(ref id);

            //取出结果
            LabelIO result = results[0];

            //转成json
            string s_result = result.to_json();

            //一行代码转换成AidiResult
            AIDIResult aidi_result = ResultParser.Parse(s_result);

            //返回
            return aidi_result;

        }

        //返回AIDIResult，输入Bitmap推理的接口
        public AIDIResult Run_Parse(Bitmap bitmap)
        {

            //将bitmap转成byte[]
            int stride, channel_num;
            byte[] transform_image = converter.bitmap_to_byte(ref bitmap, out stride, out channel_num);

            //读成aqimage图像
            image.from_chars(transform_image, bitmap.Height, bitmap.Width, channel_num);

            //清空batch_image
            batch_image.Clear();

            //向batch中加入图像
            batch_image.Add(image);

            //将batch加入client
            ulong id = aidi_.add_images(batch_image);

            //推理
            results = aidi_.wait_get_result(ref id);

            //取出结果
            LabelIO result = results[0];

            //转成json
            string s_result = result.to_json();

            //一行代码转换成AidiResult
            AIDIResult aidi_result = ResultParser.Parse(s_result);

            if (aidi_result.regions.Count > 0)
            {
                foreach (aqrose.aidi_vision_client.Region item in aidi_result.regions)
                {
                    //List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>();
                    //Outer outer = item.polygon.outer;
                    //for (int j = 0; j < outer.points.Count; j++)
                    //{
                    //    points.Add(new OpenCvSharp.Point(outer.points[j].x, outer.points[j].y));
                    //}
                    ////用opencv计算
                    //double area = Cv2.ContourArea(points);
                    //item.area = area;
                    HObject hObject;
                    hObject = converter.bitmap_to_hobject_unsafe(bitmap);
                }
            }
        //    Cv2.CalcCovarMatrix
            //返回
            return aidi_result;

        }
        public AIDIResult Run_Parse(Bitmap bitmap,HObject hobject)
        {

            //将bitmap转成byte[]
            int stride, channel_num;
            byte[] transform_image = converter.bitmap_to_byte(ref bitmap, out stride, out channel_num);

            //读成aqimage图像
            image.from_chars(transform_image, bitmap.Height, bitmap.Width, channel_num);

            //清空batch_image
            batch_image.Clear();

            //向batch中加入图像
            batch_image.Add(image);

            //将batch加入client
            ulong id = aidi_.add_images(batch_image);

            //推理
            results = aidi_.wait_get_result(ref id);

            //取出结果
            LabelIO result = results[0];

            //转成json
            string s_result = result.to_json();

            //一行代码转换成AidiResult
            AIDIResult aidi_result = ResultParser.Parse(s_result);

            if (aidi_result.regions.Count > 0)
            {
                foreach (Region item in aidi_result.regions)
                {

                    AIDIResultToHregion(item,out item.region);
                    if (channel_num>1)
                    {
                        HOperatorSet.Rgb1ToGray(hobject,out hobject);
                    }
                    HTuple h_min_gray = new HTuple();
                    HTuple h_max_gray = new HTuple();
                    HTuple h_range = new HTuple();
                    HOperatorSet.MinMaxGray(item.region, hobject, 0, out h_min_gray, out h_max_gray, out h_range);
                    HTuple h_mean_gray = new HTuple();
                    HTuple h_standarddDeviation = new HTuple();
                    HOperatorSet.Intensity(item.region, hobject, out h_mean_gray, out h_standarddDeviation);
                    HTuple area,row,colum;
                    HOperatorSet.AreaCenter(item.region, out area, out row, out colum);

                    //最小值
                    item.min_gray = h_min_gray;
                    //最大值
                    item.max_gray = h_max_gray;
                    //平均值
                    item.mean_gray = h_mean_gray;
                    //方差
                    item.deviation = Math.Pow(h_standarddDeviation, 2);
                    //标准差
                    item.standardDeviation = h_standarddDeviation;
                    //面积
                    item.area = area.D;
                }

            }
            //    Cv2.CalcCovarMatrix
            //返回
            return aidi_result;

        }

        //返回画好结果的图像的推理接口
        public Bitmap Run_Draw_Bitmap(byte[] transform_image, int height, int width, int channel)
        {

            //准备图片
            image.from_chars(transform_image, height, width, channel);

            //准备batchImage
            batch_image.Clear();

            //添加图片
            batch_image.Add(image);

            //添加id，防止多线程时错乱
            ulong id = aidi_.add_images(batch_image);

            //推理
            results = aidi_.wait_get_result(ref id);

            //用自带渲染库画上结果
            image.draw(results[0]);

            //转换图片格式
            Bitmap result_image = converter.aqimg_to_bitmap(ref image);

            //返回
            return result_image;

        }

        //多图推理，输入Bitmap列表的接口
        public List<AIDIResult> Run_List(List<Bitmap> bitmapList)
        {
            
            //准备batchImage
            batch_image.Clear();

            //遍历所有图片，转换成AidiImage加入batch
            for (int i = 0; i < bitmapList.Count(); i++)
            {
                //bitmap转aidiimage
                aqrose.aidi_vision.Image aidi_image = new aqrose.aidi_vision.Image();
                Bitmap bitmap = bitmapList[i];
                converter.bitmap_to_aqimg(ref bitmap, ref aidi_image);
                batch_image.Add(aidi_image);
            }

            //添加id，防止多线程时错乱
            ulong id = aidi_.add_images(batch_image);

            //推理
            results = aidi_.wait_get_result(ref id);

            //初始化结果List
            List<AIDIResult> aidi_results = new List<AIDIResult>();

            //遍历结果，进行转换
            for (int i = 0; i < bitmapList.Count(); i++)
            {

                //取出batch中的第i个
                LabelIO result = results[i];

                //转换成json字符串并返回
                string json_result = result.to_json();

                //一行代码转换成AidiResult
                AIDIResult aidi_result = ResultParser.Parse(json_result);

                //存结果
                aidi_results.Add(aidi_result);

            }

            //返回
            return aidi_results;

        }


        private void AIDIResultToHregions(AIDIResult Label1, out List<HObject> hRogions)
        {

            HTuple sx = new HTuple();
            HTuple sy = new HTuple();
            hRogions = new List<HObject>();
            hRogions.Clear();
            HObject hRogion = new HObject();
            for (int j = 0; j < Label1.regions.Count; j++)
            {
                int num = 0;
                for (int i = 0; i < Label1.regions[j].polygon.outer.points.Count; i++)
                {
                    sx = sx.TupleConcat((HTuple)Label1.regions[j].polygon.outer.points[i].x);
                    sy = sy.TupleConcat((HTuple)Label1.regions[j].polygon.outer.points[i].y);
                    //                    sx += (HTuple) Label1.regions[j].polygon.outer.points[i].x;
                    //                    sy += (HTuple) Label1.regions[j].polygon.outer.points[i].y;
                    num++;
                }
                hRogion = null;
                HOperatorSet.GenRegionPolygonFilled(out hRogion, sy, sx);
                hRogions.Add(hRogion);
                sx.DArr = null;
                sy.DArr = null;
            }
            for (int i = 0; i < hRogions.Count; i++)
            {
                //HOperatorSet.SetColor(WindowID, "red");
                //HOperatorSet.DispObj(hRogions[i], WindowID);
            }
        }

        private void AIDIResultToHregion(Region Label1, out HObject hRogion)
        {

            HTuple sx = new HTuple();
            HTuple sy = new HTuple();             
            int num = 0;
             for (int i = 0; i < Label1.polygon.outer.points.Count; i++)
             {
               sx = sx.TupleConcat((HTuple)Label1.polygon.outer.points[i].x);
               sy = sy.TupleConcat((HTuple)Label1.polygon.outer.points[i].y);
             }
            hRogion = null;
            HOperatorSet.GenRegionPolygonFilled(out hRogion, sy, sx);
        }

    }

}

