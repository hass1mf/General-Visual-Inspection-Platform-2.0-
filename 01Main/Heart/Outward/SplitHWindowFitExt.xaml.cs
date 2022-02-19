using HalconDotNet;
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
//SplitHWindowFitExt控件用于主窗体的显示控件，其与HWindowFixExt联合。通过画布的数量来放置HWindowFixExt

namespace Heart.Outward
{
    /// <summary>
    /// SplitHWindowFitExt.xaml 的交互逻辑
    /// </summary>
    public partial class SplitHWindowFitExt : UserControl
    {
        public event DelegateRePaint RePaint;

        public List<HWindowFitExt> HWindowFitExtList = new List<HWindowFitExt>();   //HWindowFitExtList是包含HWindowFitExt个数信息的List，故其
        public SplitHWindowFitExt()
        {
            InitializeComponent();
            SetDisplayNumber(1);    //初始化先加载一个HWindowFitExt窗口，用于主页显示
        }

        public void SetDisplayNumber(int number)//设置要显示的画布数量
        {
            _gridImage.Children.Clear();
            _gridImage.RowDefinitions.Clear();
            _gridImage.ColumnDefinitions.Clear();
            HWindowFitExtList.Clear();

            int rows = 0;
            int cols = 0;

            switch (number)
            {
                case 1:
                    rows = 1;
                    cols = 1;
                    break;
                case 2:
                    rows = 1;
                    cols = 2;
                    break;
                case 4:
                    rows = 2;
                    cols = 2;
                    break;
                case 6:
                    rows = 2;
                    cols = 3;
                    break;
                default:
                    rows = 1;
                    cols = 1;
                    break;
            }

            for (int i = 0; i < rows; i++)
            {
                RowDefinition rd = new RowDefinition();
                _gridImage.RowDefinitions.Add(rd);
            }

            for (int j = 0; j < cols; j++)
            {
                ColumnDefinition cd = new ColumnDefinition();
                _gridImage.ColumnDefinitions.Add(cd);
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    HWindowFitExt hWindowFitExt = new HWindowFitExt();  //new并调用了HWindowFitExt窗体，根据选择的画布来添加HWindowFitExt
                    hWindowFitExt.DelegateDisplayEvent += DisplayOnly;
                    if (RePaint !=null)
                    {
                       hWindowFitExt.RePaint += RePaint;
                    }
                    Grid.SetRow(hWindowFitExt, i);
                    Grid.SetColumn(hWindowFitExt, j);
                    _gridImage.Children.Add(hWindowFitExt);
                    HWindowFitExtList.Add(hWindowFitExt);
                }
            }                
        }

        public int GetDisplayNumber()   //在MainWindow被调用
        {
            return HWindowFitExtList.Count();
        }

        public void SetDisplayImage(int index,HImage image)     //流程多模块需要显示时被调用
        {
            if (index < HWindowFitExtList.Count() && index > 0)
            {
                HWindowFitExtList[index].ClearHWindow();
                HWindowFitExtList[index].Image = image;
                //HWindowFitExtList[index].DispImageFit();    //添加的，用于加载图片时自适应窗口
            }
        }

        /// <summary>
        /// 设置要显示的图像和画布信息
        /// </summary>
        /// <param name="index"></param>
        /// <param name="image"></param>
        public void SetDisplayImage(int index, Object image)
        {
            if (index < HWindowFitExtList.Count() && index >= 0)
            {
                HWindowFitExtList[index].ClearHWindow();
                HWindowFitExtList[index].Image = image as HImage;
                HWindowFitExtList[index].HRegions.Clear();
            }
        }

