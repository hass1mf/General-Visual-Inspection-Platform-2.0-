using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using HalconDotNet;
using Heart.Inward;
using Heart.Outward;

namespace Plugin.ImageGraytesting
{
    /// <summary>
    /// ModuleForm.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleForm : ModuleFormBase
    {
        private ModuleObj m_ModuleObj;  //ModuleForm类下的私有成员，可在类内部调用
        private List<CanvasItem> CanvasList = new List<CanvasItem>();
        public HTuple hv_ExpDefaultWinHandle;
        public static HObject H_Image;  //静态变量H_Image

        public ModuleForm()
        {
            InitializeComponent();
        }

        public override void LoadModule()
        {
            m_ModuleObj = (ModuleObj)ModuleObjBase;

            Title = m_ModuleObj.Info.ModuleName;
        }

        public override void RunModuleBefore()
        {

        }
        public override void RunModuleAfter()
        {
            if (m_ModuleObj.hv_img != null)
            {
                _hWinDisplay.ClearHWindow();
                _hWinDisplay.Image = (HImage)m_ModuleObj.hv_img;
            }
            else
            {
                _hWinDisplay.ClearHWindow();
            }
        }

        public override void SaveModuleBefore()
        {
        //  m_ModuleObj.CanvasIndex = (int)_displayCanvas.SelectedValue;    //储存画布
        }

        public void GetPath(System.Windows.Controls.TextBox textBox)
        {

        }

        
        private void GetImage_Click(object sender, RoutedEventArgs e)
        {
            VariableTable variableTable = new VariableTable();    //打开变量表
            variableTable.SetModuleInfo(m_ModuleObj.Info.ProjectID, m_ModuleObj.Info.ModuleName, "Image");

            //打开并维持窗体
            if (variableTable.ShowDialog() == true)
            {
                //数据操作部分
                m_ModuleObj.hv_img_Str = variableTable.SelectedVariableText;
                m_ModuleObj.ReadImg();
                _hWinDisplay.ClearHWindow();
                _hWinDisplay.Image = (HImage)m_ModuleObj.hv_img;
                H_Image = m_ModuleObj.hv_img;
            }
            
        }
        //按钮触发事件
        private void GrayImage_Click(object sender, RoutedEventArgs e)
        {
            if(H_Image != null)
            {
                GatherGray(_hWinDisplay.HWindowID, txbGray);
            }      
        }

        private void GatherGray(HTuple Window, TextBlock txbGray)
        {
            hv_ExpDefaultWinHandle = Window;    //统一的Window接口

            _hWinDisplay.ShieldMouseEvent();    //绘制ROI前，先屏蔽掉控件的鼠标事件

            HObject ho_Image1, ho_Image, ho_Rectangle;
            HTuple hv_Row1 = null, hv_Column1 = null, hv_Row2 = null, hv_Column2 = null;    //定义矩形四点
            HTuple hv_Mean = null, hv_Deviation = null;

            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Image1);
            HOperatorSet.GenEmptyObj(out ho_Image);
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            //读取图片，转换成灰度图片
            //ho_Image1 = _hWinDisplay.HWindowID.Image;

            //ho_Image1 = (HImage)m_ModuleObj.hv_img;  //有问题

            //HOperatorSet.ReadImage(out H_Image, "D:/Document And Settings3/123/Desktop/机器视觉记录/test photo/test4.jpg");
            
            HOperatorSet.DispObj(H_Image, hv_ExpDefaultWinHandle);     //hv_ExpDefaultWinHandle画布上显示
            
            //选择测量的区域
            HOperatorSet.DrawRectangle1(hv_ExpDefaultWinHandle, out hv_Row1, out hv_Column1, out hv_Row2, out hv_Column2);      //绘制矩形
            ho_Rectangle.Dispose();
            HOperatorSet.GenRectangle1(out ho_Rectangle, hv_Row1, hv_Column1, hv_Row2, hv_Column2);

            //检测灰度
            HOperatorSet.Intensity(ho_Rectangle, H_Image, out hv_Mean, out hv_Deviation);
            HOperatorSet.DispObj(H_Image, hv_ExpDefaultWinHandle);
            //MessageBox.Show(hv_Mean.ToString());
            txbGray.Text = ((double)hv_Mean).ToString("0.00");

            _hWinDisplay.ReloadMouseEvent();    //等待ROI绘制任务完成后就恢复鼠标的事件

            //释放资源
            ho_Image.Dispose();
            ho_Rectangle.Dispose(); 
           
        }
    }

    class CanvasItem
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }
}
