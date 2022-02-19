using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HalconDotNet;
using System.Threading;
using System.Runtime.Serialization;
//相机采集工具的基类

namespace AcqDevice
{
    public delegate void DispImageCallback(HImage img, int _user);
    [Serializable]
    public class AcqAreaDeviceBase : AcqDeviceBase
    {
        protected int _ImgWidth;
        protected int _ImgHeight;
        protected PIXEL_DEPTH _PixDepth;
        protected PIXEL_TYPE _PixType;

         
        public static float _exposetime;
        public float _ExposeTime//曝光时间
        {
            get { return _exposetime; }
            set { _exposetime = value; }
        }
        public int _TrigerMode = 0;//触发模式
        public float _Gain;//触发模式

        protected float _Framerate;//帧率
        [NonSerialized]
        public DispImageCallback dispImgCallback = null;

        public int m_ImgWidth
        {
            get { return _ImgWidth; }
        }

        public int m_ImgHeight
        {
            get { return _ImgHeight; }
        }

        public PIXEL_DEPTH m_PixDepth
        {
            get { return _PixDepth; }
        }

        public PIXEL_TYPE m_PixType
        {
            get { return _PixType; }
        }

        [NonSerialized]
        public AutoResetEvent SignalWait = new AutoResetEvent(false);//软触发时信号同步
        public AcqAreaDeviceBase(DeviceType _DeviceType) : base(_DeviceType)
        {
            m_SensorType = SENSOR_TYPE.Area;
        }

        public override void setTrigger()
        {
            base.setTrigger();
            SignalWait.Set();
        }

        /// <summary>
        /// 设置曝光时间
        /// </summary>
        /// <param name="dValue"></param>
        /// <returns></returns>
        public virtual bool setExposureTime(double dValue)
        {
            return true;
        }

        /// <summary>
        /// 设置触发模式
        /// </summary>
        /// <param name="mode">触发模式</param>
        /// <returns>成功返回True</returns>
        public virtual bool SetTriggerMode(TRIGGER_MODE mode)
        {
            return true;
        }

        [OnDeserializing()]
        internal void OnDeSerializingMethod(StreamingContext context)
        {
            SignalWait = new AutoResetEvent(false);//采集信号
        }
    }
}