        /// <summary>
        /// 20211019设置显示ROI
        /// </summary>
        /// <param name="index"></param>
        public void SetDisplayRoi(int index, List<MeasureROI> hRegions)
        {
            if (index < HWindowFitExtList.Count() && index >= 0)    //是否显示，系统会先判断当前的输出窗口索引是否超出当前的窗口数量。所以窗口变少也不会发生严重错误
            {
              //  HWindowFitExtList[index].HRegions.Clear();
                //HWindowFitExtList[index].ClearHWindow();  //对于ROI的显示，不能清空显示
                foreach (MeasureROI item in hRegions)   //对输入进来的RoiList进行循环遍历显示，List对应的就是当前的模块（模板匹配）的RoiList信息输出
                {
                    bool flag2 = item != null && item.roiType == enMeasureROIType.文字显示.ToString();
                    if (flag2)
                    {
                        if (!HWindowFitExtList[index].IsDispRoi)
                        {
                            MeasureROIText measureROIText = (MeasureROIText)item;
                            ImageTool.set_display_font(this.HWindowFitExtList[index].HWindowID, measureROIText.size, measureROIText.font, "false", "false");
                            ImageTool.disp_message(this.HWindowFitExtList[index].HWindowID, measureROIText.text, "image", measureROIText.row, measureROIText.col, measureROIText.drawColor, "false");
                            HWindowFitExtList[index].MeasureROITexts.Add(measureROIText);
                        }
                    }
                    else
                    {
                        if (!HWindowFitExtList[index].IsDispRoi)
                        {
                            HWindowFitExtList[index].HWindowID.SetColor(item.drawColor);
                            HWindowFitExtList[index].HWindowID.DispObj(item.hobject);
                            HWindowFitExtList[index].HRegions.Add(item.hobject);
                        }
       
                    }
                }
                
            }
        }


        public void ClearDisplayImage(int index)
        {
            if (index < HWindowFitExtList.Count() && index >= 0)
            {
                HWindowFitExtList[index].ClearHWindow();
            }
        }

        public void ClearAllDisplayImages()
        {
            for (int i = 0;i< HWindowFitExtList.Count();++i)
            {
                HWindowFitExtList[i].ClearHWindow();
            }
        }

        public void ClearAllDisplayRoi()
        {
            for (int i = 0; i < HWindowFitExtList.Count(); ++i)
            {
                HWindowFitExtList[i].HRegions.Clear();
                HWindowFitExtList[i].MeasureROITexts.Clear();
            }
        }

        public void ClearAssignDisplayRoi(int index)
        {
            if (index < HWindowFitExtList.Count() && index >= 0)    //是否显示，系统会先判断当前的输出窗口索引是否超出当前的窗口数量。所以窗口变少也不会发生严重错误
            {
                HWindowFitExtList[index].MeasureROITexts.Clear();
                HWindowFitExtList[index].HRegions.Clear();
            }
        }

        private void DisplayOnly(HWindow hWindow,bool singledisp)
        {
            if (HWindowFitExtList.Count>0)
            {
                if (singledisp)
                {
                    foreach (HWindowFitExt item in HWindowFitExtList)
                    {
                        if (item.HWindowID == hWindow)
                        {                     
                            List<HObject> hRegions = item.HRegions;
                            HObject hobject = item.Image;
                            SetSingleDisplay();
                            HWindowFitExtList[0].HWindowID.DispObj(hobject);
                            HWindowFitExtList[0].HRegions.Clear();
                            foreach (HObject item1 in hRegions)   //对输入进来的RoiList进行循环遍历显示，List对应的就是当前的模块（模板匹配）的RoiList信息输出
                            {
                                HWindowFitExtList[0].HWindowID.SetColor("red");
                                HWindowFitExtList[0].HWindowID.DispObj(item1);
                                HWindowFitExtList[0].HRegions.Add(item1);
                            }

                            break;
                        }
                    }
                }
           
            }
        
        }


        public void SetSingleDisplay()//设置要显示的画布数量
        {
            _gridImage.Children.Clear();
            _gridImage.RowDefinitions.Clear();
            _gridImage.ColumnDefinitions.Clear();
            HWindowFitExtList.Clear();
            RowDefinition rd = new RowDefinition();
            _gridImage.RowDefinitions.Add(rd);
            ColumnDefinition cd = new ColumnDefinition();
            _gridImage.ColumnDefinitions.Add(cd);
            HWindowFitExt hWindowFitExt = new HWindowFitExt();  //new并调用了HWindowFitExt窗体，根据选择的画布来添加HWindowFitExt
            hWindowFitExt.DelegateDisplayEvent += DisplayOnly;
            if (RePaint != null)
             {
                hWindowFitExt.RePaint += RePaint;
            }
             Grid.SetRow(hWindowFitExt, 1);
             Grid.SetColumn(hWindowFitExt, 1);
             _gridImage.Children.Add(hWindowFitExt);
             HWindowFitExtList.Add(hWindowFitExt);
               
        }



    }
}
