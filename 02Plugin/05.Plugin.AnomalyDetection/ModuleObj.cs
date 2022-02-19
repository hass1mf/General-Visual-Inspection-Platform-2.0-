using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HalconDotNet;
using Heart.Inward;
using Heart.Outward;
using aqrose.aidi_vision_client;
using System.IO;
using System.Runtime.Serialization;

namespace Plugin.AnomalyDetection
{
    [Category("01@检测识别")]
    [DisplayName("02@异常检测#异常检测")]
    [Serializable]
    class ModuleObj : ModuleObjBase
    {
        public int CanvasIndex { get; set; } = -1;

        /// <summary>
        /// Aidi实例
        /// </summary>
        [NonSerialized]
        public AIDIRunner aidiCollect = new AIDIRunner();

        //图片格式转换器
        aqrose.aidi_vision_client.ImageConverter converter = new aqrose.aidi_vision_client.ImageConverter();

        public string ModelPath = "";
        public string Licence = "";

        public string hv_img_Str = "";
        public HImageExt hv_img = null;     //1015这里改为了HImageExt!!!原HObject
        MatchingTool.MatchResult finalResult;  //最后
        bool ExeResult;     //模板匹配最后的执行结果
        List<MeasureROI> OutputROIs = new List<MeasureROI>();   //执行最后输出到系统的Roi信息

