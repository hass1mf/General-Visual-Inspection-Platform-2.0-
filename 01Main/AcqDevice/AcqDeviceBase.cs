using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HalconDotNet;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.Serialization;
//采集设备的基类定义

namespace AcqDevice
{
    [Serializable]
    public class AcqDeviceBase
    {
        public static int m_LastDeviceID = 0;
        private string _DeviceID;       //设备ID,自己编号
        private DeviceType _DeviceType;
        protected string _UniqueName;  //唯一标示 eg:设备连接ID/文件夹路劲/设备有效识别的ID
        private string _DevDirExt;      //设备扩展标识，大部分时间用不上
        private string _SerialNo;       //设备内部编号
        private bool _bConnected = false;//设备当前状态
        [NonSerialized]
        private HImage _Image = new HImage();
        private bool _IsNewImage;       //是不是最新采集的图像
        [NonSerialized]
        public AutoResetEvent eventWait = new AutoResetEvent(false);//采集信号

        /// <summary>
        /// sensor 类型 是线扫还是面阵相机
        /// </summary>
        public SENSOR_TYPE m_SensorType;

        /// <summary>
        /// 设备ID,自己编号
        /// </summary>
        public string m_DeviceID
        {
            set { _DeviceID = value; }
            get { return _DeviceID; }
        }

        /// <summary>
        /// 设备类型
        /// </summary>
        public DeviceType m_DeviceType
        {
            set { _DeviceType = value; }
            get { return _DeviceType; }
        }

        /// <summary>
        /// 唯一标示 eg:设备连接ID/文件夹路劲/设备有效识别的ID
        /// </summary>
        public string m_UniqueName
        {
            set { _UniqueName = value; }
            get { return _UniqueName; }
        }

        /// <summary>
        /// 设备扩展标识，大部分时间用不上
        /// </summary>
        public string m_DevDirExt
        {
            set { _DevDirExt = value; }
            get { return _DevDirExt; }
        }

        /// <summary>
        /// 设备初始期许状态是否连接
        /// </summary>
        public bool m_bConnected
        {
            set { _bConnected = value; }
            get { return _bConnected; }
        }

        /// <summary>
        /// 设备内部编号
        /// </summary>
        public string m_SerialNo
        {
            set { _SerialNo = value; }
            get { return _SerialNo; }
        }

        /// <summary>
        /// m_Image作为采集设备最后的图像
        /// </summary>
        public HImage m_Image
        {
            set { _Image = value; }
            get { return _Image; }
        }

        /// <summary>
        /// 是不是最新采集的图像
        /// </summary>
        public bool m_IsNewImage
        {
            set { _IsNewImage = value; }
            get { return _IsNewImage; }
        }

        /// <summary>
        /// X方向像素当量
        /// </summary>
        private double dScaleX = 1.0;
        public double ScaleX
        {
            get { return dScaleX; }
            set { dScaleX = value; }
        }

        /// <summary>
        /// Y方向像素当量
        /// </summary>
        private double dScaleY = 1.0;
        public double ScaleY
        {
            get { return dScaleY; }
            set { dScaleY = value; }
        }

        /// <summary>
        /// 设置路径
        /// </summary>
        public string m_SettingPath
        {
            get { return Application.StartupPath + "\\" + m_DeviceID + "_Setting"; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_DeviceType">初始化时指定设备类型</param>
        public AcqDeviceBase(DeviceType _DeviceType)
        {
            this._DeviceType = _DeviceType;
            m_LastDeviceID++;
            _DeviceID = "Dev" + m_LastDeviceID.ToString();
        }

        //public virtual void CreateDevice(ref CamInfo _CamInfo);//创建设备
        /// <summary>
        /// 连接设备
        /// </summary>
        public virtual void ConnectDev()//建立设备连接
        {
            LoadSetting(Application.StartupPath + @"\CameraConfig" + this.m_SerialNo);
        }

        /// <summary>
        /// 断开设备
        /// </summary>
        public virtual void DisConnectDev() //断开设备连接
        {

        }

        /// <summary>
        /// 抓捕图像
        /// </summary>
        /// <param name="byHand">是否手动采图</param>
        /// <returns>采集是否成功</returns>
        public virtual bool CaptureImage(bool byHand)
        {
            //这里如果传入true，就会触发函数来进行抓图
            return false;
        }

        public virtual void setSetting()//设置
        {
        }

        public virtual void getSetting()//设置
        {

        }

        public virtual void SaveSetting(string filePath)
        {
        }

        public virtual void LoadSetting(string filePath)
        {
        }

        //public virtual void ReleaseDevice();//释放设备

        public virtual void setTrigger()
        {
        }

        [OnDeserializing()]
        internal void OnDeSerializingMethod(StreamingContext context)
        {
            _Image = new HImage();
            eventWait = new AutoResetEvent(false);//采集信号
        }
    }
}
