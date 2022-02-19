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
using System.IO;
using AcqDevice;
using Heart.Outward;

namespace Plugin.ImageAcquire
{
    /// <summary>
    /// ModuleForm.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleForm : ModuleFormBase
    {
        #region 变量定义及初始化

        private ModuleObj m_ModuleObj;
        private List<CanvasItem> CanvasList = new List<CanvasItem>();
        public int ImageListIndex = 0;  //图像列表索引
        public ModuleForm()
        {
            InitializeComponent();
            Specify.Visibility = Visibility.Visible;
            Directory.Visibility = Visibility.Collapsed;
            Camera.Visibility = Visibility.Collapsed;
            //_LinkImage
        }

        #endregion

        private void _GetImage_Click(object sender, RoutedEventArgs e)
        {
            VariableTable variableTable = new VariableTable();    //打开变量表
            variableTable.SetModuleInfo(m_ModuleObj.Info.ProjectID, m_ModuleObj.Info.ModuleName, "string");  //加载表内内容？？？ //20211018传入图像格式改造为HImageExt

            //打开并维持窗体
            if (variableTable.ShowDialog() == true)     //变量窗体显示
            {
                //数据操作部分
                m_ModuleObj.input_str = variableTable.SelectedVariableText;    //通过list对话框，已经选择了要引用的变量
                _inputImage.Text = m_ModuleObj.input_str;
                m_ModuleObj.ReadImg();

            }
        }

   

        #region 插件模块保存与执行

        /// <summary>
        /// 加载插件时初始化
        /// </summary>
        public override void LoadModule()
        {
            m_ModuleObj = (ModuleObj)ModuleObjBase;         //把module实例拿回来
            Title = m_ModuleObj.Info.ModuleName;            //设置插件的标题
            //指定图像的相关信息
            if (!string.IsNullOrEmpty(m_ModuleObj.ImagePath))
            {
               // _imagePath.Text = m_ModuleObj.ImagePath;    //指定图像的图像路径
            }
            CanvasList.Clear();                             //遍历添加当前可选择的画布
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
            _displayCanvas.SelectedValue = m_ModuleObj.CanvasIndex;     //上面到这里全部都是画布相关的还原东西
            m_ModuleObj.index = 0;  //m_ModuleObj.index是文件目录时当前图像在List的索引，怕上一次的索引大过下一次的。所以每次插件重新打开时，索引至0

        }

        /// <summary>
        /// 点击执行按钮后前序函数
        /// </summary>
        public override void RunModuleBefore()
        {
            //根据当前UI插件的采集设备设定来修改moduleobj内的相关变量
            if (_SpecifyImage.IsChecked.Value)
            {
                m_ModuleObj.IsSpecify = true;
                m_ModuleObj.IsFileDirectory = false;
                m_ModuleObj.IsCamera = false;
            }
            else if (_FileDirectory.IsChecked.Value)
            {
                m_ModuleObj.IsFileDirectory = true;
                m_ModuleObj.IsSpecify = false;
                m_ModuleObj.IsCamera = false;
            }
            else
            {
                m_ModuleObj.IsCamera = true;
                m_ModuleObj.IsFileDirectory = false;
                m_ModuleObj.IsSpecify = false;
                m_ModuleObj.exposure_time = int.Parse(_Exposure.Text);
                m_ModuleObj.gain = int.Parse(_gain.Text);
            }
            m_ModuleObj.m_DeviceID = _SelectCamera.Text.Trim();  //储存m_DeviceID采集设备下拉框
        }

        /// <summary>
        /// 点击执行按钮后前序函数，中间还包含着Module内的ExeModule执行函数
        /// </summary>
        public override void RunModuleAfter()
        {
            if (_SpecifyImage.IsChecked.Value)
            {
                SpecImageShow();
            }
            else if (_FileDirectory.IsChecked.Value)
            {
                FileDirectoryShow();
            }
            else
            {
                CameramethodShow();
            }
        }

        /// <summary>
        /// 点击确定会执行的唯一函数
        /// </summary>
        public override void SaveModuleBefore()
        {
            m_ModuleObj.CanvasIndex = (int)_displayCanvas.SelectedValue;    //采集插件目前只有两个变量，一个是图像路径、一个是画布序号。图像路径在预览的时候已经有了，所以只要保存画布序号即可
            //20211022防止用户只单点保存，所以要把执行的函数内容也放在这里
            //根据当前UI插件的采集设备设定来修改moduleobj内的相关变量
            if (_SpecifyImage.IsChecked.Value)
            {
                m_ModuleObj.IsSpecify = true;
                m_ModuleObj.IsFileDirectory = false;
                m_ModuleObj.IsCamera = false;
            }
            else if (_FileDirectory.IsChecked.Value)
            {
                m_ModuleObj.IsFileDirectory = true;
                m_ModuleObj.IsSpecify = false;
                m_ModuleObj.IsCamera = false;
            }
            else
            {
                m_ModuleObj.IsCamera = true;
                m_ModuleObj.IsFileDirectory = false;
                m_ModuleObj.IsSpecify = false;
                m_ModuleObj.exposure_time = int.Parse(_Exposure.Text);
                m_ModuleObj.gain = int.Parse(_gain.Text);
            }
        }

        #endregion


        #region 插件UI事件

        /// <summary>
        /// 采集模式--切换到指定图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _SelectSpecifyImage_Click(object sender, RoutedEventArgs e)    //指定图像RadioButton
        {
            Specify.Visibility = Visibility.Visible;
            Directory.Visibility = Visibility.Collapsed;
            Camera.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 采集模式--切换到文件目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _SelectFileDirectory_Click(object sender, RoutedEventArgs e)   //文件目录FileDirectory RadioButton
        {
            Specify.Visibility = Visibility.Collapsed;
            Directory.Visibility = Visibility.Visible;
            Camera.Visibility = Visibility.Collapsed;

            if (!string.IsNullOrEmpty(m_ModuleObj.FileDirectoryPath))
            {
                _FileDirectoryPath.Text = m_ModuleObj.FileDirectoryPath;
            }
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
            _ImageList.ItemsSource = m_ModuleObj.imagelist;     //从m_ModuleObj拿到图像显示列表imagelist并赋值到控件上
            _ImageList.Items.Refresh();                         //刷新控件
        }

        /// <summary>
        /// 采集模式--切换到相机
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _SelectCamera_Click(object sender, RoutedEventArgs e)  // 相机 _Camera RadioButton
        {
            Specify.Visibility = Visibility.Collapsed;
            Directory.Visibility = Visibility.Collapsed;
            Camera.Visibility = Visibility.Visible;

            List<string> strList = new List<string>();
            foreach (AcqAreaDeviceBase item in Solution.g_AcqDeviceList)
            {
                strList.Add(item.m_DeviceID);     //m_UniqueName是相机的名称
            }
            _SelectCamera.ItemsSource = strList;  //ui的连接使用新的List<string> strList

            
            
            ////_SelectCamera.Items.Clear();
            ////foreach (string i in DeviceInfo.CameraList.Select(p => p.Name).ToList())
            ////{
            ////    _SelectCamera.Items.Add(i);
            ////}
            ////_SelectCamera.Text = m_ModuleObj.CameraName;
            ////_CameraNote.Text = m_ModuleObj.CameraRemark;
            ////_Exposure.Text = m_ModuleObj.exposure_time.ToString();
            ////_gain.Text = m_ModuleObj.gain.ToString();
        }

        /// <summary>
        /// 指定图像--图像选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _pathBrowser_Click(object sender, RoutedEventArgs e)
        {
         //   GetPath(_imagePath);
         //   m_ModuleObj.ImagePath = _imagePath.Text;
        }

        /// <summary>
        /// 文件目录--路径选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _browseDirectory_Click(object sender, RoutedEventArgs e)
        {
            m_ModuleObj.imagelist.Clear();
            _ImageList.ItemsSource = m_ModuleObj.imagelist;
            _ImageList.Items.Refresh();
            GetListFiles();
            m_ModuleObj.FileDirectoryPath = _FileDirectoryPath.Text;
        }

        /// <summary>
        /// 文件目录--图像列表索引切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _ImageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ImageList.Items.Count > 0)
            {
                _hWinDisplay.ClearHWindow();
                if (_ImageList.SelectedIndex != -1)
                {
                    _hWinDisplay.Image = new HImage(m_ModuleObj.imagelist.Select(p => p.ImagePath).ToList()[_ImageList.SelectedIndex]);
                }
                _hWinDisplay.Image.Dispose();
            }
        }

        /// <summary>
        /// 相机--相机设备更改选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _SelectCamera_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ////if (_SelectCamera.SelectedItem == null)
            ////{
            ////    return;
            ////}
            ////_CameraNote.Text = list.Find(p => p.Name == _SelectCamera.SelectedValue.ToString()).Remark;
            ////m_ModuleObj.cameralist.Add(_CameraNote.Text);   //把不同型号的相机放进相机列表
            ////m_ModuleObj.CameraRemark = _CameraNote.Text;
        }

