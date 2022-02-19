using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HalconDotNet;
using System.IO;
//AcqFolder_Area是采集设备以文件夹的方式运行

namespace AcqDevice
{
    [Serializable]
    public class AcqFolder_Area : AcqAreaDeviceBase
    {
        private int index = 0;
        [NonSerialized]
        private FileInfo[] m_FileList;
        public AcqFolder_Area(DeviceType _DeviceType) : base(_DeviceType)
        {
        }

        public static void SearchCameras(string path, out List<CamInfo> m_CamInfoList)
        {
            m_CamInfoList = new List<CamInfo>();
            CamInfo _CamInfo = new CamInfo();
            _CamInfo.m_UniqueName = path;
            _CamInfo.m_SerialNO = path;
            m_CamInfoList.Add(_CamInfo);
        }

        public override void ConnectDev()//建立设备连接
        {
            try
            {
                index = 0;
                DirectoryInfo di = new DirectoryInfo(m_SerialNo);
                m_FileList = di.GetFiles();
                m_bConnected = true;
            }
            catch (System.Exception ex)
            {
                m_bConnected = false;
                System.Windows.Forms.MessageBox.Show(m_DeviceID + "采集设备路劲不存在");
            }
        }

        public override void DisConnectDev() //断开设备连接
        {
            m_bConnected = false;
            index = 0;
        }

        public override bool CaptureImage(bool byHand)
        {
            try
            {
                while (true)
                {
                    if (index >= m_FileList.Length)
                        index = 0;
                    string ext = Path.GetExtension(m_FileList[index].Name).ToUpper();
                    string sFullName = m_FileList[index].FullName;
                    index++;
                    //判断当前文件后缀名是否与给定后缀名一样
                    if (ext == ".HE" || ext == ".BMP" || ext == ".JPG" || ext == ".PNG" || ext == ".TIFF")
                    {
                        if (!File.Exists(sFullName))
                        {
                            return false;
                        }
                        m_Image = new HImage(sFullName);
                        //m_Image.ReadImage(sFullName);
                        break;
                    }
                }
                //使用委托传递出去
                eventWait.Set();
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        public override void setSetting()//设置
        {
        }

        public override void getSetting()//设置
        {
        }

    }
}
