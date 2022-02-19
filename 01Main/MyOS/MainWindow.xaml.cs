using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Heart.Inward;
using Heart.Outward;
using System.Diagnostics;   //wpf输出打印的命名空间
using MyOS.Common.communacation;
using HalconDotNet;

namespace MyOS
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        #region 变量定义及初始化

        /// <summary>
        /// 流程栏信息记录的变量
        /// </summary>
        public List<ModuleTreeExt> ModuleTreeExtList = new List<ModuleTreeExt>();   //ModuleTreeExtList是存在与MainWindow内的List<ModuleTreeExt>，用于添加或删除流程时，主界面可以方便记录和删除信息
        private DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
            Solution.Instance.UpdataIteam += UpdataStaus;
        }

        /// <summary>
        /// 更新状态栏
        /// </summary>
        void UpdataStaus(string info)
        {
            m_StatusBar.Text = info;
        }

        /// <summary>
        /// 窗体初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _splitHWindowFitExt.RePaint += UpdataUI;
            Project.s_SplitHWindowFitExt = _splitHWindowFitExt;
            Project.s_OutputListView = _outputListView;
            Project.s_OutputLabel = _outputLabel;
        }

        /// <summary>
        /// 定时器--线程管理和界面显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            // m_StatusBar.Text = Solution.Instance.ProjectIteam;
            ModuleTreeExt cModuleTree = _tabControl.SelectedContent as ModuleTreeExt;
            if (cModuleTree != null)
            {
                //Trace.WriteLine("当前是流程" + cModuleTree.ProjectID);
                if (Solution.Instance.GetStatesById(cModuleTree.ProjectID) == true)     //通过当前的流程id来获取流程的运行状态，实现了切换不同流程时可以显示不同的运行状态和不同的操作界面这种效果
                {
                    ProjectExecuteOnce.IsEnabled = false;
                    ProjectStartRun.IsEnabled = false;
                    ProjectStopRun.IsEnabled = true;
                }
                else
                {
                    ProjectExecuteOnce.IsEnabled = true;
                    ProjectStartRun.IsEnabled = true;
                    ProjectStopRun.IsEnabled = false;
                }

            }

            if (Solution.Instance.GetStates() == true)
            {
                AllProjectStartRun.IsEnabled = false;
                AllProjectStopRun.IsEnabled = true;
            }
            else
            {
                AllProjectStartRun.IsEnabled = true;
                AllProjectStopRun.IsEnabled = false;
            }
        }

        /// <summary>
        /// 窗体--关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            EComManageer.DisConnectAll();   //关闭所有的通讯
            HOperatorSet.CloseAllFramegrabbers();   //关闭对于Halcon下的DirectShow相机连接
        }
        #endregion


        #region 界面UI事件

        #region 设置栏

        /// <summary>
        /// 文件--新建
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _NewProject_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dr = System.Windows.MessageBox.Show("是否保存当前工程", "提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (dr == MessageBoxResult.Yes)
            {
                Solution.Instance.SaveConfig(Solution.sConfigPath);     //用户按下确定后，保存下当前的工程
                Solution.Instance.DeleteSolution();         //清空工程的一些信息
                DeleteModuleTree();                         //清除流程栏的树信息
                EComManageer.DisConnectAll();               //关闭所有的通讯
                EComManageer.s_ECommunacationDic.Clear();   //删除核心的通讯储存信息
                Solution.CloseAllDev();                     //关闭所有的相机
                Solution.g_AcqDeviceList.Clear();           //删除核心的变量储存信息
                m_StatusBar.Text = "新建方案！";
            }
            else if (dr == MessageBoxResult.No)
            {
                Solution.Instance.DeleteSolution();         //清空工程的一些信息
                DeleteModuleTree();                         //清除流程栏的树信息
                EComManageer.DisConnectAll();               //关闭所有的通讯
                EComManageer.s_ECommunacationDic.Clear();   //删除核心的通讯储存信息
                Solution.CloseAllDev();                     //关闭所有的相机
                Solution.g_AcqDeviceList.Clear();           //删除核心的变量储存信息
                m_StatusBar.Text = "新建方案！";
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// 文件--打开...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _OpenProject_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.FileName = System.Environment.CurrentDirectory;
            fileDialog.Filter = "(*.os)|*.os";

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //在工程内打开项目，也要先清空现场的项目信息，才能初始化加载其他的
                Solution.Instance.DeleteSolution();         //清空工程的一些信息
                DeleteModuleTree();                         //清除流程栏的树信息
                EComManageer.DisConnectAll();               //关闭所有的通讯
                EComManageer.s_ECommunacationDic.Clear();   //删除核心的通讯储存信息
                Solution.CloseAllDev();                     //关闭所有的相机
                Solution.g_AcqDeviceList.Clear();           //删除核心的变量储存信息

                string files = fileDialog.FileName;
                Solution.Instance.InitialVisionProgram(files);  //初始化视觉工程项目
                InitModuleTreeExt();    //初始化并还原ModuleTreeExt
                _splitHWindowFitExt.SetDisplayNumber(Solution.Instance.GetHWindowNumber());
                m_StatusBar.Text = "打开" + fileDialog.FileName + "方案！";
            }

        }

        /// <summary>
        /// 文件--保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _SaveProject_Click(object sender, RoutedEventArgs e)
        {

            int hw_number = _splitHWindowFitExt.GetDisplayNumber();
            Solution.Instance.SaveConfig(Solution.sConfigPath);
            m_StatusBar.Text = "保存成功！";
        }

        /// <summary>
        /// 文件--另存为
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _Saveas_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.FileName = System.Environment.CurrentDirectory;
            fileDialog.Filter = "(*.os)|*.os";

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string files = fileDialog.FileName;
                Solution.sConfigPath = files;
                Solution.Instance.SaveConfig(files);
                m_StatusBar.Text = "保存成功！";
            }
        }

        /// <summary>
        /// 设置--图像窗口设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuSettingImageDisplay_Click(object sender, RoutedEventArgs e)
        {
            DialogSettingImageDisplay dialogSettingImageDisplay = new DialogSettingImageDisplay();

            dialogSettingImageDisplay.CanvasNumber = _splitHWindowFitExt.GetDisplayNumber();
            if (dialogSettingImageDisplay.ShowDialog() == true)
            {
                _splitHWindowFitExt.SetDisplayNumber(dialogSettingImageDisplay.CanvasNumber);
                Solution.Instance.SetHWindowNumber(dialogSettingImageDisplay.CanvasNumber);
            }
        }

        #endregion

        #region 配置栏

        /// <summary>
        /// 配置栏--相机设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _buttonCamera_Click(object sender, RoutedEventArgs e)
        {
            CameraSetting.Instance.Show();      //相机设置对于项目来说是唯一的实例，所以直接show即可
        }

        /// <summary>
        /// 配置栏--通讯管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _buttonCommunication_Click(object sender, RoutedEventArgs e)
        {
            CommunicationManagement.Instance.Show();    //通讯配置对于项目来说是唯一的实例，所以直接show即可
        }

        /// <summary>
        /// 配置栏--变量管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _buttonVariable_Click(object sender, RoutedEventArgs e)
        {
            List<VariableInfo> DialogGlobalVariableInfoList = CloneObject.DeepCopy(Solution.Instance.GetGlobalVariableInfoList());
            DialogGlobalVariable dialogGlobalVariable = new DialogGlobalVariable(DialogGlobalVariableInfoList);
            if (dialogGlobalVariable.ShowDialog() == true)
            {
                Solution.Instance.UpdateGlobalVariable(dialogGlobalVariable.GlobalVariableInfoCollection.ToList());
            }
        }

        #endregion

        #region 流程栏

        /// <summary>
        /// 流程栏--新建流程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectNew_Click(object sender, RoutedEventArgs e)
        {
            ModuleTreeExt moduleTreeExt = new ModuleTreeExt();                          //新建一个ModuleTreeExt 流程栏基本对象
            moduleTreeExt.ProjectID = Solution.Instance.CreateProject();                //CreateProject()为工程产生新对象的同时，也返回ProjectID。赋值给了
            Project prj = Solution.Instance.GetProjectById(moduleTreeExt.ProjectID);    //根据id获取对应的project
            prj.ModuleTreeExt = moduleTreeExt;                                          //这一步实现了把UI建立的moduleTreeExt传到Project里面去
            TabItem tabItem = new TabItem();                                            //在流程栏处，实例一个对象出来
            tabItem.Header = "流程" + moduleTreeExt.ProjectID.ToString();               //给流程栏标题添加流程0、1、2、3、4
            _tabControl.Items.Add(tabItem);                                             //流程栏UI添加该对象
            tabItem.Content = moduleTreeExt;                                            //流程栏UI界面Content内容绑定对象信息
            ModuleTreeExtList.Add(moduleTreeExt);                                       //主窗体的相关变量ModuleTreeExtList添加信息
            _tabControl.SelectedIndex = ModuleTreeExtList.Count - 1;                    //_tabControl选择最后一个元素
        }

        /// <summary>
        /// 流程栏--删除流程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectDelete_Click(object sender, RoutedEventArgs e)
        {
            ModuleTreeExt cModuleTree = _tabControl.SelectedContent as ModuleTreeExt;   //通过鼠标UI来获取当前是哪个ModuleTreeExt
            if (cModuleTree != null)                                                    //判断当前选到的cModuleTree是否有效
            {
                Solution.Instance.DeleteProject(cModuleTree.ProjectID);                 //根据ID号来删除流程
                _tabControl.Items.Remove(_tabControl.SelectedItem);                     //UI的_tabControl移除该项_tabControl.SelectedItem
                ModuleTreeExtList.Remove(cModuleTree);                                  //主窗体的相关变量ModuleTreeExtList移除信息
            }
        }

        /// <summary>
        /// 流程栏--编辑流程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectSetting_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 流程栏--执行一次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectExecuteOnce_Click(object sender, RoutedEventArgs e)
        {
            ModuleTreeExt cModuleTree = _tabControl.SelectedContent as ModuleTreeExt;
            if (cModuleTree != null)
            {
                Solution.Instance.ExecuteOnce(cModuleTree.ProjectID);
            }
        }

        /// <summary>
        /// 流程栏--循环运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectStartRun_Click(object sender, RoutedEventArgs e)
        {
            ModuleTreeExt cModuleTree = _tabControl.SelectedContent as ModuleTreeExt;
            if (cModuleTree != null)
            {
                Solution.Instance.StartRun(cModuleTree.ProjectID);
            }
        }





        /// <summary>
        /// 工具栏-启动所有流程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void All_ProjectStartRun_Click(object sender, RoutedEventArgs e)
        {

            if (_tabControl.Items.Count > 0)
            {
                foreach (TabItem item in _tabControl.Items)
                {

                    ModuleTreeExt cModuleTree = item.Content as ModuleTreeExt;
                    if (cModuleTree != null)
                    {
                        Solution.Instance.StartRun(cModuleTree.ProjectID);
                    }
                }
            }

        }


        /// <summary>
        /// 工具栏-一次启动所有流程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void All_ProjectOnceStartRun_Click(object sender, RoutedEventArgs e)
        {

            if (_tabControl.Items.Count > 0)
            {
                foreach (TabItem item in _tabControl.Items)
                {

                    ModuleTreeExt cModuleTree = item.Content as ModuleTreeExt;
                    if (cModuleTree != null)
                    {
                        Solution.Instance.ExecuteOnce(cModuleTree.ProjectID);
                    }
                }
            }
        }

        /// <summary>
        /// 流程栏--停止运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void All_ProjectStopRun_Click(object sender, RoutedEventArgs e)
        {

            if (_tabControl.Items.Count > 0)
            {
                foreach (TabItem item in _tabControl.Items)
                {

                    ModuleTreeExt cModuleTree = item.Content as ModuleTreeExt;
                    if (cModuleTree != null)
                    {
                        Solution.Instance.StartRun(cModuleTree.ProjectID);
                    }
                    Solution.Instance.StopRun(cModuleTree.ProjectID);

                    //为了防止有些通讯还留有阻塞的信号，所以全部清空
                    List<EComInfo> infos = EComManageer.GetKeyList();
                    foreach (EComInfo imfo in infos)
                    {
                        EComManageer.StopRecStrSignal(imfo.Key);    //停止阻塞
                    }
                }
            }
        }

        /// <summary>
        /// 流程栏--停止运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectStopRun_Click(object sender, RoutedEventArgs e)
        {
            ModuleTreeExt cModuleTree = _tabControl.SelectedContent as ModuleTreeExt;
            if (cModuleTree != null)
            {
                Solution.Instance.StopRun(cModuleTree.ProjectID);

                //为了防止有些通讯还留有阻塞的信号，所以全部清空
                List<EComInfo> infos = EComManageer.GetKeyList();
                foreach (EComInfo imfo in infos)
                {
                    EComManageer.StopRecStrSignal(imfo.Key);    //停止阻塞
                }
            }
        }

        #endregion

        #endregion


        #region 相关辅助函数

        /// <summary>
        /// 初始化并还原ModuleTreeExt
        /// </summary>
        private void InitModuleTreeExt()
        {
            _tabControl.Items.Clear();  //先清空流程栏UI的_tabControl，不然再加进去就会重复冲突
            ModuleTreeExtList.Clear();  //ModuleTreeExtList是存在与MainWindow内的List<ModuleTreeExt>，用于添加或删除流程时，主界面可以方便记录和删除信息    //故重新初始化时要清空它
            Solution.Instance.UpdataIteam += UpdataStaus;
            foreach (Project item in Solution.m_ProjectList)
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = "流程" + item.ModuleTreeExt.ProjectID.ToString();
                _tabControl.Items.Add(tabItem);             //UI上的内容添加
                tabItem.Content = item.ModuleTreeExt;       //流程栏树的内容绑定item.ModuleTreeExt
                item.UpdataIteam += UpdataStaus;
                ModuleTreeExtList.Add(item.ModuleTreeExt);  //ModuleTreeExtList是MainWindow内的List，可能没什么用

            }
            _tabControl.SelectedIndex = 0;  //添加了多个之后呢，重新选择第一个来选中
        }

        private void DeleteModuleTree()
        {
            _tabControl.Items.Clear();  //清空ui的_tabControl
            ModuleTreeExtList.Clear();  //清空List<ModuleTreeExt> ModuleTreeExtList
        }

        //通知更新UI
        private void UpdataUI()
        {
            //   this.UpdateLayout();
          //  this._splitHWindowFitExt.IVL
         }
        #endregion


    }
}
