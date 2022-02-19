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
using AcqDevice;

namespace Plugin.ImageAcquire
{
    [Category("00@图像处理")]
    [DisplayName("00@采集图像#采集图像")]
    [Serializable]
    class ModuleObj : ModuleObjBase
    {
        #region 变量定义声明

        #endregion

        //变量必须赋初值
        public string ImagePath { get; set; } = null;
        public HImage AcquiredImage { get; set; } = null;

        public HObject image = null;
        public HTuple AcqHandle = null;

        public string input_str = null;
        public string FileDirectoryPath { get; set; } = null;   //文件夹路径
        public bool IsSpecify { get; set; } = true;  //是否选择指定图像
        public bool IsFileDirectory { get; set; } = false;  //是否选择文件目录
        public bool IsCamera { get; set; } = false;    //是否选择相机
        public string CameraName { get; set; } = "";   //相机名称
        public string CameraRemark { get; set; } = "";  //相机备注
        public int exposure_time { get; set; } = 0;   //曝光时间
        public int gain { get; set; } = 0;     //增益
        public int index = 0;
        //[field: NonSerialized()]
        //public VideoCaptureDevice finalFrame = null;
        public List<string> ImagePathList = new List<string>();     //ImagePathList文件目录下所有图像路径的List
        public List<Image> imagelist = new List<Image>();           //imagelist内容为①图像名称imagename②图像索引index③图像路径imagepath    该变量存在的意义是在插件UI上可以显示出一个List图像列表供点击
        public List<string> cameralist = new List<string>();        //cameralist相机列表

