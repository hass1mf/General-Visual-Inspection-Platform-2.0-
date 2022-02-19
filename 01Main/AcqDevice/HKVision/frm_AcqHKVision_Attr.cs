using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AcqDevice;
using HalconDotNet;
//这一层代码实现了对海康相机的底层封装，全部变成了接口

namespace AcqDevice
{
    public partial class frm_AcqHKVision_Attr : Form
    {

        AcqHKVision m_AcqHKVision;    //实例m_AcqHKVision类
        ////private HalconControl.HWindow_Fit hWindow = null;
        ////public HalconControl.HWindow_Fit m_HWindow
        ////{
        ////    set
        ////    {
        ////        hWindow = value;
        ////        if (hWindow != null)
        ////        {
        ////            m_AcqHKVision.dispImgCallback = dispImage;    //通过dispImage函数来传图像
        ////        }
        ////    }
        ////}
        public frm_AcqHKVision_Attr(ref AcqDeviceBase _AcqDevice)
        {
            InitializeComponent();
            m_AcqHKVision = (AcqHKVision)_AcqDevice;  //这里可能也是一种实例化的过程
        }

        ~frm_AcqHKVision_Attr()
        {
            m_AcqHKVision.dispImgCallback = null;
        }

        /// <summary>
        /// 窗体初始化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frm_AcqHKVision_Attr_Load(object sender, EventArgs e)
        {
            if (m_AcqHKVision.m_bConnected)
            {
                m_AcqHKVision.getSetting();
            }
            txt_ExposeTime.Text = m_AcqHKVision.m_ExposeTime.ToString();
            //txt_Framerate.Text = m_AcqHKVision.m_Framerate.ToString();    //当前帧率
            cmb_TrigerMode.SelectedIndex = m_AcqHKVision.m_TrigerMode;
        }


        #region 窗体按钮事件

        /// <summary>
        /// 按钮--设置曝光时间
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
                m_AcqHKVision.setExposureTime(_ExposeTime);   //设置曝光时间
            }
            m_AcqHKVision.m_ExposeTime = (float)_ExposeTime;  //m_ExposeTime才是定义的标准参数

            m_AcqHKVision.stopGrab();  //每次切换都尝试关闭相机
            m_AcqHKVision.setSetting();   //再保存下曝光模式

        }

        /// <summary>
        /// 按钮--软触发一次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_solftTrigger_Click(object sender, EventArgs e)
        {
            //m_AcqHKVision.startGrab();    //开始采集
            ////定义曝光时间
            //double _ExposeTime;
            ////str转double
            //string str = txt_ExposeTime.Text;
            //if (double.TryParse(str, out _ExposeTime))
            //{
            //    _ExposeTime = Double.Parse(str);
            //}
            //m_AcqHKVision.setExposureTime(_ExposeTime);   //设置曝光时间

            //m_AcqHKVision.CaptureImage(true);
            //m_AcqHKVision.SignalWait.WaitOne();
            //m_AcqHKVision.stopGrab();    //停止采集
            //定义曝光时间
            double _ExposeTime;
            //str转double
            string str = txt_ExposeTime.Text;
            if (double.TryParse(str, out _ExposeTime))
            {
                _ExposeTime = Double.Parse(str);
            }
            m_AcqHKVision.setExposureTime(_ExposeTime);   //设置曝光时间

            m_AcqHKVision.CaptureImage(true);
        }

        /// <summary>
        /// 按钮--测试采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bt_StartTestAcq_Click(object sender, EventArgs e)
        {
            cmb_TrigerMode.Enabled = Bt_SetExposeTime.Enabled = bt_solftTrigger.Enabled = Bt_StartTestAcq.Enabled = bt_Save.Enabled = bt_Exit.Enabled = false;
            m_AcqHKVision.startGrab();    //开始采集
            //m_AcqHKVision.CaptureImage(false);
        }

        /// <summary>
        /// 按钮--停止采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bt_StopTestAcq_Click(object sender, EventArgs e)
        {
            cmb_TrigerMode.Enabled = Bt_SetExposeTime.Enabled = bt_solftTrigger.Enabled = Bt_StartTestAcq.Enabled = bt_Save.Enabled = bt_Exit.Enabled = true;
            m_AcqHKVision.stopGrab();    //停止采集
        }

        /// <summary>
        /// 按钮--确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_Save_Click(object sender, EventArgs e)
        {
            m_AcqHKVision.m_ExposeTime = float.Parse(txt_ExposeTime.Text.Trim());
            m_AcqHKVision.m_TrigerMode = cmb_TrigerMode.SelectedIndex;
            m_AcqHKVision.setSetting();
            m_AcqHKVision.SaveSetting(m_AcqHKVision.m_SettingPath);
            this.Close();
        }

        /// <summary>
        /// 按钮--取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion


        /// <summary>
        /// 触发模式下拉框选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmb_TrigerMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            //m_AcqHKVision.stopGrab();
            bt_solftTrigger.Enabled = false;
            m_AcqHKVision.m_TrigerMode = cmb_TrigerMode.SelectedIndex;
            m_AcqHKVision.setSetting();
            if (cmb_TrigerMode.SelectedIndex == 1)
            {
                bt_solftTrigger.Enabled = true;
            }
        }

        /// <summary>
        /// 显示函数，这里可以实现最终图像的显示
        /// </summary>
        /// <param name="image"></param>
        /// <param name="user"></param>
        private void dispImage(HImage image, int user)
        {
            ////hWindow.Image = image;
            //int width, height;
            //image.GetImageSize(out width, out height);
            //hWindow.SetPart(0, 0, height, width);
            //hWindow.HWindowID.DispImage(image);
            ////hWindow_Fit1.Image = image;
        }


    }
}
