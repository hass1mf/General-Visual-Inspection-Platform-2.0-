using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
using System.Windows.Threading;
using HalconDotNet;
using HForms = System.Windows.Forms;
using System.Data;
//HWindowFitExt类用于创建基本显示控件，可传入图像并显示。带有鼠标右键和滑轮事件

namespace Heart.Outward
{
    public delegate void DelegateRePaint();     //控件留有委托接口RePaint
    public delegate void DelegateDisplay(HWindow HWindowID,bool disp);     //控件留有委托接口RePaint

    /// <summary>
    /// HWindowFitExt.xaml 的交互逻辑
    /// </summary>
    public partial class HWindowFitExt : UserControl
    {
        private HImage hv_image;
        private List<HObject> hv_Objects = new List<HObject>();
        public List<MeasureROIText> MeasureROITexts = new List<MeasureROIText>();
        public bool IsDispRoi
        {
          get
          {
                if (OriginalDisp_Item != null)
                {
                    return OriginalDisp_Item.IsChecked;
                }
                else 
                {
                    return false;
                }          
          }
          
        }
        //private HImage hv_image;
        private HWindow hv_window;
        private int hv_imageWidth, hv_imageHeight;
        private double mposition_row, mposition_col;
        private int _offsetRow, _offsetCol, _dispWidth, _dispHeight;
        // 设定图像的窗口显示部分
        private int zoom_beginRow, zoom_beginCol, zoom_endRow, zoom_endCol;
        // 获取图像的当前显示部分
        private int current_beginRow, current_beginCol, current_endRow, current_endCol;
        // 窗口和图像的宽高比
        private double ratio_win, ratio_img;
        //鼠标在控件上的按下or松开状态
        private bool b_leftButton, b_rightButton;
        //记录鼠标按下时鼠标在图像的相对位置
        private double start_positionRow, start_positionColumn;
        private bool SingleDisp = false;
        double hv_lastRow;
        double hv_lastCol;

        //是否涂抹
        private bool blnDraw = false;
        private int drawMode = 0;// 0 - 绘制模式 增加ROI 1- 涂抹减去ROI
        private HRegion brushRegion = new HRegion();
        private DateTime lastTime = DateTime.Now;

        //底部label显示的图像信息 
        private string str_value;//灰度值
        private string str_position;//鼠标位置
        private double focusScore;//图像清晰度评分
        private HRegion focusRegion;//图像清晰度区域
        /// *Tenegrad Tenegrad函数法 对于高像素的图片,如2900W,该算法时间约为100ms,其他均大于200ms
        /// *Deviation   '方差法
        /// *laplace    '拉普拉斯能量函数
        /// *energy     '能量梯度函数 效果最好
        /// *Brenner    ' Brenner函数法
        private string focusMethod = "energy";
        public DelegateDisplay DelegateDisplayEvent = null;
        private string str_imgSize;
        private ContextMenu hv_Menu;
         
        MenuItem fitWindow_Item;
        MenuItem saveImage_Item;
        MenuItem showCross_Item;
        MenuItem showFocus_Item;
        MenuItem switchBackcolor_Item;
        MenuItem OriginalDisp_Item;

        public event DelegateRePaint RePaint;

        public HWindow HWindowID
        {
            get { return this._hWindowControlWPF.HalconWindow; }
        }
        //public IntPtr HWindowHalconID
        //{
        //    get { return _hWindowControlWPF.HalconID; }
        //}

        public HImage Image
        {
            get { return hv_image; }
            set
            {
                //hv_image = value;
                this.hv_image = value;
                if (hv_image != null && value.IsInitialized())
                {
                    hv_image.GetImageSize(out hv_imageWidth, out hv_imageHeight);

                    str_imgSize = String.Format("{0}X{1}", hv_imageWidth, hv_imageHeight);

                    //string type = hv_image.GetImageType().ToString();

                    if (showFocus_Item.IsChecked && focusRegion != null && focusRegion.IsInitialized())
                    {
                        focusScore = FocusTool.evaluate_definition(this.hv_image.ReduceDomain(focusRegion), focusMethod);
                        this.Dispatcher.Invoke(new Action(
                        delegate
                        {
                            _labelStatus.Content = str_imgSize + "    " + str_position + "    " + str_value + "  清晰度:  " + focusScore;
                        }));
                    }

                    fitWindow_Item.Visibility = Visibility.Visible;
                    saveImage_Item.Visibility = Visibility.Visible;
                    showFocus_Item.Visibility = Visibility.Visible;

                    hv_window.SetDraw("margin");
                    hv_window.DispObj(hv_image);
                    PaintCross();
                }
            }
        }