        /// <summary>
        /// 邓新加的图像类，用于储存文件目录下的图像List信息
        /// </summary>
        [Serializable]
        public class Image : INotifyPropertyChanged
        {
            public string imagename;
            public int index;
            public string imagepath;
            [field: NonSerialized()]
            public event PropertyChangedEventHandler PropertyChanged;
            public string ImageName
            {
                get { return imagename; }
                set
                {
                    imagename = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ImageName"));
                    }
                }
            }
            public int Index
            {
                get { return index; }
                set
                {
                    index = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Index"));
                    }
                }
            }
            public string ImagePath
            {
                get { return imagepath; }
                set
                {
                    imagepath = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("ImagePath"));
                    }
                }

            }
            public Image(int index, string imagename, string imagepath)
            {
                this.ImageName = imagename;
                this.Index = index;
                this.ImagePath = imagepath;
            }
        }

        [NonSerialized]
        public AcqAreaDeviceBase m_AcqDevice;
        private string _DeviceID;
        public string m_DeviceID
        {
            set
            {
                _DeviceID = value;
                m_AcqDevice = (AcqAreaDeviceBase)Solution.g_AcqDeviceList.FirstOrDefault(c => c.m_DeviceID == _DeviceID);    //这里运行的时候可能会对m_AcqDevice进行赋值，对工程加载初始化很重要
            }
            get { return _DeviceID; }
        }

        /// <summary>
        /// ①执行插件 
        /// ModuleObj内的ExeModule执行函数会有两个地方。
        /// 一个是在ModuleForm端插件内部点击执行时->RunModuleBefore();->ModuleObjBase.ExeModule("")->RunModuleAfter();
        /// 另一个在框架直接开始运行，点击单次或循环运行时->ExeModule(entryName);->UpdateOutputVariable();->UpdateDisplay();
        /// </summary>
        /// <param name="entryName"></param>
        /// <returns></returns>
        public override int ExeModule(string entryName)
        {
            if (IsSpecify)
            {
                if (!string.IsNullOrEmpty(input_str))
                {
                    //AcquiredImage = new HImage(ImagePath);
                    ReadImg();
                    return 1;   //return 1执行成功
                }
                else
                {
                    AcquiredImage = null;
                    return -1;
                }
            }
            else if (IsFileDirectory)
            {
                if (FileDirectoryPath != null)
                {
                    AcquiredImage = new HImage(ImagePathList[index]);    //每次点击执行默认自动循环
                    if (index == ImagePathList.Count - 1)
                    {
                        index = 0;
                        return 1;
                    }
                    else { index++; }
                    return 1;
                }
            }
            else if (IsCamera)                                                    //USB相机_Integrated Webcam
            {
                if (m_AcqDevice == null)    m_AcqDevice = (AcqAreaDeviceBase)Solution.g_AcqDeviceList.FirstOrDefault(c => c.m_DeviceID == _DeviceID);    //这里运行的时候可能会对m_AcqDevice进行赋值，对工程加载初始化很重要
                if (m_AcqDevice != null && IsCamera == true)     //使用halcon的函数平均1.5s   使用aforge平均40ms
                {
                    //HOperatorSet.OpenFramegrabber("DirectShow", 1, 1, 0, 0, 0, 0, "default", 8, "rgb", -1, "false", "default", "[0] " + CameraRemark, 0, -1, out AcqHandle);
                    //HOperatorSet.SetFramegrabberParam(AcqHandle, "exposure", exposure_time);  //设置曝光时间
                    //HOperatorSet.SetFramegrabberParam(AcqHandle, "video_gain", gain);    //设置增益
                    //HOperatorSet.GrabImageStart(AcqHandle, -1);
                    //HOperatorSet.GrabImageAsync(out image, AcqHandle, -1);
                    //AcquiredImage = new HImage(image);
                    //HOperatorSet.CloseFramegrabber(AcqHandle);

                    ////FilterInfoCollection CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                    ////finalFrame = new VideoCaptureDevice(CaptureDevice[1].MonikerString);
                    ////finalFrame.SetCameraProperty(CameraControlProperty.Exposure, exposure_time, CameraControlFlags.Manual);  //设置曝光时间
                    ////finalFrame.NewFrame += new NewFrameEventHandler(finalFrame_newframe);
                    ////finalFrame.Start();
                    //GC.Collect();
                    m_AcqDevice.eventWait.Reset();
                    //发送采集命令                   

                    if (!m_AcqDevice.CaptureImage(true))   //此处给m_AcqDevice赋值了
                    {
                        return -1;
                    }
                    m_AcqDevice.eventWait.WaitOne();    //这里会等待到硬触发的IO进来
                    try
                    {
                        AcquiredImage = m_AcqDevice.m_Image;
                        return 1;
                    }
                    catch
                    {
                        return -1;
                    }
                }
                else
                {
                    return -1;
                }
            }
            return 0;

        }

        /// <summary>
        /// ②更新输出
        /// </summary>
        public override void UpdateOutput()
        {
            HTuple width;
            HTuple height;
            if (AcquiredImage != null)
            {
                AcquiredImage.GetImageSize(out width, out height);
                AddOutputVariable("AcquiredImage", "Image", AcquiredImage, width.ToString()+"x"+height.ToString(), "采集到的图像");   // public bool AddOutputVariable(string vName,string vType, object vObject,string vValue,string vRemark) 
            }
            else
            {
                AddOutputVariable("AcquiredImage", "Image", AcquiredImage, "null", "采集到的图像");   // public bool AddOutputVariable(string vName,string vType, object vObject,string vValue,string vRemark) 
            }                   
        }


        public int ReadImg()
        {
            if (input_str != "" && input_str != null)
            {
                string obj= (string)GetVariableDataByVarName(input_str);
            
                if (obj != null&& obj != "")
                {
                    if (obj.Contains(@":\") == true)
                    {                    
                        if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(obj)))
                        {
                            try
                            {
                                AcquiredImage = new HImageExt(obj);     //用文件索引来加载文件,并转换格式（IObject）   //20211022系统的传输图像变量改回为HImage，如果插件内部需要ROI内容，可自己将(himage)object实例为HImageExt
                                return 1;
                            }
                            catch
                            {

                                AcquiredImage= null;
                                return -1;
                            }
                        }
                        return -1;
                    }
                    return -1;
                }
                return -1;
            }
            return -1;
        }
        /// <summary>
        /// ③更新显示
        /// </summary>
        public override void UpdateDisplay()
        {
            if (AcquiredImage != null)
            {
                if (IsSpecify)
                {
                    AddDisplayImage(CanvasIndex, AcquiredImage);
                }
                else if (IsFileDirectory)
                {
                    AddDisplayImage(CanvasIndex, AcquiredImage);
                }
                else
                {
                    AddDisplayImage(CanvasIndex, AcquiredImage);
                }
            }
        }
    }
}
