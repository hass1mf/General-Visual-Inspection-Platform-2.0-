using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Basler.Pylon;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.Serialization;

namespace AcqDevice
{
    /// <summary>
    /// magical 20171027
    /// </summary>
    [Serializable]
    public class AcqBasler : AcqAreaDeviceBase
    {
        [NonSerialized]
        private Camera camera;
        [NonSerialized]
        private Stopwatch stopWatch = new Stopwatch();
        [NonSerialized]
        private PixelDataConverter converter = new PixelDataConverter();

        public AcqBasler(DeviceType _DeviceType) : base(_DeviceType)
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
            List<ICameraInfo> allCameras = CameraFinder.Enumerate();

            foreach (ICameraInfo item in allCameras)
            {
                CamInfo _camInfo = new CamInfo();
                _camInfo.m_CamName = "basler" + item[CameraInfoKey.SerialNumber];
                _camInfo.m_SerialNO = item[CameraInfoKey.SerialNumber];
                _camInfo.m_UniqueName = "basler" + item[CameraInfoKey.SerialNumber];
                _camInfo.m_MaskName = "basler" + item[CameraInfoKey.SerialNumber];
                m_CamInfoList.Add(_camInfo);
            }
        }

        /// <summary>
        /// 连接相机
        /// </summary>
        public override void ConnectDev()
        {
            try
            {
                // 如果设备已经连接先断开
                DisConnectDev();
                base.ConnectDev();


                camera = new Camera(this.m_SerialNo);
                camera.Open();
                camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                camera.ConnectionLost += OnConnectionLost;
                camera.StreamGrabber.GrabStarted += OnGrabStarted;
                camera.StreamGrabber.GrabStopped += OnGrabStopped;
                m_ExposeTime = (float)camera.Parameters[PLCamera.ExposureTimeAbs].GetValue();

                m_bConnected = true;
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
                    camera.Close();
                    camera.Dispose();
                    if (camera != null)
                    {
                        camera.Close();
                        camera.Dispose();
                        camera = null;
                        m_bConnected = false;
                    }
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
                camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(dValue);
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
                //如果是实时采集 则先停止
                if (camera.StreamGrabber.IsGrabbing && camera.Parameters[PLCamera.AcquisitionMode].GetValue() == PLCamera.AcquisitionMode.Continuous)
                {
                    stopGrab();
                }

                switch (mode)
                {
                    case TRIGGER_MODE.硬件触发:   // no acquisition
                        {
                            camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
                            camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Line1);
                            camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerActivation.AnyEdge);
                            //startGrab();      //另外写了采集开始，所以这里先不启动
                            break;
                        }
                    case TRIGGER_MODE.软件触发:   // freerunning
                        {
                            camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.Off);
                            camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Software);
                            break;
                        }
                    case TRIGGER_MODE.上升沿:   // Software trigger
                        {
                            camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
                            camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Line1);
                            camera.Parameters[PLCamera.TriggerActivation].SetValue(PLCamera.TriggerActivation.RisingEdge);
                            //startGrab();
                            break;
                        }
                    case TRIGGER_MODE.下降沿:   // Software trigger
                        {
                            camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
                            camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Line1);
                            camera.Parameters[PLCamera.TriggerActivation].SetValue(PLCamera.TriggerActivation.FallingEdge);
                            //startGrab();
                            break;
                        }
                }

                return true;
            }
            catch (Exception)
            {

                return false;
            }




        }

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
            try
            {              
                if (_TrigerMode == 0)   //设置为软触发模式
                {
                    //SignalWait.WaitOne();
                    //拍一张
                    camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                    camera.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                    //SignalWait.WaitOne();   //WaitOne类似于一直等待采集成功信号进来，不然就一直等待
                }
                else   //设置为上升沿硬触发,等3种触发模式
                {
                    if (camera.StreamGrabber.IsGrabbing)    //三种硬触发都是在设置的时候就启动的，所以在后面采集这里如果没开启的话，开启一次即可
                    {
                        //MessageBox.Show("相机当前正处于采集状态！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //return false;
                    }
                    else
                    {
                        //SignalWait.WaitOne();
                        camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                        camera.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);
                        stopWatch.Restart();    // ****  重启采集时间计时器   ****
                        return true;
                    }          
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());     //这里是相机采集的核心函数，值得加报错提示
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
            m_ExposeTime = (float)camera.Parameters[PLCamera.ExposureTimeAbs].GetValue();
            base.getSetting();
        }


        #region 相机事件
        /// <summary>
        /// 断开连接时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionLost(Object sender, EventArgs e)
        {
            // Close the camera object.
            DisConnectDev();
        }

        /// <summary>
        /// 开始触发时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGrabStarted(Object sender, EventArgs e)
        {
            // Reset the stopwatch used to reduce the amount of displayed images. The camera may acquire images faster than the images can be displayed.
            stopWatch.Reset();
        }

        // Occurs when a camera has stopped grabbing.
        private void OnGrabStopped(Object sender, GrabStopEventArgs e)
        {

            // Reset the stopwatch.
            stopWatch.Reset();

        }

        /// <summary>
        /// 采集回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnImageGrabbed(Object sender, ImageGrabbedEventArgs e)
        {
            try
            {

                // Acquire the image from the camera. Only show the latest image. The camera may acquire images faster than the images can be displayed.

                // Get the grab result.
                IGrabResult grabResult = e.GrabResult;

                // Check if the image can be displayed.
                if (grabResult.IsValid)
                {
                    // Reduce the number of displayed images to a reasonable amount if the camera is acquiring images very fast.
                    if (!stopWatch.IsRunning || stopWatch.ElapsedMilliseconds > 33)
                    {
                        stopWatch.Restart();
                        Bitmap bitmap = new Bitmap(grabResult.Width, grabResult.Height, PixelFormat.Format32bppRgb);
                        //// Lock the bits of the bitmap.
                        BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                        //// Place the pointer to the buffer of the bitmap.
                        //converter.OutputPixelFormat = PixelType.BGRA8packed;
                        IntPtr ptrBmp = bmpData.Scan0;
                        converter.Convert(ptrBmp, bmpData.Stride * bitmap.Height, grabResult); //Exception handling TODO
                        //bitmap.UnlockBits(bmpData);
                        //// EventSaveImageCallBack((Bitmap)bitmap.Clone());
                        _ImgWidth = grabResult.Width;
                        _ImgHeight = grabResult.Height;

                        m_Image.GenImage1("byte", _ImgWidth, _ImgHeight, ptrBmp);

                        if (dispImgCallback != null)
                        {
                            dispImgCallback(m_Image, 1);//1 无意义
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                //  ShowException(exception);
            }
            finally
            {
                //使用委托传递出去
                eventWait.Set();
                // Dispose the grab result if needed for returning it to the grab loop.
                e.DisposeGrabResultIfClone();
            }
        }
        #endregion


        /// <summary>
        /// 开始实时采集
        /// </summary>
        public void startGrab()
        {
            camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
            camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
        }

        /// <summary>
        /// 停止实时采集
        /// </summary>
        public void stopGrab()
        {
            try
            {
                camera.StreamGrabber.Stop();
            } catch { }
            
        }


        [OnDeserializing()]
        internal void OnDeSerializingMethod(StreamingContext context)
        {
            stopWatch = new Stopwatch();
            converter = new PixelDataConverter();
        }
    }
}