        #endregion


        #region 相关辅助函数

        /// <summary>
        /// 指定图像--打开对话框拿到图像路径
        /// </summary>
        /// <param name="textBox"></param>
        public void GetPath(System.Windows.Controls.TextBox textBox)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = m_ModuleObj.Info.ModuleName;
            if (m_ModuleObj.ImagePath != null)
            {
                fileDialog.InitialDirectory = m_ModuleObj.ImagePath;
            }

            fileDialog.Filter = "(*.jpg;*.png;*.bmp;*.tif)|*.jpg;*.png;*.bmp;*.tif";

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox.Text = fileDialog.FileName;
            }
        }

        /// <summary>
        /// 文件目录--打开对话框拿到文件夹路径及图像
        /// </summary>
        public void GetListFiles()     //选择文件夹 遍历文件夹中的图片
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件夹";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _FileDirectoryPath.Text = dialog.SelectedPath;
            }
            else { return; }
            DirectoryInfo info = new DirectoryInfo(_FileDirectoryPath.Text);    //using System.IO;
            FileInfo[] fileInfos = info.GetFiles().Where(f => f.Name.EndsWith(".jpg") || f.Name.EndsWith(".png") || f.Name.EndsWith(".bmp")).ToArray();  //选择指定格式的文件
            ImageListIndex = 0;
            if (fileInfos.Length != 0)
            {
                m_ModuleObj.ImagePathList.Clear();  //因为每次都要重新加的，所以到了这里就把它给清空掉
                foreach (FileInfo f in fileInfos)
                {
                    m_ModuleObj.imagelist.Add(new ModuleObj.Image(ImageListIndex, f.Name, f.FullName));     //将获取到了路径下的图像文件的信息添加到imagelist内，这里添加的单元是 Image(int index, string imagename, string imagepath)，该类是专门定义了原来储存和显示
                    _ImageList.ItemsSource = m_ModuleObj.imagelist;
                    _ImageList.Items.Refresh();
                    m_ModuleObj.ImagePathList.Add(f.FullName);     //文件夹路径的图像List   最后整个List会保存图像的全部路径
                    ImageListIndex++;
                }
            }
        }



        /// <summary>
        /// 指定图像--插件内执行显示
        /// </summary>
        public void SpecImageShow()    //指定图像函数
        {
            if (m_ModuleObj.AcquiredImage != null)
            {
                _hWinDisplay.ClearHWindow();
                _hWinDisplay.Image = m_ModuleObj.AcquiredImage;
            }
            else
            {
                _hWinDisplay.ClearHWindow();
            }
        }

        /// <summary>
        /// 文件目录--插件内执行显示
        /// </summary>
        public void FileDirectoryShow()    //文件目录函数
        {
            if (m_ModuleObj.AcquiredImage != null)
            {
                _hWinDisplay.ClearHWindow();
                _hWinDisplay.Image = m_ModuleObj.AcquiredImage;
            }
            else { _hWinDisplay.ClearHWindow(); }
        }

        /// <summary>
        /// 相机--插件内执行显示
        /// </summary>
        public void CameramethodShow()    //相机函数
        {
            if (m_ModuleObj.AcquiredImage != null)
            {
                _hWinDisplay.ClearHWindow();
                _hWinDisplay.Image = m_ModuleObj.AcquiredImage;
            }
            else { _hWinDisplay.ClearHWindow(); }

        }

        #endregion

    }
    /// <summary>
    /// 画布相关
    /// </summary>
    class CanvasItem
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }
}
