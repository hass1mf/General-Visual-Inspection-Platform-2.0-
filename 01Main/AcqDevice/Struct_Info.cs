using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Struct_Info:结构信息  该类单独存在，没有其他引用
//CamInfo 定义了相机类的基本信息

namespace AcqDevice
{
    /// <summary>
    /// 相机信息
    /// </summary>
    [Serializable]
    public struct CamInfo
    {
        //private int _CamHandle;//相机句柄
        private string _CamName;
        private string _Manufacturer;
        private string _SerialNO;
        private string _UniqueName;//IP
        private string _MaskName;//mask
        private int _ImageWidth;//宽度
        private int _ImageHeigh;
        private int _PixDepth;
        private string _Color;/// BW 黑白，RGB彩色
        private bool _bConnected;
        public object mExtInfo;
        //public int m_CamHandle
        //{
        //    set { _CamHandle = value; }
        //    get { return _CamHandle; }
        //}
        public string m_CamName
        {
            set { _CamName = value; }
            get { return _CamName; }
        }

        public string m_Manufacturer
        {
            set { _Manufacturer = value; }
            get { return _Manufacturer; }
        }

        public string m_SerialNO
        {
            set { _SerialNO = value; }
            get { return _SerialNO; }
        }

        public string m_UniqueName
        {
            set { _UniqueName = value; }
            get { return _UniqueName; }
        }

        public string m_MaskName
        {
            set { _MaskName = value; }
            get { return _MaskName; }
        }

        public int m_ImageWidth
        {
            set { _ImageWidth = value; }
            get { return _ImageWidth; }
        }

        public int m_ImageHeigh
        {
            set { _ImageHeigh = value; }
            get { return _ImageHeigh; }
        }

        public int m_PixDepth
        {
            set { _PixDepth = value; }
            get { return _PixDepth; }
        }

        public string m_Color
        {
            set { _Color = value; }
            get { return _Color; }
        }

        public bool m_bConnected
        {
            set { _bConnected = value; }
            get { return _bConnected; }
        }
    }
}
