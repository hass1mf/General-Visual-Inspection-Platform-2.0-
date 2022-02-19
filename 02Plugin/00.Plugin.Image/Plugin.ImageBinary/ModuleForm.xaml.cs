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

namespace Plugin.ImageBinary
{
    /// <summary>
    /// ModuleForm.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleForm : ModuleFormBase
    {
        private ModuleObj m_ModuleObj;  //ModuleForm类下的私有成员，可在类内部调用
        private List<CanvasItem> CanvasList = new List<CanvasItem>();
        public HTuple hv_ExpDefaultWinHandle;
        public HObject Ho_Image0;   //定义最后的灰度图像
        public static HObject H_Image;  //静态变量H_Image

        public ModuleForm()
        {
            InitializeComponent();
        }

        public override void LoadModule()   //画布选择
        {
            m_ModuleObj = (ModuleObj)ModuleObjBase;

            Title = m_ModuleObj.Info.ModuleName;

          

            CanvasList.Clear();
            CanvasList.Add(new CanvasItem { Name = "不显示", Index = -1 });
            for (int i = 0; i < CanvasCount; ++i)
            {
                CanvasList.Add(new CanvasItem { Name = "画布" + (i + 1).ToString(), Index = i });
            }
            _displayCanvas.ItemsSource = CanvasList;
            _displayCanvas.DisplayMemberPath = "Name";
            _displayCanvas.SelectedValuePath = "Index";

            if ((m_ModuleObj.CanvasIndex < -1) || (m_ModuleObj.CanvasIndex >= CanvasCount))
            {
                m_ModuleObj.CanvasIndex = -1;
            }
            _displayCanvas.SelectedValue = m_ModuleObj.CanvasIndex;
        }

        public override void RunModuleBefore()
        {

        }
       
        //点击执行按钮
        public override void RunModuleAfter()   
        {
            if (m_ModuleObj.Ho_Image != null)
            {
                _hWinDisplay.ClearHWindow();
                _hWinDisplay.Image = m_ModuleObj.Ho_Image;  //直接传入HObject
            }
            else
            {
                _hWinDisplay.ClearHWindow();
            }
        }

        public override void SaveModuleBefore()
        {
            m_ModuleObj.CanvasIndex = (int)_displayCanvas.SelectedValue;    //储存画布
        }

        public void GetPath(System.Windows.Controls.TextBox textBox)    //加载文件路径
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
            GatherGray(_hWinDisplay.HWindowID);                    
        }

        private void GatherGray(HTuple Window)  //函数只需传入一个Window
        {
            hv_ExpDefaultWinHandle = Window;    //统一的Window接口

            // Local iconic variables 
            HImage Gray = new HImage();     //定义HImage类型的Gray变量，并实例化
            // Local control variables 
            HOperatorSet.GenEmptyObj(out Ho_Image0);    //创建Ho_Image0

            HOperatorSet.Rgb1ToGray(H_Image, out Ho_Image0);  //HOperatorSet的方法只能对HObject对象进行操作

            HobjectToHimage(  Ho_Image0,ref Gray);      //转换格式，输出Gray
            m_ModuleObj.Ho_Image = Gray;
            HOperatorSet.DispObj(m_ModuleObj.Ho_Image, hv_ExpDefaultWinHandle);     //hv_ExpDefaultWinHandle画布上显示         
        }
   
        //HObject转HImage函数  
        private void HobjectToHimage(HObject hobject, ref HImage image)
        {
            HTuple pointer, type, width, height;
            HOperatorSet.GetImagePointer1(hobject, out pointer, out type, out width, out height);
            image.GenImage1(type, width, height, pointer);
        }
    }

    class CanvasItem    //画布相关
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }
}
