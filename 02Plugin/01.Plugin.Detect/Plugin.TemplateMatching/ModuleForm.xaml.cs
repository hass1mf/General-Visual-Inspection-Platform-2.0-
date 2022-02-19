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

namespace Plugin.TemplateMatching
{
    /// <summary>
    /// ModuleForm.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleForm : ModuleFormBase
    {

        #region 变量定义及初始化

        private ModuleObj m_ModuleObj;  //ModuleForm类下的私有成员，可在类内部调用
        private List<CanvasItem> CanvasList = new List<CanvasItem>();
        public HTuple hv_ExpDefaultWinHandle;
        public HObject Ho_Image0;   //定义最后的灰度图像
        public static HObject H_Image;  //静态变量H_Image
        private ROI m_TemplateRegion;     //模型区域   //此ROI类在Struct_Info类内被定义好了
        private ROI m_SearchRegion;    //搜索区域
        /// <summary>
        /// 工具对象
        /// </summary>
        internal static MatchingTool shapeMatchTool = new MatchingTool();
        private MeasureROI roi搜索范围;
        private MeasureROI roi模板范围;
        private MeasureROI roi最终轮廓;
        /// <summary>
        /// 模板类型
        /// </summary>
        enum MatchMode
        {
            BasedShape,
            BasedGray,
        }

        public ModuleForm()
        {
            InitializeComponent();
            _hWinDisplay.RePaint += new DelegateRePaint(PaintMetrology);    //标定鼠标滚动时缩放
            _Manual.Visibility = Visibility.Visible;
            _Link.Visibility = Visibility.Collapsed;
        }

        #endregion


        #region 插件模块保存与执行

        /// <summary>
        /// 加载插件时初始化
        /// </summary>
        public override void LoadModule()   //画布选择
        {
            m_ModuleObj = (ModuleObj)ModuleObjBase;     //固定方法，拿出m_ModuleObj
            Title = m_ModuleObj.Info.ModuleName;
            if (m_ModuleObj.hv_img_Str != "")
            {
                //m_ModuleObj.ReadImg();  //通过这个函数，插件的m_ModuleObj.hv_img就被赋值了   //1020屏蔽掉这个hv_image的赋值，怕roi信息被冲掉了
                _inputImage.Text = m_ModuleObj.hv_img_Str;   //获取图像来源的信息
            }
            if (m_ModuleObj.hv_img != null)
            {
                HImage hImage = new HImage(m_ModuleObj.hv_img);
                _hWinDisplay.Image = hImage;  //经过New的转换，保证himage的纯净的himage
                _hWinDisplay.DispImageFit();    //自适应窗体
                PaintMetrology();   //重新绘制
                H_Image = m_ModuleObj.hv_img;       //H_Image是老插件的变量，用来判断当前插件是否用有效的变量进行传入。在模板匹配插件内没有用到
            }
            if(m_ModuleObj.Obj_shapeMatchTool != null)
            {
                shapeMatchTool = m_ModuleObj.Obj_shapeMatchTool;    //传入核心内容shapeMatchTool
            }
        }

        /// <summary>
        /// 点击执行按钮后前序函数
        /// </summary>
        public override void RunModuleBefore()
        {

        }

        /// <summary>
        /// 点击执行按钮后前序函数，中间还包含着Module内的ExeModule执行函数
        /// </summary>
        public override void RunModuleAfter()
        {
            if (m_ModuleObj.hv_img != null && m_ModuleObj.Obj_shapeMatchTool != null)     //保证moduleobj的图像和模板信息都存在才进行
            {
                _hWinDisplay.ClearHWindow();
                HImage hImage = new HImage(m_ModuleObj.hv_img);
                _hWinDisplay.Image = hImage;  //经过New的转换，保证himage的纯净的himage
                MatchingTool.MatchResult matchResult;
                shapeMatchTool.FindModel(out matchResult, out bool result);     //通过发现模板函数来返回matchResult和执行结果
                if (result) 
                {
                    ShowTemplate2(shapeMatchTool, matchResult.Angle, matchResult.Row, matchResult.Col);
                    m_ModuleObj.Obj_shapeMatchTool = shapeMatchTool;    //在执行函数的最后，如果寻找模板成功，就把shapeMatchTool传过去ModuleObj那边
                }
                // _hWinDisplay.ClearHWindow();    //匹配不到模板时也不要清空窗体
            }
            else
            {
                //_hWinDisplay.ClearHWindow();
            }
        }

        /// <summary>
        /// 点击保存按钮就只会进入这里
        /// </summary>
        public override void SaveModuleBefore()
        {
            m_ModuleObj.Obj_shapeMatchTool = shapeMatchTool;    //整个模板匹配最核心的内容都在MatchingTool类下的shapeMatchTool，所以转这个过去ModuleObj处即可
        }

        #endregion


        #region 插件UI按钮事件

        #region 基本参数

        /// <summary>
        /// 按钮--配置输入图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _GetImage_Click(object sender, RoutedEventArgs e)
        {
            VariableTable variableTable = new VariableTable();    //打开变量表
            variableTable.SetModuleInfo(m_ModuleObj.Info.ProjectID, m_ModuleObj.Info.ModuleName, "Image");  //加载表内内容？？？ //20211018传入图像格式改造为HImageExt

            //打开并维持窗体
            if (variableTable.ShowDialog() == true)     //变量窗体显示
            {
                //数据操作部分
                m_ModuleObj.hv_img_Str = variableTable.SelectedVariableText;    //通过list对话框，已经选择了要引用的变量
                m_ModuleObj.ReadImg();  //通过这个函数，插件的m_ModuleObj.hv_img就被赋值了
                if(m_ModuleObj.hv_img != null)  //链接读进来的图像不一定是有效的，所以需要先判断下是否为空。如果H_Image是空，则下面的操作无法进行
                {
                    _hWinDisplay.ClearHWindow();
                    _hWinDisplay.Image = (HImage)m_ModuleObj.hv_img;    //这里还是用HimageExt来强转Himage，可能有风险
                    _hWinDisplay.DispImageFit();    //链接进来的图像，做自适应处理
                    H_Image = m_ModuleObj.hv_img;       //H_Image是老插件的变量，用来判断当前插件是否用有效的变量进行传入。在模板匹配插件内没有用到
                    _inputImage.Text = m_ModuleObj.hv_img_Str;   //获取图像来源的信息，显示在画布上
                }
            }
        }

        /// <summary>
        /// 按钮--删除链接图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _DeleteImage_Click(object sender, RoutedEventArgs e)
        {
            _inputImage.Text = "";
        }

        /// <summary>
        /// 按钮--模板搜索区域编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _EditSearchArea_Click(object sender, RoutedEventArgs e)
        {
            if(H_Image != null)     //确保已经载入了图像才有函数被执行
            {
                string color = "red";
                double _rowBegin, _colBegin, _rowEnd, _colEnd;
                this._hWinDisplay.DrawRectangle1(color, out _rowBegin, out _colBegin, out _rowEnd, out _colEnd);  //在图像中绘制矩形区域，该函数在控件内就定义了。该矩阵不能旋转
                m_SearchRegion = new Rectangle_INFO(_rowBegin, _colBegin, _rowEnd, _colEnd);    //Struct_Info.cs类内定义的矩形信息。m_SearchRegion搜索区域
                m_SearchRegion.sColor = color;  //将选择好了的颜色传给模板模型区域
                //搜索范围
                roi搜索范围 = new MeasureROI(m_ModuleObj.Info.ProjectID.ToString(), "模板匹配", m_ModuleObj.Info.ModuleName, enMeasureROIType.搜索范围.ToString(), "red", new HObject(m_SearchRegion.genRegion()));    //整理ROI信息
                m_ModuleObj.hv_img.UpdateRoiList(roi搜索范围);
                shapeMatchTool.searchRegion = roi搜索范围;  //每次如果是重新的绘制或者是第一次绘制，也赋值过去
                PaintMetrology();   //重新绘制
            }
        }

        /// <summary>
        /// 搜索区域--选择手动输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _ManualInput_Click(object sender, RoutedEventArgs e)
        {
            _Manual.Visibility = Visibility.Visible;
            _Link.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 搜索区域--选择链接区域
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _LinkRegion_Click(object sender, RoutedEventArgs e)
        {
            _Manual.Visibility = Visibility.Collapsed;
            _Link.Visibility = Visibility.Visible;
        }

        #endregion

        #region 参数设置

        /// <summary>
        /// 按钮--模板学习区域编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _EditTemplateArea_Click(object sender, RoutedEventArgs e)
        {
            if (H_Image != null)    //确保已经载入了图像才有函数被执行
            {
                string color = "green";
                double _rowBegin, _colBegin, _rowEnd, _colEnd;
                this._hWinDisplay.DrawRectangle1(color, out _rowBegin, out _colBegin, out _rowEnd, out _colEnd);  //在图像中绘制矩形区域，该函数在控件内就定义了。该矩阵不能旋转
                m_TemplateRegion = new Rectangle_INFO(_rowBegin, _colBegin, _rowEnd, _colEnd);    //Struct_Info.cs类内定义的矩形信息
                m_TemplateRegion.sColor = color;  //将选择好了的颜色传给模板模型区域
                //模板范围
                roi模板范围 = new MeasureROI(m_ModuleObj.Info.ProjectID.ToString(), "模板匹配", m_ModuleObj.Info.ModuleName, enMeasureROIType.模板范围.ToString(), "green", new HObject(m_TemplateRegion.genRegion()));    //整理ROI信息
                m_ModuleObj.hv_img.UpdateRoiList(roi模板范围);
                shapeMatchTool.templateRegion = roi模板范围;    //每次如果是重新的绘制或者是第一次绘制，也赋值过去
                PaintMetrology();   //重新绘制
            }
        }

        /// <summary>
        /// 按钮--模板学习，插件模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _TemplateLearning_Click(object sender, RoutedEventArgs e)
        {
            if ((shapeMatchTool.templateRegion != null && shapeMatchTool.searchRegion != null) || ((m_TemplateRegion != null && m_SearchRegion != null)))    //m_ModelRegion模板区域  m_SearchRegion搜索区域  //20211020重新打开插件时，前两者是没有值的
            {
                //if(m_TemplateRegion != null && m_SearchRegion != null)
                //{
                //    shapeMatchTool.templateRegion = roi模板范围;
                //    shapeMatchTool.searchRegion = roi搜索范围;
                //}
                shapeMatchTool.Ho_Image = new HObject(m_ModuleObj.hv_img);
                if (shapeMatchTool.CreateTemplate() == 0)   //shapeMatchTool.CreateTemplate();    //CreateTemplate()结果状态返回值：0表示成功 -1表示未知异常 1表示特征过少
                {
                    ShowTemplate();     //显示模板

                    MatchingTool.MatchResult matchResult;
                    shapeMatchTool.FindModel(out matchResult, out bool result);     //通过发现模板函数来返回matchResult和执行结果

                    roi最终轮廓 = new MeasureROI(m_ModuleObj.Info.ProjectID.ToString(), "模板匹配", m_ModuleObj.Info.ModuleName, enMeasureROIType.检测结果.ToString(), "green", new HObject(shapeMatchTool.contour));    //整理ROI信息
                    m_ModuleObj.hv_img.UpdateRoiList(roi最终轮廓);      //在学习时，如果有新的模板识别生成，就要把ROI加进去
                    m_ModuleObj.Obj_shapeMatchTool = shapeMatchTool;    //在插件的核心按钮这里，赋值过去
                    PaintMetrology();   //重新绘制
                }
            }
        }

        #endregion

        #endregion


        #region 相关辅助函数

        /// <summary>
        /// HObject转HImage函数
        /// </summary>
        /// <param name="hobject"></param>
        /// <param name="image"></param>
        private void HobjectToHimage(HObject hobject, ref HImage image)
        {
            HTuple pointer, type, width, height;
            HOperatorSet.GetImagePointer1(hobject, out pointer, out type, out width, out height);
            image.GenImage1(type, width, height, pointer);
        }

        /// <summary>
        /// 显示模板
        /// </summary>
        internal void ShowTemplate()
        {
            try
            {
                HOperatorSet.GetShapeModelContours(out shapeMatchTool.contour, shapeMatchTool.modelID, new HTuple(1)); //shapeMatchTool.contour 最后输出的轮廓对象
                HTuple area, row, col;
                HOperatorSet.AreaCenter(shapeMatchTool.totalRegion, out area, out row, out col);   //totalRegion 原有模板的信息
                HTuple homMat2D;
                HOperatorSet.HomMat2dIdentity(out homMat2D);
                HOperatorSet.HomMat2dTranslate(homMat2D, row, col, out homMat2D);
                HOperatorSet.AffineTransContourXld(shapeMatchTool.contour, out shapeMatchTool.contour, homMat2D);   //有了放射矩阵homMat2D，计算后的shapeMatchTool.contour应该已经经过了修正
                _hWinDisplay.HWindowID.DispObj(shapeMatchTool.contour);

                HObject outBoundary, inBoundary;
                HOperatorSet.Boundary(shapeMatchTool.templateRegion.hobject, out outBoundary, "inner_filled");
                HOperatorSet.Boundary(shapeMatchTool.templateRegion.hobject, out inBoundary, "outer");
                _hWinDisplay.HWindowID.DispObj(shapeMatchTool.totalRegion);
            }
            catch (Exception ex)
            {
                
            }
        }

        /// <summary>
        /// 显示模板2
        /// </summary>
        /// <param name="matchingTool"></param>
        /// <param name="angle"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        internal void ShowTemplate2(MatchingTool matchingTool, double angle, double row, double col)
        {
            try
            {
                HOperatorSet.GetShapeModelContours(out matchingTool.contour, matchingTool.modelID, new HTuple(1)); //shapeMatchTool.contour 最后输出的轮廓对象   //通过modelID来生成目标轮廓contour
                //HHomMat2D homMat2D;
                //HOperatorSet.HomMat2dIdentity(out homMat2D);        //创建生成一个2D矩阵
                //HOperatorSet.HomMat2dTranslate(homMat2D, row, col, out homMat2D);     //2D平移矩阵
                //HOperatorSet.HomMat2dRotate(homMat2D, (HTuple)angle, (HTuple)row, (HTuple)col, out homMat2D);   //通过角度和xy来赋值旋转平移矩阵
                HHomMat2D homMat2D = new HHomMat2D();  //二维变换的齐次变换矩阵  
                homMat2D.VectorAngleToRigid(0, 0, 0, (HTuple)row, (HTuple)col, (HTuple)angle);    //从点和角度计算刚性仿射变换
                HOperatorSet.AffineTransContourXld(matchingTool.contour, out matchingTool.contour, homMat2D);   //有了放射矩阵homMat2D，计算后的shapeMatchTool.contour应该已经经过了修正    //使用旋转平移矩阵仿射变换来得到最新的目标轮廓contour
                _hWinDisplay.HWindowID.SetColor("green");
                _hWinDisplay.HWindowID.DispObj(matchingTool.contour);   //显示最终的目标轮廓

                HObject outBoundary, inBoundary;
                HOperatorSet.Boundary(matchingTool.templateRegion.hobject, out outBoundary, "inner_filled");
                HOperatorSet.Boundary(matchingTool.templateRegion.hobject, out inBoundary, "outer");
                _hWinDisplay.HWindowID.DispObj(matchingTool.totalRegion);   //显示模板区域

                _hWinDisplay.HWindowID.SetColor("red");
                _hWinDisplay.HWindowID.DispObj(matchingTool.searchRegion.hobject);   //显示搜索框
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 绘画出模型
        /// </summary>
        protected void PaintMetrology()
        {
            if (m_ModuleObj.hv_img != null && m_ModuleObj.hv_img.IsInitialized())
            {
                HImage hImage = new HImage(m_ModuleObj.hv_img);
                _hWinDisplay.Image = hImage;  //经过New的转换，保证himage的纯净的himage

                foreach (MeasureROI roi in m_ModuleObj.hv_img.measureROIlist)
                {
                    if (roi != null && roi.roiType == enMeasureROIType.文字显示.ToString())
                    {
                        MeasureROIText roiText = (MeasureROIText)roi;
                        ///窗体显示字体部分，先不加
                        ///HalconControl.CHelper.set_display_font(hWindow_Fit.HWindowID, roiText.size, roiText.font, "false", "false");
                        ///HalconControl.CHelper.disp_message(hWindow_Fit.HWindowID, roiText.text, "image", roiText.row, roiText.col, roiText.drawColor, "false");
                    }
                    else
                    {
                        if (roi != null && roi.hobject.IsInitialized())
                        {
                            _hWinDisplay.HWindowID.SetColor(roi.drawColor);
                            _hWinDisplay.HWindowID.DispObj(roi.hobject);
                        }
                    }
                }
            }
        }


        #endregion

    }

    /// <summary>
    /// 画布相关
    /// </summary>
    class CanvasItem    //画布相关
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }
}
