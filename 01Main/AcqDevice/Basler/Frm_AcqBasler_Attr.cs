using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HalconDotNet;
//这一层代码实现了对巴斯勒相机的底层封装，全部变成了接口

namespace AcqDevice
{
    public partial class Frm_AcqBasler_Attr : Form
    {
        //Frm_AcqBasler_Attr frm_AcqBasler_Attr;  //定义一个自己的对象
        AcqBasler m_AcqBasler;

        ////private HalconControl.HWindow_Fit hWindow = null;

        ////public HalconControl.HWindow_Fit m_HWindow
        ////{

        ////    set
        ////    {
        ////        hWindow = value;
        ////        if (hWindow != null)
        ////        {
        ////            m_AcqBasler.dispImgCallback = dispImage;
        ////        }
        ////    }
        ////}


        ///public static Frm_AcqBasler_Attr thisHandle;  //由主程序赋值 magical 20171122 用以嵌入到主程序使用


        public Frm_AcqBasler_Attr(ref AcqDeviceBase _AcqDevice)
        {
            InitializeComponent();
            m_AcqBasler = (AcqBasler)_AcqDevice;
        }

        public Frm_AcqBasler_Attr()
        {
            InitializeComponent();
            m_AcqBasler.dispImgCallback = null;
        }

        private void Frm_AcqBasler_Attr_Load(object sender, EventArgs e)
        {
            ///Frm_AcqBasler_Attr.thisHandle = this;

            if (m_AcqBasler.m_bConnected)
            {
                m_AcqBasler.getSetting();
            }

            txt_ExposeTime.Text = m_AcqBasler.m_ExposeTime.ToString();
            cmb_TrigerMode.SelectedIndex = m_AcqBasler.m_TrigerMode;


        }

        private void bt_Save_Click(object sender, EventArgs e)
        {
            m_AcqBasler.m_ExposeTime = float.Parse(txt_ExposeTime.Text.Trim());
            m_AcqBasler.m_TrigerMode = cmb_TrigerMode.SelectedIndex;
            m_AcqBasler.setSetting();
            m_AcqBasler.SaveSetting(m_AcqBasler.m_SettingPath);
            this.Close();
        }

        private void bt_Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 触发方式下拉框选择改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmb_TrigerMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            //m_AcqBasler.stopGrab(); //每次切换都尝试关闭相机
            bt_solftTrigger.Enabled = false;
            m_AcqBasler.m_TrigerMode = cmb_TrigerMode.SelectedIndex;
            m_AcqBasler.setSetting();
            if (cmb_TrigerMode.SelectedIndex == 1)
            {
                bt_solftTrigger.Enabled = true;
            }
        }

        /// <summary>
        /// 软触发一次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_solftTrigger_Click(object sender, EventArgs e)
        {
            //定义曝光时间
            double _ExposeTime;
            //str转double
            string str = txt_ExposeTime.Text;
            if (double.TryParse(str, out _ExposeTime))
            {
                _ExposeTime = Double.Parse(str);
            }
            if(_ExposeTime != 0)
            {
                m_AcqBasler.setExposureTime(_ExposeTime);   //设置曝光时间
            }
            

            m_AcqBasler.CaptureImage(true); //最终的采集事件
        }

        /// <summary>
        /// 设置曝光时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bt_SetExposeTime_Click(object sender, EventArgs e)
        {
            //定义曝光时间
            double _ExposeTime;
            //str转double
            string str = txt_ExposeTime.Text;
            if (double.TryParse(str, out _ExposeTime))
            {
                _ExposeTime = Double.Parse(str);
            }
            if (_ExposeTime != 0)
            {
                m_AcqBasler.setExposureTime(_ExposeTime);   //设置曝光时间
            }

            //m_AcqBasler.stopGrab(); //每次切换都尝试关闭相机
            m_AcqBasler.setSetting();
        }

        /// <summary>
        /// 测试采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bt_StartTestAcq_Click(object sender, EventArgs e)
        {
            cmb_TrigerMode.Enabled = Bt_SetExposeTime.Enabled = bt_solftTrigger.Enabled = Bt_StartTestAcq.Enabled = bt_Save.Enabled = bt_Exit.Enabled = false;
            m_AcqBasler.startGrab();    //开始采集
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bt_StopTestAcq_Click(object sender, EventArgs e)
        {
            cmb_TrigerMode.Enabled = Bt_SetExposeTime.Enabled = bt_solftTrigger.Enabled = Bt_StartTestAcq.Enabled = bt_Save.Enabled = bt_Exit.Enabled = true;
            m_AcqBasler.stopGrab();    //开始采集
        }

        /// <summary>
        /// 相机最终的显示函数可以在这里找到！！！
        /// </summary>
        /// <param name="image"></param>
        /// <param name="user"></param>
        private void dispImage(HImage image, int user)
        {
            ////hWindow.Image = image;
            //////int width, height;
            //////image.GetImageSize(out width, out height);
            //////hWindow.SetPart(0, 0, height, width);
            //////hWindow.HWindowID.DispImage(image);
            ////hWindow_Fit1.Image = image;     //这里就可以实现把采集的图像显示在当前窗体
            ////hWindow_Fit1.DispImageFit();
        }


    }
}