        public List<HObject> HRegions
        {
            get { return hv_Objects; }

            set
            {
                HRegions = value;

            }
         
        }        

        public int offsetRow
        {
            get { return _offsetRow; }
        }

        public int offsetCol
        {
            get { return _offsetCol; }
        }

        public int dispWidth
        {
            get { return _dispWidth; }
        }

        public int dispHeight
        {
            get { return _dispHeight; }
        }

        public HWindowFitExt()
        {
            InitializeComponent();
        }


        private void _hWindowControlWPF_HInitWindow(object sender, EventArgs e)
        {
            hv_window = _hWindowControlWPF.HalconWindow;
            //hv_window.SetDraw("margin");
            //              设定鼠标按下时图标的形状
            //              'arrow'  'default' 'crosshair' 'text I-beam' 'Slashed circle' 'Size All'
            //              'Size NESW' 'Size S' 'Size NWSE' 'Size WE' 'Vertical Arrow' 'Hourglass'
            //
            //hv_window.SetMshape("Hourglass");

            fitWindow_Item = new MenuItem();
            fitWindow_Item.Header = "适应窗口";
            fitWindow_Item.IsCheckable = false;
            fitWindow_Item.Click += new RoutedEventHandler((s, ev) => DispImageFit());

            showCross_Item = new MenuItem();
            showCross_Item.Header = "显示十字线";
            showCross_Item.IsCheckable = true;

            showFocus_Item = new MenuItem();
            showFocus_Item.Header = "显示图像清晰度";
            showFocus_Item.IsCheckable = true;
            showFocus_Item.Click += new RoutedEventHandler((s, ev) => FocusClick());

            saveImage_Item = new MenuItem();
            saveImage_Item.Header = "保存结果图像";
            saveImage_Item.Click += new RoutedEventHandler((s, ev) => SaveWindowDumpDialog());

            switchBackcolor_Item = new MenuItem();
            switchBackcolor_Item.Header = "窗体颜色";
            switchBackcolor_Item.IsCheckable = true;
            switchBackcolor_Item.Click += new RoutedEventHandler((s, ev) => BackColorChanged(s, ev));

            OriginalDisp_Item = new MenuItem();
            OriginalDisp_Item.Header = "显示原图";
            OriginalDisp_Item.IsCheckable = true;
            OriginalDisp_Item.Click += new RoutedEventHandler((s, ev) => OriginalDisp());



            hv_Menu = new ContextMenu();
            hv_Menu.Items.Add(fitWindow_Item);
            hv_Menu.Items.Add(showCross_Item);
            hv_Menu.Items.Add(showFocus_Item);
            hv_Menu.Items.Add(saveImage_Item);
            hv_Menu.Items.Add(switchBackcolor_Item);
            hv_Menu.Items.Add(OriginalDisp_Item);

            fitWindow_Item.Visibility = Visibility.Collapsed;
            saveImage_Item.Visibility = Visibility.Collapsed;
            showFocus_Item.Visibility = Visibility.Collapsed;

            _hWindowControlWPF.ContextMenu = hv_Menu;
            _hWindowControlWPF.SizeChanged += new SizeChangedEventHandler((s, ev) => DispImageFit());
            _hWindowControlWPF.MouseDoubleClick += _hWindowControlWPF_MouseDoubleClick;
           // _hWindowControlWPF.
            hv_Menu.MouseEnter += new MouseEventHandler((s, ev) => { _hWindowControlWPF.HMouseWheel -= _hWindowControlWPF_HMouseWheel; });
            hv_Menu.MouseLeave += new MouseEventHandler((s, ev) => { _hWindowControlWPF.HMouseWheel += _hWindowControlWPF_HMouseWheel; });

        }

        /// <summary>
        /// 清空图像显示
        /// </summary>
        public void ClearHWindow()
        {
            HSystem.SetSystem("flush_graphic", "false");
            hv_window.ClearWindow();
            fitWindow_Item.Visibility = Visibility.Collapsed;
            saveImage_Item.Visibility = Visibility.Collapsed;
            showFocus_Item.Visibility = Visibility.Collapsed;
            Image = null;
            HSystem.SetSystem("flush_graphic", "true");
        }



        private void OriginalDisp()
        {
            try
            {

                if (OriginalDisp_Item.IsChecked)
                {
                    if (hv_image != null && hv_image.IsInitialized())
                    {
                        hv_window.DispObj(hv_image);
                    }
                }
                else
                {
                        DispMessageROI();                  
                }
                
            }
            catch
            {

            }

        }

