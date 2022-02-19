using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using HalconDotNet;

namespace AcqDevice
{
    [Serializable]
    public class AcqHKVision : AcqAreaDeviceBase
    {
        [NonSerialized]
        private MyCamera m_pMyCamera = new MyCamera();     //海康相机的实例，后期对海康的操作需要通过它

        [NonSerialized]
        private MyCamera.cbOutputExdelegate ImageCallback;     //相机的回调

        [NonSerialized]
        private MyCamera.MV_CC_DEVICE_INFO CurDevice;   //相机的基本信息，包括 nMajorVer:主要版本 nMinorVer:次要版本 nMacAddrHigh:高MAC地址 nMacAddrLow:低MAC地址 nTLayerType:设备传输层协议类型  nReserved:预留

        [NonSerialized]
        byte[] m_pBufForSaveImage = new byte[50 * 1024 * 1024];    //图片缓存容器

        /// <summary>
        /// 曝光时间
        /// </summary>
        public float m_ExposeTime
        {
            set { _ExposeTime = value; }
            get { return _ExposeTime; }
        }

        /// <summary>
        /// 触发模式
        /// </summary>
        public int m_TrigerMode
        {
            set { _TrigerMode = value; }
            get { return _TrigerMode; }
        }

        /// <summary>
        /// 帧数
        /// </summary>
        public float m_Framerate
        {
            set { _Framerate = value; }
            get { return _Framerate; }
        }

        /// <summary>
        /// 外部信息
        /// </summary>
        public object extInfo
        {
            set { CurDevice = (MyCamera.MV_CC_DEVICE_INFO)value; }
            get { return CurDevice; }
        }

        public AcqHKVision(DeviceType _DeviceType) : base(_DeviceType)
        {
        }

        /// <summary>
        /// 查找相机，添加到相机列表中
        /// </summary>
        public static void SearchCameras(out List<CamInfo> m_CamInfoList)
        {
            MyCamera.MV_CC_DEVICE_INFO_LIST m_pDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            m_CamInfoList = new List<CamInfo>();

            int nRet;
            // ch:创建设备列表 en:Create Device List
            System.GC.Collect();
            nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_pDeviceList);
            if (0 != nRet)
            {
                MessageBox.Show("查找设备失败");
                return;
            }

            // ch:在窗体列表中显示设备名 | en:Display device name in the form list
            for (int i = 0; i < m_pDeviceList.nDeviceNum; i++)
            {
                CamInfo _camInfo = new CamInfo();
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    if (gigeInfo.chUserDefinedName != "")
                    {
                        _camInfo.m_CamName = "GigE: " + gigeInfo.chUserDefinedName + " (" + gigeInfo.chSerialNumber + ")";
                    }
                    else
                    {
                        _camInfo.m_CamName = "GigE: " + gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" + gigeInfo.chSerialNumber + ")";
                    }
                    _camInfo.m_SerialNO = gigeInfo.chSerialNumber;
                    _camInfo.m_UniqueName = gigeInfo.chSerialNumber;
                    _camInfo.mExtInfo = device;
                    _camInfo.m_MaskName = gigeInfo.chSerialNumber;
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    if (usbInfo.chUserDefinedName != "")
                    {
                        _camInfo.m_CamName = "USB: " + usbInfo.chUserDefinedName + " (" + usbInfo.chSerialNumber + ")";
                    }
                    else
                    {
                        _camInfo.m_CamName = ("USB: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")");
                    }
                    _camInfo.m_SerialNO = usbInfo.chSerialNumber;
                    _camInfo.m_UniqueName = usbInfo.chSerialNumber;
                    _camInfo.mExtInfo = device;
                    _camInfo.m_MaskName = usbInfo.chSerialNumber;
                }