        /// <summary>
        /// ①执行模块
        /// </summary>
        /// <param name="entryName"></param>
        /// <returns></returns>
        public override int ExeModule(string entryName)     //载入图像路径 
        {
            ReadImg();  //执行的第一步先读取图像进来
            ExeResult = false;   //先取false
            if (hv_img != null && aidiCollect != null)
            {
                AIDIResult aidiResult = new AIDIResult();
                aidiResult = aidiCollect.Run_Parse(converter.hobject_to_bitmap(hv_img), hv_img);

             //   List<RegionInfo> resultParser = ResultParser.AbstractRegionInfo(aidiResult);

               // List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>();
                //List<HObject> region = new List<HObject>();
               // AIDIResultToHregion(aidiResult, out region);
                //Obj_shapeMatchTool.Ho_Image = new HObject(hv_img);  //这里不仅要进行新的抓取，还要赋值给变量
                //Obj_shapeMatchTool.FindModel(out finalResult, out ExeResult);     //执行使，只直接进行执行查找模板函数
                //if (ExeResult)
                //{
                //    UpdateRoi(ref Obj_shapeMatchTool, finalResult.Angle, finalResult.Row, finalResult.Col);     //通过矩阵变换来更新计算新的ROI
                //    //搜索范围
                //    MeasureROI roi搜索范围 = new MeasureROI(Info.ProjectID.ToString(), "模板匹配", Info.ModuleName, enMeasureROIType.搜索范围.ToString(), "red", new HObject(Obj_shapeMatchTool.searchRegion.hobject));    //整理ROI信息
                //    hv_img.UpdateRoiList(roi搜索范围);
                //    ////模板范围
                //    //MeasureROI roi模板范围 = new MeasureROI(Info.ProjectID.ToString(), "模板匹配", Info.ModuleName, enMeasureROIType.模板范围.ToString(), "green", new HObject(Obj_shapeMatchTool.templateRegion.hobject));    //整理ROI信息
                //    //hv_img.UpdateRoiList(roi模板范围);
                //    //检测结果

                int index = 0;
                OutputROIs.Clear();     //一定要先清空

                if(aidiResult.regions.Count > 0)    //区域个数大于零表示识别有异常结果，检测成功
                {
                    foreach (Region item in aidiResult.regions)
                    {
                        string str = item.name;
                        string area = item.area.ToString();
                        //MeasureROIText roi结果文字 = new MeasureROIText(Info.ProjectID.ToString(), "异常输出" + index, Info.ModuleName, enMeasureROIType.文字显示.ToString(), "green", item);    //整理ROI信息
                        MeasureROIText roi标签文字 = new MeasureROIText(Info.ProjectID.ToString(), "异常标签输出" + index, Info.ModuleName, enMeasureROIType.文字显示.ToString(), "red", str + "  面积:" + area+ "  标准差:"+item.standardDeviation.ToString("0.0")+ "  最大/小:" + item.max_gray+"/"+item.min_gray+"  均值:" + item.mean_gray.ToString("0.0"), "mono", aidiResult.regions[index].polygon.outer.points[0].y, aidiResult.regions[index].polygon.outer.points[0].x, 20);
                        hv_img.UpdateRoiList(roi标签文字);
                        OutputROIs.Add(roi标签文字);
                        //MeasureROIText roi面积文字 = new MeasureROIText(Info.ProjectID.ToString(), "异常面积输出" + index, Info.ModuleName, enMeasureROIType.文字显示.ToString(), "red", area, "mono", aidiResult.regions[index].polygon.outer.points[0].y, aidiResult.regions[index].polygon.outer.points[0].x+50, 20);
                        //hv_img.UpdateRoiList(roi面积文字);
                        //OutputROIs.Add(roi面积文字);
                        index++;
                        MeasureROI roi检测结果 = new MeasureROI(Info.ProjectID.ToString(), "异常区域输出" + index, Info.ModuleName, enMeasureROIType.检测结果.ToString(), "green", item.region);    //整理ROI信息
                        hv_img.UpdateRoiList(roi检测结果);
                        OutputROIs.Add(roi检测结果);
                    }
                    ExeResult = true;   //识别有异常结果
                }
                else if (aidiResult.regions.Count == 0)     //区域个数大于零表示识别没有异常结果，检测失败
                {
                    ExeResult = false;   //识别没有异常结果
                }
                else
                {
                    ExeResult = false;   //识别没有异常结果
                }
                //   OutputROIs.Add(roi搜索范围);
                //    //OutputROIs.Add(roi模板范围);  去掉模板区域

                //    return 1;
                //}
                //else
                //{
                //    finalResult.Row = 0;
                //    finalResult.Col = 0;
                //    finalResult.Angle = 0;
                //    finalResult.Socre = 0;
                //    return -1;  //赋值操作要放在return前面，不然直接return就退出了
                //}
                return 1;   //这里的1表示的是当前函数执行成功，与识别结果无关
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// ②变量输出
        /// </summary>
        public override void UpdateOutput()
        {
            if (hv_img != null && ExeResult)
            {
                AddOutputVariable("异常检测结果", "bool", !ExeResult, "false", "异常检测结果");                                        //输出到变量表
                AddOutputVariable("异常检测个数", "int", OutputROIs.Count/2, (OutputROIs.Count/2).ToString(), "异常检测个数");             //输出到变量表
            }
            else
            {
                AddOutputVariable("异常检测结果", "bool", !ExeResult, "true", "异常检测结果");                                        //输出到变量表
                AddOutputVariable("异常检测个数", "int", OutputROIs.Count, "0", "异常检测个数");                                      //输出到变量表
            }
        }

        /// <summary>
        /// ③更新显示
        /// </summary>
        public override void UpdateDisplay()
        {
            if (hv_img != null && ExeResult && hv_img_Str != "")
            {
                int canvas = ConfirmCanvasByImageVariable(hv_img_Str);
                //AddDisplayImage(CanvasIndex, Ho_Image);     //显示图像，CanvasIndex为之前选择的画布
                AddDisplayRois(canvas, OutputROIs);      //和输出图像变量一样，输出RoiList的变量输出
            }
        }

        /// <summary>
        /// 给moduleform用的读图函数
        /// </summary>
        public void ReadImg()
        {
            if (hv_img_Str != "")
            {
                hv_img = new HImageExt((HImage)GetVariableDataByVarName(hv_img_Str));     //用文件索引来加载文件,并转换格式（IObject）   //20211022系统的传输图像变量改回为HImage，如果插件内部需要ROI内容，可自己将(himage)object实例为HImageExt
            }
        }


        /// <summary>
        /// 显示模板2
        /// </summary>
        /// <param name="matchingTool"></param>
        /// <param name="angle"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        internal void UpdateRoi(ref MatchingTool matchingTool, double angle, double row, double col)
        {
            try
            {
                HOperatorSet.GetShapeModelContours(out matchingTool.contour, matchingTool.modelID, new HTuple(1)); //shapeMatchTool.contour 最后输出的轮廓对象   //通过modelID来生成目标轮廓contour
                HHomMat2D homMat2D = new HHomMat2D();  //二维变换的齐次变换矩阵  
                homMat2D.VectorAngleToRigid(0, 0, 0, (HTuple)row, (HTuple)col, (HTuple)angle);    //从点和角度计算刚性仿射变换
                HOperatorSet.AffineTransContourXld(matchingTool.contour, out matchingTool.contour, homMat2D);   //有了放射矩阵homMat2D，计算后的shapeMatchTool.contour应该已经经过了修正    //使用旋转平移矩阵仿射变换来得到最新的目标轮廓contour 
                //HXLDCont modelRegionXLD = new HXLDCont();
                //HobjectToXld(matchingTool.searchRegion.hobject, ref modelRegionXLD);
                //modelRegionXLD.AffineTransContourXld(homMat2D);     //模型检测范围的搜索区域
                //matchingTool.searchRegion = new MeasureROI(Info.ProjectID.ToString(), "模板匹配", Info.ModuleName, enMeasureROIType.模板范围.ToString(), "green", new HObject(modelRegionXLD));    //整理ROI信息
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Hobject转Xld
        /// </summary>
        /// <param name="ho_Region"></param>
        /// <param name="Contour"></param>
        private void HobjectToXld(HObject ho_Region, ref HXLDCont Contour)
        {
            HObject ho_RegionBorder, ho_Contours;
            HTuple hv_Row, hv_Col;
            HOperatorSet.Boundary(ho_Region, out ho_RegionBorder, "inner");
            HOperatorSet.GenContourRegionXld(ho_RegionBorder, out ho_Contours, "border");
            HOperatorSet.GetContourXld(ho_Contours, out hv_Row, out hv_Col);
            Contour.GenContourPolygonXld(hv_Row, hv_Col);
        }

        private void AIDIResultToHregion(AIDIResult Label1, out List<HObject> hRogions)
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

        /// <summary>
        /// 有了[OnDeserialized()]标志的函数在反序列化时会被调用，与引用无关
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized()]
        internal void OnDeSerializedMethod(StreamingContext context)
        {

            //在序列化时，m_Thread和m_AutoResetEvent以为其类型的原因，并不能标记为序列化。故在反序列化时，要对这两个变量进行初始化
            if (aidiCollect == null)
            {
                aidiCollect = new AIDIRunner(Licence);  //重新生成对象
                if (ModelPath != "")
                {
                    aidiCollect.InitModel(ModelPath);   //确保模型路径正确才重新加载模型，加快速度
                }  
            }
        }

    }
}