        public void DispImageFit()
        {
            try
            {
                if (hv_image != null && hv_image.IsInitialized())
                {
                    ratio_win = (double)_hWindowControlWPF.ActualWidth / (double)_hWindowControlWPF.ActualHeight;
                    ratio_img = (double)hv_imageWidth / (double)hv_imageHeight;

                    int _beginRow, _begin_Col, _endRow, _endCol;

                    if (ratio_win >= ratio_img)
                    {
                        _beginRow = 0;
                        _endRow = hv_imageHeight - 1;
                        _begin_Col = (int)(-hv_imageWidth * (ratio_win / ratio_img - 1d) / 2d);
                        _endCol = (int)(hv_imageWidth + hv_imageWidth * (ratio_win / ratio_img - 1d) / 2d);
                    }
                    else
                    {
                        _begin_Col = 0;
                        _endCol = hv_imageWidth - 1;
                        _beginRow = (int)(-hv_imageHeight * (ratio_img / ratio_win - 1d) / 2d);
                        _endRow = (int)(hv_imageHeight + hv_imageHeight * (ratio_img / ratio_win - 1d) / 2d);
                    }
                    zoom_beginRow = _beginRow;
                    zoom_beginCol = _begin_Col;
                    zoom_endRow = _endRow;
                    zoom_endCol = _endCol;
                    HSystem.SetSystem("flush_graphic", "false");
                    _hWindowControlWPF.HalconWindow.ClearWindow();
                    HSystem.SetSystem("flush_graphic", "true");
                    _hWindowControlWPF.HalconWindow.SetPart(_beginRow, _begin_Col, _endRow, _endCol);
                    _offsetRow = _beginRow;
                    _offsetCol = _begin_Col;
                    _dispWidth = _endRow - _beginRow;
                    _dispHeight = _endCol - _begin_Col;
                    _hWindowControlWPF.HalconWindow.DispObj(hv_image);
                    DispMessageROI();
                    HSystem.SetSystem("flush_graphic", "true");
                    PaintCross();
                    if (RePaint != null)
                    {
                        RePaint();
                    }
                }
            }
            catch (Exception ex)
            {
                _labelStatus.Content = ex.Message;
            }
        }

        private void PaintCross()
        {
            if (showCross_Item.IsChecked)
            {
                //显示十字线
                HXLDCont xldCross = new HXLDCont();
                _hWindowControlWPF.HalconWindow.SetColor("green");

                _hWindowControlWPF.HalconWindow.DispLine(hv_imageHeight / 2.0, 0, hv_imageHeight / 2.0, hv_imageWidth);
                _hWindowControlWPF.HalconWindow.DispLine(0, hv_imageWidth / 2.0, hv_imageHeight, hv_imageWidth / 2.0);
            }

            //显示清晰评价区域
            if (showFocus_Item.IsChecked && focusRegion != null && focusRegion.IsInitialized())
            {
                _hWindowControlWPF.HalconWindow.DispObj(focusRegion);
            }
        }

        private void FocusClick()//评价图像清晰度
        {

            _hWindowControlWPF.HalconWindow.SetDraw("margin");
            if (showFocus_Item.IsChecked)//如果选中显示图像清晰度
            {
                //画出兴趣图像区域
                double _rowBegin, _colBegin, _rowEnd, _colEnd;

                if (focusRegion == null)
                {
                    focusRegion = new HRegion();
                }
                else
                {
                    focusRegion.Dispose();
                }

                this.DrawRectangle1("red", out _rowBegin, out _colBegin, out _rowEnd, out _colEnd);
                focusRegion.GenRectangle1(_rowBegin, _colBegin, _rowEnd, _colEnd);

                focusScore = FocusTool.evaluate_definition(this.hv_image.ReduceDomain(focusRegion), focusMethod);
                _labelStatus.Content = str_imgSize + "    " + str_position + "    " + str_value + "  清晰度:  " + focusScore;
            }
        }