                m_CamInfoList.Add(_camInfo);
            }


        }

        /// <summary>
        /// 连接海康相机
        /// </summary>
        public override void ConnectDev()
        {
            try
            {
                base.ConnectDev();
                // 如果设备已经连接先断开
                DisConnectDev();
                int nRet = -1;
                // ch:打开设备 | en:Open device
                if (null == m_pMyCamera)
                {
                    m_pMyCamera = new MyCamera();
                    if (null == m_pMyCamera)
                    {
                        return;
                    }
                }

                nRet = m_pMyCamera.MV_CC_CreateDevice_NET(ref CurDevice);
                if (MyCamera.MV_OK != nRet)
                {
                    return;
                }
                nRet = m_pMyCamera.MV_CC_OpenDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    m_pMyCamera.MV_CC_DestroyDevice_NET();
                    ShowErrorMsg("Device open fail!", nRet);
                    return;
                }
                setSetting();
                // ch:设置采集连续模式 | en:Set Continues Aquisition Mode
                //m_pMyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", 2);// ch:工作在连续模式 | en:Acquisition On Continuous Mode
                // ch:注册回调函数 | en:Register image callback
                ImageCallback = new MyCamera.cbOutputExdelegate(ImageCallbackFunc);
                nRet = m_pMyCamera.MV_CC_RegisterImageCallBackEx_NET(ImageCallback, IntPtr.Zero);
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Register image callback failed!");
                }

                // ch:设置采集连续模式 | en:Set Continues Aquisition Mode
                //m_pMyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", 2);// ch:工作在连续模式 | en:Acquisition On Continuous Mode
                // ch:开启抓图 || en: start grab image
                nRet = m_pMyCamera.MV_CC_StartGrabbing_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    Console.WriteLine("Start grabbing failed:{0:x8}", nRet);
                }
                m_bConnected = true;
            }
            catch (Exception ex)
            {
                m_bConnected = false;
            }

        }

        /// <summary>
        /// 断开海康相机
        /// </summary>
        public override void DisConnectDev()
        {
            if (m_bConnected /*&& camera.DeviceValid*/)
            {
                int nRet;

                nRet = m_pMyCamera.MV_CC_CloseDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    return;
                }

                nRet = m_pMyCamera.MV_CC_DestroyDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    return;
                }
                m_bConnected = false;
            }
        }

        /// <summary>
        /// 关键的回调函数，这里会产生对HImage的赋值
        /// </summary>
        /// <param name="pData"></param>
        /// <param name="pFrameInfo"></param>
        /// <param name="pUser"></param>
        void ImageCallbackFunc(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            //Console.WriteLine("Get one frame: Width[" + Convert.ToString(pFrameInfo.nWidth) + "] , Height[" + Convert.ToString(pFrameInfo.nHeight)
            //                    + "] , FrameNum[" + Convert.ToString(pFrameInfo.nFrameNum) + "]");
            try
            {
                if (pFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)    //第一种是Mono8的灰度模式
                {
                    m_Image.GenImage1("byte", pFrameInfo.nWidth, pFrameInfo.nHeight, pData);
                }
                else
                {                                                                               //第二种模式更加底层，适用与所有图像格式。不过效率较慢
                    m_Image = new HalconDotNet.HImage("byte", pFrameInfo.nWidth, pFrameInfo.nHeight, pData);
                    uint nWidth = pFrameInfo.nWidth;

                    uint nHeight = pFrameInfo.nHeight;

                    Marshal.Copy(pData, m_pBufForSaveImage, 0, (int)nWidth * (int)nHeight * 3);

                    HObject Hobj = new HObject();
                    UInt32 nSupWidth = (pFrameInfo.nWidth + (UInt32)3) & 0xfffffffc;
                    Int32 nLength = (Int32)pFrameInfo.nWidth * (Int32)pFrameInfo.nHeight;

                    byte[] m_pDataForRed = new byte[(int)nWidth * (int)nHeight];
                    byte[] m_pDataForGreen = new byte[(int)nWidth * (int)nHeight];
                    byte[] m_pDataForBlue = new byte[(int)nWidth * (int)nHeight];

                    for (int nRow = 0; nRow < pFrameInfo.nHeight; nRow++)
                    {
                        for (int col = 0; col < pFrameInfo.nWidth; col++)
                        {
                            m_pDataForRed[nRow * nSupWidth + col] = m_pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col)];
                            m_pDataForGreen[nRow * nSupWidth + col] = m_pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col + 1)];
                            m_pDataForBlue[nRow * nSupWidth + col] = m_pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col + 2)];
                        }
                    }

                    IntPtr RedPtr = BytesToIntptr(m_pDataForRed);
                    IntPtr GreenPtr = BytesToIntptr(m_pDataForGreen);
                    IntPtr BluePtr = BytesToIntptr(m_pDataForBlue);

                    m_Image.GenImage3((HTuple)"byte", pFrameInfo.nWidth, pFrameInfo.nHeight, (new HTuple(RedPtr)), (new HTuple(GreenPtr)), (new HTuple(BluePtr)));

                }
                eventWait.Set();    //这里可能是运行后产生一个型号出去，让别人自动当前有图像被抓取到了
                if (dispImgCallback != null)
                {
                    dispImgCallback(m_Image, 0);
                }
            }
            catch (System.Exception ex)
            {
                eventWait.Set();
            }
        }

        /// <summary>
        /// 辅助函数，Intptr转Bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static IntPtr BytesToIntptr(byte[] bytes)
        {
            int size = bytes.Length;
            IntPtr buffer = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, buffer, size);
            return buffer;
        }

        /// <summary>
        /// 设置曝光时间
        /// </summary>
        /// <param name="dValue"></param>
        /// <returns></returns>
        public override bool setExposureTime(double dValue)
        {
            m_ExposeTime = (float)dValue;
            int nRet = m_pMyCamera.MV_CC_SetFloatValue_NET("ExposureTime", m_ExposeTime);
            if (nRet != MyCamera.MV_OK)
            {
                ShowErrorMsg("Set Exposure Time Fail!", nRet);
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 设置触发模式
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public override bool SetTriggerMode(TRIGGER_MODE mode)
        {
            int nRet = 0;
            if (mode == TRIGGER_MODE.硬件触发)
                nRet += m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0);
            else
                nRet += m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 1);
            // ch:触发源选择:0 - Line0; | en:Trigger source select:0 - Line0;
            //           1 - Line1;
            //           2 - Line2;
            //           3 - Line3;
            //           4 - Counter;
            //           7 - Software;
            switch (mode)
            {
                case TRIGGER_MODE.硬件触发:   // no acquisition
                    break;
                case TRIGGER_MODE.软件触发:   // freerunning
                    {
                        nRet += m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerSource", 7);
                        break;
                    }
                case TRIGGER_MODE.上升沿:   // Software trigger
                    {
                        nRet += m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerSource", 0);
                        nRet += m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerActivation", 0);
                        break;
                    }
                case TRIGGER_MODE.下降沿:   // Software trigger
                    {
                        nRet += m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerSource", 0);
                        nRet += m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerActivation", 1);
                        break;
                    }
            }
            if (nRet != MyCamera.MV_OK)
                return false;
            else
                return true;
        }

        /// <summary>
        /// 采集图像
        /// </summary>
        /// <param name="byHand">是否手动采图</param>
        public override bool CaptureImage(bool byHand)
        {
            try
            {
                int nRet = 0;
                if (_TrigerMode == 1)   //设置为软触发模式
                {
                    nRet = m_pMyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
                }
                else
                {
                    //nRet = m_pMyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 设置相机参数
        /// </summary>
        public override void setSetting()
        {
            int nRet = 0;
            //设置采集模式
            SetTriggerMode((TRIGGER_MODE)_TrigerMode);
            //设置曝光时间
            nRet = m_pMyCamera.MV_CC_SetFloatValue_NET("ExposureTime", m_ExposeTime);
            //置帧率
            //nRet = m_pMyCamera.MV_CC_SetFloatValue_NET("AcquisitionFrameRate", m_Framerate);
            //设置ip
            //apiReturn = myApi.Gige_Camera_setIPAddress(m_Handle, uint.Parse(_UniqueLabel), uint.Parse(_DevDirExt));
            base.setSetting();
        }

        /// <summary>
        /// 获取相机参数
        /// </summary>
        public override void getSetting()
        {
            int nRet = 0;
            MyCamera.MVCC_ENUMVALUE stTriggerMode = new MyCamera.MVCC_ENUMVALUE();
            MyCamera.MVCC_ENUMVALUE stTriggerSource = new MyCamera.MVCC_ENUMVALUE();
            MyCamera.MVCC_ENUMVALUE stTriggerActivation = new MyCamera.MVCC_ENUMVALUE();
            nRet = m_pMyCamera.MV_CC_GetTriggerMode_NET(ref stTriggerMode);
            nRet = m_pMyCamera.MV_CC_GetTriggerSource_NET(ref stTriggerSource);

            if (stTriggerMode.nCurValue == (uint)TRIGGER_MODE.硬件触发)
                _TrigerMode = (int)TRIGGER_MODE.硬件触发;
            else if (stTriggerSource.nCurValue == 7)//软触发
                _TrigerMode = (int)TRIGGER_MODE.软件触发;
            else if (stTriggerSource.nCurValue == 0) //Line0 触发
            {
                nRet += m_pMyCamera.MV_CC_GetEnumValue_NET("TriggerActivation", ref stTriggerActivation);
                if (stTriggerActivation.nCurValue == 0)
                    _TrigerMode = (int)TRIGGER_MODE.上升沿;
                else
                    _TrigerMode = (int)TRIGGER_MODE.下降沿;
            }

            MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            nRet = m_pMyCamera.MV_CC_GetFloatValue_NET("ExposureTime", ref stParam);
            _ExposeTime = stParam.fCurValue;
            m_pMyCamera.MV_CC_GetFloatValue_NET("ResultingFrameRate", ref stParam);
            _Framerate = stParam.fCurValue;
            base.getSetting();
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="filePath"></param>
        public override void SaveSetting(string filePath)
        {
            m_pMyCamera.MV_CC_FeatureSave_NET(filePath);
        }

        /// <summary>
        /// 加载设置
        /// </summary>
        /// <param name="filePath"></param>
        public override void LoadSetting(string filePath)
        {
            if (File.Exists(filePath))
                m_pMyCamera.MV_CC_FeatureLoad_NET(filePath);
        }

        /// <summary>
        /// 开始实时采集
        /// </summary>
        public void startGrab()
        {
            try
            {
                m_pMyCamera.MV_CC_StartGrabbing_NET();
            }
            catch { }
        }

        /// <summary>
        /// 停止实时采集
        /// </summary>
        public void stopGrab()
        {
            try
            {
                m_pMyCamera.MV_CC_StopGrabbing_NET();
            }
            catch { }

        }

        [OnDeserializing()]
        internal void OnDeSerializingMethod(StreamingContext context)
        {
            m_pMyCamera = new MyCamera();
        }
        [OnDeserialized()]
        internal void OnDeSerializedMethod(StreamingContext context)
        {
            FindCBySN(m_SerialNo);

        }

        #region HKVisionHelper

        /// <summary>
        /// /初始化时发现设备
        /// </summary>
        /// <param name="Ctemp"></param>
        public void FindCBySN(string Ctemp)
        {
            MyCamera.MV_CC_DEVICE_INFO_LIST m_pDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            int nRet;
            // ch:创建设备列表 en:Create Device List
            System.GC.Collect();
            nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_pDeviceList);
            if (0 != nRet)
            {
                MessageBox.Show("没有找到任何设备,请确认相机是否连接好");
                return;
            }

            // ch:在窗体列表中显示设备名 | en:Display device name in the form list
            for (int i = 0; i < m_pDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    if (Ctemp == gigeInfo.chSerialNumber)//判断是否等于指定相机序号
                    {
                        CurDevice = device;
                        return;
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    if (Ctemp == usbInfo.chSerialNumber)//判断是否等于指定相机序号
                        if (Ctemp == usbInfo.chSerialNumber)//判断是否等于指定相机序号
                        {
                            CurDevice = device;
                            return;
                        }

                }
            }
            MessageBox.Show("没有找当前到设备,请确认相机是否连接好");
            return;
        }

        /// <summary>
        /// ch:显示错误信息 | en:Show error message
        /// </summary>
        /// <param name="csMessage"></param>
        /// <param name="nErrorNum"></param>
        private void ShowErrorMsg(string csMessage, int nErrorNum)
        {
            string errorMsg;
            if (nErrorNum == 0)
            {
                errorMsg = csMessage;
            }
            else
            {
                errorMsg = csMessage + ": Error =" + String.Format("{0:X}", nErrorNum);
            }

            switch (nErrorNum)
            {
                case MyCamera.MV_E_HANDLE: errorMsg += " Error or invalid handle "; break;
                case MyCamera.MV_E_SUPPORT: errorMsg += " Not supported function "; break;
                case MyCamera.MV_E_BUFOVER: errorMsg += " Cache is full "; break;
                case MyCamera.MV_E_CALLORDER: errorMsg += " Function calling order error "; break;
                case MyCamera.MV_E_PARAMETER: errorMsg += " Incorrect parameter "; break;
                case MyCamera.MV_E_RESOURCE: errorMsg += " Applying resource failed "; break;
                case MyCamera.MV_E_NODATA: errorMsg += " No data "; break;
                case MyCamera.MV_E_PRECONDITION: errorMsg += " Precondition error, or running environment changed "; break;
                case MyCamera.MV_E_VERSION: errorMsg += " Version mismatches "; break;
                case MyCamera.MV_E_NOENOUGH_BUF: errorMsg += " Insufficient memory "; break;
                case MyCamera.MV_E_UNKNOW: errorMsg += " Unknown error "; break;
                case MyCamera.MV_E_GC_GENERIC: errorMsg += " General error "; break;
                case MyCamera.MV_E_GC_ACCESS: errorMsg += " Node accessing condition error "; break;
                case MyCamera.MV_E_ACCESS_DENIED: errorMsg += " No permission "; break;
                case MyCamera.MV_E_BUSY: errorMsg += " Device is busy, or network disconnected "; break;
                case MyCamera.MV_E_NETER: errorMsg += " Network error "; break;
            }

            MessageBox.Show(errorMsg, "PROMPT");
        }

        #endregion
    }
}
