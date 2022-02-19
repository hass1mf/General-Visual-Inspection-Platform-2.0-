using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.Serialization;
using HalconDotNet;

namespace AcqDevice
{
    /// <summary>
    /// magical 20171027
    /// </summary>
    [Serializable]
    public class AcqUsbCamera : AcqAreaDeviceBase
    {
        
        private static HTuple Information, ValueList, Length, hv_AcqHandle;  //Direct相机Halcon采集参数
        public static string connect;       //相机名称
        public static string colorspace;    //颜色空间

        public AcqUsbCamera(DeviceType _DeviceType) : base(_DeviceType)
        {

        }

        public int m_TrigerMode
        {
            set
            { _TrigerMode = value; }
            get
            { return _TrigerMode; }
        }

        public float m_ExposeTime
        {
            set
            {
                _ExposeTime = value;
            }
            get
            {
                return _ExposeTime;
            }
        }

        /// <summary>
        /// 搜索相机
        /// </summary>
        /// <param name="m_CamInfoList"></param>
        public static void SearchCameras(out List<CamInfo> m_CamInfoList)
        {
            m_CamInfoList = new List<CamInfo>();
            HOperatorSet.InfoFramegrabber("DirectShow", "device", out Information, out ValueList);      //发现Direct相机，集合ValueList
            HOperatorSet.TupleLength(ValueList, out Length);    //计算ValueList的数量Length
            for (int i = 0; i < Length; i++)
            {
                CamInfo _camInfo = new CamInfo();
                _camInfo.m_UniqueName = ValueList.SArr[i];  //拿出当前的ValueList单元的名称，并赋值给_camInfo的m_UniqueName
                _camInfo.m_SerialNO = ValueList.SArr[i];    //m_SerialNO是设备的唯一标识，不能不写...
                m_CamInfoList.Add(_camInfo);
            }
        }

        /// <summary>
        /// 连接相机
        /// </summary>
        public override void ConnectDev()
        {
            if (hv_AcqHandle != null && m_bConnected)
            {
                m_bConnected = true;
                return;
            }
            try
            {
                //HOperatorSet.CloseAllFramegrabbers();
                connect = m_UniqueName;     //连接设备的名字
                colorspace = "gray";        //USB颜色空间先不开放，所以下面的参数也设置成"default"
                HOperatorSet.OpenFramegrabber("DirectShow", 1, 1, 0, 0, 0, 0, "default", 8, "gray", -1, "false", "default", connect, 0, -1, out hv_AcqHandle);
                m_bConnected = true;    //相机连接成功
                m_ExposeTime = 8000;    //halcon Direct相机没有曝光选项
                m_TrigerMode = 0;       //触发写软触发吧
                _Gain = 0;              //增益写0
            }
            catch (Exception ex)
            {
                m_bConnected = false;
            }
        }

        /// <summary>
        /// 断开相机
        /// </summary>
        public override void DisConnectDev()
        {
            if (m_bConnected)
            {
                // Destroy the camera object.
                try
                {
                    //HOperatorSet.CloseAllFramegrabbers();
                    HOperatorSet.CloseFramegrabber(hv_AcqHandle);
                    hv_AcqHandle = null;
                    m_bConnected = false;
                }
                catch (Exception e)
                {

                }
            }
        }

        /// <summary>
        /// 设置曝光
        /// </summary>
        /// <param name="dValue"></param>
        /// <returns></returns>
        public override bool setExposureTime(double dValue)
        {
            try
            {
                //Direct暂时没有曝光这一属性
                ////camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(dValue);
                //bool ok= camera.Parameters[PLCamera.ExposureTimeAbs].TrySetValue(dValue);
                //double xxx=camera.Parameters[PLCamera.ExposureTimeAbs].GetValue();

                //double? xxx1 = camera.Parameters[PLCamera.ExposureTimeAbs].GetIncrement();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 设置触发
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public override bool SetTriggerMode(TRIGGER_MODE mode)
        {
            try
            {
                //同样DirectShow相机没有触发模式
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        /// <summary>
        /// 设置触发模式--对外的函数
        /// </summary>
        /// <param name="mode"></param>
        private void SetTriggerMode(int mode)
        {
            switch (mode)
            {
                case 0:
                    {
                        SetTriggerMode(TRIGGER_MODE.硬件触发);
                        break;
                    }
                case 1:
                    {

                        SetTriggerMode(TRIGGER_MODE.软件触发);
                        break;
                    }
                case 2:
                    {
                        SetTriggerMode(TRIGGER_MODE.上升沿);
                        break;
                    }
                case 3:
                    {
                        SetTriggerMode(TRIGGER_MODE.下降沿);
                        break;
                    }
            }
        }

        /// <summary>
        /// 采集图像
        /// </summary>
        /// <param name="byHand">是否手动采图</param>
        public override bool CaptureImage(bool byHand)
        {
            HTuple cnnect;
            try
            {
                connect = m_UniqueName;     //连接设备的名字
                string conn = connect;
                HOperatorSet.TupleLength(conn, out cnnect);
                if (cnnect > 0)
                {
                    HObject h_image = new HObject();
                    HOperatorSet.GrabImage(out h_image, hv_AcqHandle);
                    m_Image = new HImage(h_image);
                    //使用委托传递出去
                    eventWait.Set();    //一定要对eventWait进行set操作，才能让外部函数知道当前采集已经完成，信号
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 未使用
        /// </summary>
        /// <param name="filePath"></param>
        public override void LoadSetting(string filePath)
        {
            base.LoadSetting(filePath);
        }

        /// <summary>
        /// 未使用
        /// </summary>
        /// <param name="filePath"></param>
        public override void SaveSetting(string filePath)
        {
            base.SaveSetting(filePath);
            //if (camera!=null)
            //{
            //    camera.Parameters.Save
            //}
        }

        public override void setSetting()
        {
            //设置曝光模式
            SetTriggerMode(_TrigerMode);
            //设置曝光时间
            //setExposureTime(_ExposeTime);

            base.setSetting();
        }

        public override void getSetting()
        {
            m_ExposeTime = 8000;
            m_TrigerMode = 0;
            base.getSetting();
        }

        /// <summary>
        /// 开始实时采集
        /// </summary>
        public void startGrab()
        {

        }

        /// <summary>
        /// 停止实时采集
        /// </summary>
        public void stopGrab()
        {

        }

        [OnDeserializing()]
        internal void OnDeSerializingMethod(StreamingContext context)
        {

        }
    }
}