        private void SaveWindowDumpDialog()
        {
            try
            {
                HForms.SaveFileDialog sfd = new HForms.SaveFileDialog();
                //string imgFileName;

                sfd.Filter = "HE图像|*.he|PNG图像|*.png|BMP图像|*.bmp|JPEG图像|*.jpg|所有文件|*.*";

                if (sfd.ShowDialog() == HForms.DialogResult.OK)
                {
                    if (String.IsNullOrEmpty(sfd.FileName))
                        return;
                    if (hv_image is HImageExt)
                        ((HImageExt)hv_image).WriteImageExt(sfd.FileName);
                    else
                    {
                        string ext = System.IO.Path.GetExtension(sfd.FileName);
                        hv_image.WriteImage(ext.Substring(1), 0, sfd.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                _labelStatus.Content = ex.Message;
            }

        }

        /// <summary>
        /// 保存结果至特定分辨率的图像文件
        /// </summary>
        /// <param name="fileName">图像文件路径</param>
        /// <param name="size">图像分辨率</param>
        public void SaveWindowDump(string fileName, System.Drawing.Size size)
        {
            try
            {
                if (hv_image != null && hv_image.IsInitialized())
                {
                    HWindowControl hw = new HWindowControl();
                    hw.WindowSize = size;

                    DispImageFit();
                    hw.HalconWindow.DumpWindow("png best", fileName);
                    hw.Dispose();
                }
            }
            catch (Exception ex)
            {
                _labelStatus.Content = ex.Message;
            }
        }

        private void BackColorChanged(object sender, EventArgs e)
        {
            if (((MenuItem)sender).IsChecked)
            {
                _hWindowControlWPF.HalconWindow.SetWindowParam("background_color", "white");
            }
            else
            {
                _hWindowControlWPF.HalconWindow.SetWindowParam("background_color", "black");
            }
            refreshWindow();
        }

    



        public void DrawRectangle1Mod(string color, double row1, double col1, double row2, double col2, out double rowBegin, out double colBegin, out double rowEnd, out double colEnd)
        {
            try
            {
                Double _rowBegin, _colBegin, _rowEnd, _colEnd;

                //ShieldMouseEvent();
                _hWindowControlWPF.Focus();
                hv_window.SetColor(color);
                hv_window.DrawRectangle1Mod(row1, col1, row2, col2, out _rowBegin, out _colBegin, out _rowEnd, out _colEnd);
                HRegion rectangle = new HRegion();
                rectangle.GenRectangle1(_rowBegin, _colBegin, _rowEnd, _colEnd);
                rectangle.DispObj(hv_window);
                //ReloadMouseEvent();
                rectangle.Dispose();
                rowBegin = _rowBegin;
                colBegin = _colBegin;
                rowEnd = _rowEnd;
                colEnd = _colEnd;
            }
            catch (System.Exception ex)
            {
                rowBegin = 0.0;
                colBegin = 0.0;
                rowEnd = 0.0;
                colEnd = 0.0;
                _labelStatus.Content = ex.Message;
            }
        }

        private void _hWindowControlWPF_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DelegateDisplayEvent?.Invoke(HWindowID,SingleDisp);
        }



        /// <summary>
        ///  在图像中绘制矩形区域
        /// </summary>
        /// <param name="rowBegin"></param>
        /// <param name="colBegin"></param>
        /// <param name="rowEnd"></param>
        /// <param name="colEnd"></param>
        public void DrawRectangle1(string color, out double rowBegin, out double colBegin, out double rowEnd, out double colEnd)
        {

            try
            {
                Double _rowBegin, _colBegin, _rowEnd, _colEnd;

                ShieldMouseEvent();

                _hWindowControlWPF.Focus();
                hv_window.SetColor(color);
                hv_window.DrawRectangle1(out _rowBegin, out _colBegin, out _rowEnd, out _colEnd);
                HRegion rectangle = new HRegion();
                rectangle.GenRectangle1(_rowBegin, _colBegin, _rowEnd, _colEnd);
                rectangle.DispObj(hv_window);
                rectangle.Dispose();

                ReloadMouseEvent();

                rowBegin = _rowBegin;
                colBegin = _colBegin;
                rowEnd = _rowEnd;
                colEnd = _colEnd;
            }
            catch (System.Exception ex)
            {
                rowBegin = 0.0;
                colBegin = 0.0;
                rowEnd = 0.0;
                colEnd = 0.0;
                _labelStatus.Content = ex.Message;
            }
        }

        /// <summary>
        ///  在图像中绘制矩形区域
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="phi"></param>
        /// <param name="length1"></param>
        /// <param name="length2"></param>
        public void DrawRectangle2(string color, out double row, out double column, out double phi, out double length1, out double length2)
        {

            try
            {
                Double _row, _column, _phi, _length1, _length2;

                ShieldMouseEvent();

                _hWindowControlWPF.Focus();
                hv_window.SetColor(color);
                hv_window.DrawRectangle2(out _row, out _column, out _phi, out _length1, out _length2);
                HRegion rectangle = new HRegion();
                rectangle.GenRectangle2(_row, _column, _phi, _length1, _length2);
                rectangle.DispObj(hv_window);
                rectangle.Dispose();
                ReloadMouseEvent();

                row = _row;
                column = _column;
                phi = _phi;
                length1 = _length1;
                length2 = _length2;
            }
            catch (System.Exception ex)
            {
                row = 0.0;
                column = 0.0;
                phi = 0.0;
                length1 = 0.0;
                length2 = 0.0;
                _labelStatus.Content = ex.Message;
            }
        }

        /// <summary>
        /// 在图像中绘制圆形区域
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="radius"></param>
        public void DrawCircle(string color, out double row, out double column, out double radius)
        {
            try
            {

                Double _row, _column, _radius;
                ShieldMouseEvent();

                _hWindowControlWPF.Focus();
                hv_window.SetColor(color);
                hv_window.DrawCircle(out _row, out _column, out _radius);

                HRegion circle = new HRegion();
                circle.GenCircle(_row, _column, _radius);
                circle.DispObj(hv_window);
                circle.Dispose();
                ReloadMouseEvent();

                row = _row;
                column = _column;
                radius = _radius;

            }
            catch (Exception ex)
            {
                row = 0.0;
                column = 0.0;
                radius = 0.0;
                _labelStatus.Content = ex.Message;
            }
        }

        public void DrawPoint(string color, out double Row, out double Col)
        {
            try
            {
                ShieldMouseEvent();

                _hWindowControlWPF.Focus();
                hv_window.SetColor(color);
                hv_window.DrawPoint(out Row, out Col);
                hv_window.DispCross(Row, Col, 10, 0);
                hv_window.DispRectangle2(Row, Col, 0, 3, 3);
                ReloadMouseEvent();
            }
            catch (Exception ex)
            {
                Row = 0;
                Col = 0;
                _labelStatus.Content = ex.Message;
            }
        }

        /// <summary>
        /// 在图像中绘制直线
        /// </summary>
        /// <param name="color"></param>
        /// <param name="beginRow"></param>
        /// <param name="beginCol"></param>
        /// <param name="endRow"></param>
        /// <param name="endCol"></param>
        public void DrawLine(string color, out double beginRow, out double beginCol, out double endRow, out double endCol)
        {
            try
            {

                Double _beginRow, _beginCol, _endRow, _endCol;
                ShieldMouseEvent();

                _hWindowControlWPF.Focus();
                hv_window.SetColor(color);
                hv_window.DrawLine(out _beginRow, out _beginCol, out _endRow, out _endCol);

                hv_window.DispLine(_beginRow, _beginCol, _endRow, _endCol);
                ReloadMouseEvent();

                beginRow = _beginRow;
                beginCol = _beginCol;
                endRow = _endRow;
                endCol = _endCol;

            }
            catch (Exception ex)
            {
                beginRow = 0.0;
                beginCol = 0.0;
                endRow = 0.0;
                endCol = 0.0;
                _labelStatus.Content = ex.Message;
            }
        }
        /// <summary>
        /// 在图像中绘制椭圆形区域
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="radius"></param>
        public void DrawEllipse(string color, out double row, out double column, out double phi, out double radius1, out double radius2)
        {
            try
            {

                Double _row, _column, _phi, _radius1, _radius2;
                ShieldMouseEvent();

                _hWindowControlWPF.Focus();
                hv_window.SetColor(color);
                hv_window.DrawEllipse(out _row, out _column, out _phi, out _radius1, out _radius2);

                HRegion ellipse = new HRegion();
                ellipse.GenEllipse(_row, _column, _phi, _radius1, _radius2);
                ellipse.DispObj(hv_window);
                ellipse.Dispose();
                ReloadMouseEvent();

                row = _row;
                column = _column;
                phi = _phi;
                radius1 = _radius1;
                radius2 = _radius2;

            }
            catch (Exception ex)
            {
                row = 0.0;
                column = 0.0;
                phi = 0.0;
                radius1 = 0.0;
                radius2 = 0.0;
                _labelStatus.Content = ex.Message;
            }
        }

        public HRegion SetROI(HRegion region)
        {
            this._hWindowControlWPF.Focus();//必须先聚焦

            try
            {
                _hWindowControlWPF.HMouseMove -= _hWindowControlWPF_HMouseMove;
                blnDraw = true;
                double Row = 0, Column = 0;
                //鼠标状态
                int hv_Button = 0;
                // 4为鼠标右键
                while (hv_Button != 4)
                {
                    //一直在循环,需要让halcon控件也响应事件,不然到时候跳出循环,之前的事件会一起爆发触发,
                    //Application.DoEvents();
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
                    //获取鼠标坐标
                    try
                    {
                        hv_window.GetMpositionSubPix(out Row, out Column, out hv_Button);

                        hv_window.GetPart(out current_beginRow, out current_beginCol, out current_endRow, out current_endCol);
                        double d = Math.Sqrt(Math.Pow(current_endRow - current_beginRow, 2) + Math.Pow(current_endCol - current_beginCol, 2));
                        brushRegion.GenCircle(Row, Column, d / 50.0);
                    }
                    catch (HalconException ex)
                    {
                        hv_Button = 0;
                    }

                    //check if mouse cursor is over window
                    if (Row >= 0 && Column >= 0)
                    {
                        //1为鼠标左键
                        if (hv_Button == 1)
                        {
                            //画出笔刷
                            switch (drawMode)
                            {
                                case 0:
                                    {
                                        if (region.IsInitialized())
                                            region = region.Union2(brushRegion);
                                        else
                                            region = brushRegion;
                                    }
                                    break;
                                case 1:
                                    region = region.Difference(brushRegion);
                                    break;
                                default:
                                    MessageBox.Show("设置错误");
                                    break;
                            }//end switch

                        }//end if
                    }
                    HOperatorSet.SetSystem("flush_graphic", "false");//防止画面闪烁
                    hv_window.DispObj(hv_image);

                    hv_window.SetDraw("margin");
                    hv_window.SetLineWidth(1);

                    hv_window.SetColor("green");//这一段也必须放在中间
                    if (region != null && region.IsInitialized())
                        hv_window.DispObj(region);

                    HOperatorSet.SetSystem("flush_graphic", "true");//很奇怪必须放在这里,不能把下面的给包含进去

                    if (drawMode == 0)
                        hv_window.SetColor("green");
                    else
                        hv_window.SetColor("gray");
                    if (brushRegion != null && brushRegion.IsInitialized())
                        hv_window.DispObj(brushRegion);

                }//end while
                blnDraw = false;
                _hWindowControlWPF.HMouseMove += _hWindowControlWPF_HMouseMove;
                return region;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 刷新控件
        /// </summary>
        public void refreshWindow()
        {
            HSystem.SetSystem("flush_graphic", "false");
            hv_window.ClearWindow();
            HSystem.SetSystem("flush_graphic", "true");
            if (hv_image != null && hv_image.IsInitialized())
            {
                hv_window.DispObj(hv_image);
            }
        }

        /// <summary>
        /// 屏蔽鼠标事件
        /// </summary>
        public void ShieldMouseEvent()
        {
            _hWindowControlWPF.ContextMenu = null;
            this._hWindowControlWPF.HMouseDown -= new HalconDotNet.HMouseEventHandlerWPF(this._hWindowControlWPF_HMouseDown);
            this._hWindowControlWPF.HMouseUp -= new HalconDotNet.HMouseEventHandlerWPF(this._hWindowControlWPF_HMouseUp);
            _hWindowControlWPF.HMouseMove -= _hWindowControlWPF_HMouseMove;
            _hWindowControlWPF.HMouseWheel -= _hWindowControlWPF_HMouseWheel;
            _hWindowControlWPF.MouseDoubleClick -= _hWindowControlWPF_MouseDoubleClick;

        }

        /// <summary>
        /// 重新加载　鼠标事件
        /// </summary>
        public void ReloadMouseEvent()
        {
            _hWindowControlWPF.ContextMenu = hv_Menu;
            this._hWindowControlWPF.HMouseDown += new HalconDotNet.HMouseEventHandlerWPF(this._hWindowControlWPF_HMouseDown);
            this._hWindowControlWPF.HMouseUp += new HalconDotNet.HMouseEventHandlerWPF(this._hWindowControlWPF_HMouseUp);
            _hWindowControlWPF.HMouseMove += _hWindowControlWPF_HMouseMove;
            _hWindowControlWPF.HMouseWheel += _hWindowControlWPF_HMouseWheel;
            _hWindowControlWPF.MouseDoubleClick += _hWindowControlWPF_MouseDoubleClick;
        }

        ////鼠标按下
        private void _hWindowControlWPF_HMouseDown(object sender, HMouseEventArgsWPF e)
        {
            int temp_button_state;
            try
            {
                switch (e.Button)
                {
                    case MouseButton.Left:                     
                         this.Cursor = Cursors.Arrow;
                        // if ((DateTime.Now - lastTime).TotalMilliseconds < 600)
                        // {
                        //    //drawMode = Math.Abs(drawMode - 1);
                        //    if (SingleDisp ==false)
                        //    {
                        //        SingleDisp = true;
                        //        DelegateDisplayEvent?.Invoke(HWindowID, SingleDisp);
                        //    }
                        //    else
                        //    {
                        //        SingleDisp = false;
                        //    }

                        //}
                         lastTime = DateTime.Now;

                        hv_window.GetMpositionSubPix(out start_positionRow, out start_positionColumn, out temp_button_state);
                        b_leftButton = true;
                        break;
                    case MouseButton.Right:
                        b_rightButton = true;

                        this.Cursor = Cursors.Arrow;
                        break;
                    case MouseButton.Middle:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _labelStatus.Content = ex.Message;
            }


        }
        
        private void _hWindowControlWPF_HMouseMove(object sender, HMouseEventArgsWPF e)
        {
            if (hv_image != null && hv_image.IsInitialized())
            {
                try
                {
                    int button_state;
                    double positionRow, positionColumn;
                    bool _isXOut = true, _isYOut = true;
                    HTuple channel_count;
                    HTuple hv_mode = new HTuple();


                    HOperatorSet.CountChannels(hv_image, out channel_count);

                    hv_window.GetMpositionSubPix(out positionRow, out positionColumn, out button_state);
                    str_position = String.Format("ROW: {0:0000.0}, COLUMN: {1:0000.0}", positionRow, positionColumn);

                    _isXOut = (positionColumn < 0 || positionColumn >= hv_imageWidth);
                    _isYOut = (positionRow < 0 || positionRow >= hv_imageHeight);


                    if (!_isXOut && !_isYOut)
                    {
                        try
                        {
                            if ((int)channel_count == 1)
                            {
                                double grayVal;
                                grayVal = hv_image.GetGrayval((int)positionRow, (int)positionColumn);
                                str_value = String.Format("Val: {0:000.0}", grayVal);
                            }
                            else if ((int)channel_count == 3)
                            {
                                double grayValRed, grayValGreen, grayValBlue;

                                HImage _RedChannel, _GreenChannel, _BlueChannel;

                                _RedChannel = hv_image.AccessChannel(1);
                                _GreenChannel = hv_image.AccessChannel(2);
                                _BlueChannel = hv_image.AccessChannel(3);

                                grayValRed = _RedChannel.GetGrayval((int)positionRow, (int)positionColumn);
                                grayValGreen = _GreenChannel.GetGrayval((int)positionRow, (int)positionColumn);
                                grayValBlue = _BlueChannel.GetGrayval((int)positionRow, (int)positionColumn);

                                _RedChannel.Dispose();
                                _GreenChannel.Dispose();
                                _BlueChannel.Dispose();

                                str_value = String.Format("Val: ({0:000.0}, {1:000.0}, {2:000.0})", grayValRed, grayValGreen, grayValBlue);
                            }
                            else
                            {
                                str_value = "";
                            }


                            if (showFocus_Item.IsChecked)//如果选中显示图像清晰度
                            {
                                _labelStatus.Content = str_imgSize + "    " + str_position + "    " + str_value + "  清晰度:  " + focusScore;
                            }
                            else
                            {
                                _labelStatus.Content = str_imgSize + "    " + str_position + "    " + str_value;
                            }
                        }
                        catch (System.Exception ex)
                        {
                            _labelStatus.Content = ex.Message;
                        }


                        //当鼠标左键按下移动时，移动图片，并且鼠标变成手状
                        switch (button_state)
                        {
                            case 0:
                                this.Cursor = Cursors.Arrow;
                                break;
                            case 1:
                                if (b_leftButton)
                                {
                                    this.Cursor = Cursors.Hand;
                                    HSystem.SetSystem("flush_graphic", "false");
                                    hv_window.ClearWindow();
                                    HSystem.SetSystem("flush_graphic", "true");
                                    hv_window.SetPaint(new HTuple("default"));
                                    //              保持图像显示比例
                                    zoom_beginCol -= (int)(positionColumn - start_positionColumn);
                                    zoom_beginRow -= (int)(positionRow - start_positionRow);
                                    zoom_endCol -= (int)(positionColumn - start_positionColumn);
                                    zoom_endRow -= (int)(positionRow - start_positionRow);
                                    hv_window.SetPart(zoom_beginRow, zoom_beginCol, zoom_endRow, zoom_endCol);
                                    hv_window.DispObj(hv_image);
                                    DispMessageROI();
                                    PaintCross();
                                    if (RePaint != null)
                                    {
                                        RePaint();
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    hv_lastCol = positionRow;
                    hv_lastRow = positionColumn;
                }
                catch (Exception ex)
                {
                    _labelStatus.Content = ex.Message;
                }
            }

        }

        //鼠标释放
        private void _hWindowControlWPF_HMouseUp(object sender, HMouseEventArgsWPF e)
        {
            try
            {
                switch (e.Button)
                {
                    case MouseButton.Left:
                        this.Cursor = Cursors.Arrow;
                        b_leftButton = false;
                        break;
                    case MouseButton.Right:
                        _hWindowControlWPF.ContextMenu.IsOpen = true;
                        b_rightButton = false;
                        break;
                    case MouseButton.Middle:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _labelStatus.Content = ex.Message;
            }
        }

        private void _hWindowControlWPF_HMouseWheel(object sender, HMouseEventArgsWPF e)
        {
            if (hv_window != null && hv_window.IsInitialized())
            {
                try
                {
                    int button_state;
                    hv_window.GetMpositionSubPix(out mposition_row, out mposition_col, out button_state);
                    hv_window.GetPart(out current_beginRow, out current_beginCol, out current_endRow, out current_endCol);
                }
                catch (Exception ex)
                {
                    _labelStatus.Content = ex.Message;
                }

                if (e.Delta > 0)                 // 放大图像
                {
                    zoom_beginRow = (int)(current_beginRow + (mposition_row - current_beginRow) * 0.100d);
                    zoom_beginCol = (int)(current_beginCol + (mposition_col - current_beginCol) * 0.100d);
                    zoom_endRow = (int)(current_endRow - (current_endRow - mposition_row) * 0.100d);
                    zoom_endCol = (int)(current_endCol - (current_endCol - mposition_col) * 0.100d);

                }
                else                // 缩小图像
                {
                    zoom_beginRow = (int)(mposition_row - (mposition_row - current_beginRow) / 0.900d);
                    zoom_beginCol = (int)(mposition_col - (mposition_col - current_beginCol) / 0.900d);
                    zoom_endRow = (int)(mposition_row + (current_endRow - mposition_row) / 0.900d);
                    zoom_endCol = (int)(mposition_col + (current_endCol - mposition_col) / 0.900d);
                }
                try
                {
                    int hw_width, hw_height;
                    hw_width = (int)_hWindowControlWPF.ActualWidth;
                    hw_height = (int)_hWindowControlWPF.ActualHeight;

                    bool _isOutOfArea = true;
                    bool _isOutOfSize = true;
                    bool _isOutOfPixel = true; //避免像素过大

                    _isOutOfArea = zoom_beginRow >= hv_imageHeight || zoom_endRow <= 0 || zoom_beginCol >= hv_imageWidth || zoom_endCol < 0;
                    _isOutOfSize = (zoom_endRow - zoom_beginRow) > hv_imageHeight * 20 || (zoom_endCol - zoom_beginCol) > hv_imageWidth * 20;
                    _isOutOfPixel = hw_height / (zoom_endRow - zoom_beginRow) > 500 || hw_width / (zoom_endCol - zoom_beginCol) > 500;

                    if (_isOutOfArea || _isOutOfSize)
                    {
                        DispImageFit();
                    }
                    else if (!_isOutOfPixel)
                    {

                        HSystem.SetSystem("flush_graphic", "false");
                        hv_window.ClearWindow();
                        HSystem.SetSystem("flush_graphic", "true");

                        //hv_window.SetPaint(new HTuple("default"));
                        //              保持图像显示比例
                        zoom_endCol = zoom_beginCol + (zoom_endRow - zoom_beginRow) * hw_width / hw_height;
                        hv_window.SetPart(zoom_beginRow, zoom_beginCol, zoom_endRow, zoom_endCol);
                        hv_window.DispObj(hv_image);
                        DispMessageROI();
                     }
                    PaintCross();
                    if (RePaint != null)
                    {
                        RePaint();
                    }
                }
                catch (Exception ex)
                {
                    DispImageFit();
                    _labelStatus.Content = ex.Message;
                }
            }
        }

        private void DispMessageROI()
        {
            if (!OriginalDisp_Item.IsChecked)
            {
                if (MeasureROITexts.Count > 0)
                {
                    foreach (MeasureROIText item in MeasureROITexts)
                    {
                        ImageTool.set_display_font(HWindowID, item.size, item.font, "false", "false");
                        ImageTool.disp_message(HWindowID, item.text, "image", item.row, item.col, item.drawColor, "false");
                    }

                }
                if (hv_Objects.Count > 0)
                {
                    hv_Objects.ForEach(i => { hv_window.DispObj(i); });
                }
            }
            
        }
    }
}
