using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Heart.Outward;
using HalconDotNet;
using System.Runtime.Serialization;
//Project.cs类负责单个流程的函数方法，而Solution.cs类用于多线程时管理多个流程
//Project.cs类中包含数据操作的函数，所以并不是单独成立的库 OutputVariable

namespace Heart.Inward
{
    /// <summary>
    /// 流程信息
    /// </summary>
    [Serializable]
    public class ProjectInfo
    {
        public int ProjectID { get; set; }           //项目ID
        public string ProjectName { get; set; }       //项目  名称
    }

    /// <summary>
    /// 流程状态
    /// </summary>
    [Serializable]
    public enum RunMode
    {
        None = 0,
        RunOnce = 1,//运行一次
        RunCycle = 2,//循环运行
    }
    [Serializable]
    public delegate void UpdataIteamInfo(string info);
    [Serializable]
    public class Project
    {
        public List<ModuleObjBase> m_ModuleObjList = new List<ModuleObjBase>();//模块列表   //当前工程对应的模块列表List
        private List<KeyValuePair<string, string>> m_ModuleEntryList = new List<KeyValuePair<string, string>>();

        //线程控制
        [field:NonSerialized]
        private Thread m_Thread;
        [field: NonSerialized]
        private AutoResetEvent m_AutoResetEvent = new AutoResetEvent(false);//控制流程
        private bool m_ThreadStatus = false;

        //项目信息
        public ProjectInfo ProjectInfo { set; get; } = new ProjectInfo();//项目信息
        public RunMode RunMode { set; get; } = RunMode.None;//运行模式

        //输出容器 每个模块 都把自己的输出放到该容器中
        private Dictionary<string, Dictionary<string, object>> m_VariableDataDict = new Dictionary<string, Dictionary<string, object>>();
        private Dictionary<string, List<VariableInfo>> m_VariableInfoList = new Dictionary<string, List<VariableInfo>>();

        private Dictionary<int, object> m_DisplayImages = new Dictionary<int, object>();    //插件输出的图像，要显示在画布上。插件在调用AddDisplayImage函数时，更新了图像输出，然后在核心执行函数时，会遍历这些内容并显示
        private Dictionary<int, object> m_DisplayRois = new Dictionary<int, object>();      //插件输出的区域信息，要显示在画布上。插件在调用AddDisplayImage函数时，更新了区域输出，然后在核心执行函数时，会遍历这些内容并显示

        [field: NonSerialized]
        public ModuleTreeExt ModuleTreeExt { set; get; }    //1023涉及UI的类不能序列化，后面要考虑重新定义一个类去保存里面的重要参数。的时候反序列化的时候就把类的值传回给ModuleTreeExt对象，然后再调ModuleTreeExt内的刷新树的函数来实现重新显示的功能
        [field: NonSerialized]
        public static SplitHWindowFitExt s_SplitHWindowFitExt { set; get; }
        [field: NonSerialized]
        public static ListView s_OutputListView { set; get; }
        [field: NonSerialized]
        public static Label s_OutputLabel { set; get; }
        public string OutputTitle { set; get; }
        [field: NonSerialized]
        public DataTable OutputDataTable = new DataTable();
        [field: NonSerialized]
        public bool isSelected = false;

        public static ImageSource OKIcon { get; set; }
        public static ImageSource NGIcon { get; set; }
        public static ImageSource WaitingIcon { get; set; }
        public static ImageSource DisableIcon { get; set; }

        //注册委托用于返回信息   
        [field: NonSerialized]
        public event UpdataIteamInfo UpdataIteam;

        /// <summary>
        /// 更新传送信息
        /// </summary>
        /// <param name="updata"></param>
        private void Updata(string updata)
        {
            if (UpdataIteam != null)
            {
                UpdataIteam(updata);
            }
        }
        /// <summary>
        /// new的流程Project
        /// </summary>
        public Project()//定义了一个流程该有的元素，供添加流程使用
        {
            OutputDataTable.Columns.Add("OutputVariable");
            OutputDataTable.Columns.Add("OutputValue");
            m_Thread = new Thread(Process);    //每个流程的新建都会new一份线程出来并开启后台的线程
            m_Thread.IsBackground = true;
            m_Thread.Start();
        }

        /// <summary>
        /// 流程Project初始化，加载了Icon
        /// </summary>
        public static void Init()//该函数必须做为初始化函数被程序一开始被调用
        {
            OKIcon = new BitmapImage(new Uri("pack://application:,,,/" + "Heart" + ";Component/Icon/OK.png"));
            NGIcon = new BitmapImage(new Uri("pack://application:,,,/" + "Heart" + ";Component/Icon/NG.png"));       
            DisableIcon = new BitmapImage(new Uri("pack://application:,,,/" + "Heart" + ";Component/Icon/disable.png"));
            WaitingIcon = new BitmapImage(new Uri("pack://application:,,,/" + "Heart" + ";Component/Icon/waiting.png"));
        }

        /// <summary>
        /// 流程Project运行线程开启
        /// </summary>
        public void Start()//运行函数，启动线程
        {
            //流程开启运行时，其实只是置位了信号，所以重复开启理论上是不会报错的
            m_ThreadStatus = true;  //开启线程置位信号
            m_AutoResetEvent.Set(); //准备事件，开启事件
        }

        /// <summary>
        /// 流程Project运行线程停止
        /// </summary>
        public void Stop()//结束函数，关闭线程
        {
            m_ThreadStatus = false; //关闭进程即结束流程运行
        }

        /// <summary>
        /// 流程Project获取线程状态
        /// </summary>
        /// <returns></returns>
        public bool GetThreadStatus()//获取线程状态
        {
            return m_ThreadStatus;
        }

        /// <summary>
        /// 流程Project添加Module单元
        /// </summary>
        /// <param name="moduleObjBase"></param>
        public void AddModuleObj(ModuleObjBase moduleObjBase)//List<ModuleObjBase>内添加
        {       
            m_ModuleObjList.Add(moduleObjBase);     //List<ModuleObjBase> m_ModuleObjList;
            Updata("流程"+moduleObjBase.Info.ProjectID+"添加模块：" +moduleObjBase.Info.ModuleName);
        }

        /// <summary>
        /// 流程Project清除m_ModuleObjList
        /// </summary>
        public void ClearModuleObjList()//List<ModuleObjBase>删除
        {
            m_ModuleObjList.Clear();
            Updata("清除完成！");
        }

        /// <summary>
        /// 记录添加进来的Module信息
        /// </summary>
        /// <param name="moduleEntry"></param>
        /// <param name="moduleName"></param>
        public void AddModuleEntry(string moduleEntry,string moduleName)//List<KeyValuePair<string, string>>内添加
        {
            m_ModuleEntryList.Add(new KeyValuePair<string, string>(moduleEntry, moduleName));
        }

        /// <summary>
        /// 清空记录的Module信息
        /// </summary>
        public void ClearModuleEntryList()//清除List    m_ModuleEntryList存放了由工具区拖动过来到放置区的模块列表
        {
            m_ModuleEntryList.Clear();
        }

        /// <summary>
        /// 获取m_ModuleObjList所有模块名称
        /// </summary>
        /// <returns></returns>
        public List<string> GetModuleNameList()//获取所有模块名称
        {
            return m_ModuleObjList.Select(c => c.Info.ModuleName).ToList();
        }

        /// <summary>
        /// 根据模块名称找m_ModuleObjList对应模块
        /// </summary>
        /// <returns></returns>
        public ModuleObjBase GetModuleByName(string moduelName)//获取所有模块名称
        {
            return m_ModuleObjList.FirstOrDefault(c => c.Info.ModuleName == moduelName);
        }

        /// <summary>
        /// 还原模块数据
        /// </summary>
        /// <param name="backModuleObjBase"></param>
        public void RecoverModuleObj(ModuleObjBase backModuleObjBase)//还原模块数据，用于模块取消按钮
        {
            int index = m_ModuleObjList.FindIndex(c => c.Info.ModuleName == backModuleObjBase.Info.ModuleName);
            m_ModuleObjList[index] = backModuleObjBase;
        }

        /// <summary>
        /// 更新模块的变量输出信息
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="infoList"></param>
        public void UpdateVariableInfo(string moduleName, List<VariableInfo> infoList)//更新变量表，写入当前模块变量信息
        {
            if (!m_VariableInfoList.ContainsKey(moduleName))    //Dictionary<string, List<VariableInfo>> m_VariableInfoList;
            {
                m_VariableInfoList[moduleName] = new List<VariableInfo>();  //如果List不包含当前模块的名字，就添加当前模块名的索引信息和空间
            }          

            m_VariableInfoList[moduleName] = infoList;  //将新的信息写入到当前索引的dictionary
        }

        /// <summary>
        /// 删除模块的变量输出信息
        /// </summary>
        /// <param name="moduleName"></param>
        public void DeleteVariableInfo(string moduleName)//更新变量表，删除当前模块变量信息
        {
            if (m_VariableInfoList.ContainsKey(moduleName))
            {
                m_VariableInfoList.Remove(moduleName);  //如果当前索引存在模块则删除掉
                Updata("流程"+ProjectInfo.ProjectID+":删除"+ moduleName);
            }
        }

        /// <summary>
        /// 通过模块名来获取模块变量
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public List<VariableInfo> GetVariableInfoByModule(string moduleName)
        {
            if (m_VariableInfoList.ContainsKey(moduleName))
            {
                return m_VariableInfoList[moduleName];
            }

            return null;
        }

        /// <summary>
        /// 通过模块和变量类型获取变量信息(变量名)
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="vType"></param>
        /// <returns></returns>
        public List<VariableInfo> GetVariableInfoByModuleWithType(string moduleName,string vType)//通过模块获取变量信息(变量名)
        {
            if (vType == "")
            {
                return GetVariableInfoByModule(moduleName);
            }

            if (m_VariableInfoList.ContainsKey(moduleName))
            {
                List<VariableInfo> sList = null;

                sList = m_VariableInfoList[moduleName].FindAll(c => c.vType == vType);

                return sList;
            }

            return null;
        }
        
        /// <summary>
        /// 获取包含着变量的所有模块名字List
        /// </summary>
        /// <returns></returns>
        public List<string> GetVariableModuleList()
        {
            return m_VariableInfoList.Keys.ToList();
        }

        /// <summary>
        /// 通过变量类型获取模块信息List
        /// </summary>
        /// <param name="vType"></param>
        /// <returns></returns>
        public List<string> GetVariableModuleListByType(string vType)
        {
            return (from d in m_VariableInfoList where d.Value.FindIndex(e => e.vType == vType) >= 0 select d.Key).ToList();    //m_VariableInfoList当前系统内的变量，包括全局变量和模块输出变量
        }

        /// <summary>
        /// 获取全局变量
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public List<string> GetAboveVariableModuleList(string moduleName)
        {
            if (moduleName == "")
            {
                return m_VariableInfoList.Keys.ToList();
            }

            List<string> aboveModuleList = new List<string>();
            foreach (KeyValuePair<string,string> item in  m_ModuleEntryList)    //m_ModuleEntryList存放了由工具区拖动过来到放置区的模块列表
            {
                if (item.Value != moduleName)
                {
                    aboveModuleList.Add(item.Value);    //模块列表
                }
                else if(item.Value.Contains("变量定义"))
                {
                    aboveModuleList.Add(item.Value);
                }
                else
                {
                    break;
                }
            }
            aboveModuleList.Add("全局变量");

            List<string> varModuleList = m_VariableInfoList.Keys.ToList();

            List<string> aboveVarModuleList = new List<string>();
            foreach (string md in varModuleList)
            {
                if(aboveModuleList.Contains(md))
                {
                    aboveVarModuleList.Add(md);
                }
            }

            return aboveVarModuleList;
        }

        /// <summary>
        /// 通过变量类型获取全局变量
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="vType"></param>
        /// <returns></returns>
        public List<string> GetAboveVariableModuleListByType(string moduleName, string vType)
        {
            if (vType == "")
            {
                return GetAboveVariableModuleList(moduleName);
            }

            if (moduleName == "")
            {
                return GetVariableModuleListByType(vType);
            }

            List<string> aboveModuleList = new List<string>();
            foreach (KeyValuePair<string, string> item in m_ModuleEntryList)    //m_ModuleEntryList当前流程栏内容模块
            {
                if (item.Value != moduleName)
                {
                    aboveModuleList.Add(item.Value);
                }
                else
                {
                    break;
                }
            }
            aboveModuleList.Add("全局变量");

            List<string> varModuleList = GetVariableModuleListByType(vType);

            List<string> aboveVarModuleList = new List<string>();
            foreach (string md in varModuleList)
            {
                if (aboveModuleList.Contains(md))
                {
                    aboveVarModuleList.Add(md);
                }
            }

            return aboveVarModuleList;
        }

        /// <summary>
        /// 更新修改输出变量数据
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="dataDict"></param>
        public void UpdateVariableData(string moduleName, Dictionary<string, object> dataDict)
        {
            if (!m_VariableDataDict.ContainsKey(moduleName))    //Dictionary<string, Dictionary<string, object>> m_VariableDataDict     //输出容器 每个模块 都把自己的输出放到该容器中
            {
                m_VariableDataDict[moduleName] = new Dictionary<string, object>();
            }

            m_VariableDataDict[moduleName] = dataDict;
        }

        /// <summary>
        /// 删除输出变量数据值
        /// </summary>
        /// <param name="moduleName"></param>
        public void DeleteVariableData(string moduleName)
        {
            if (m_VariableDataDict.ContainsKey(moduleName))
            {
                m_VariableDataDict.Remove(moduleName);
                Updata("流程" + ProjectInfo.ProjectID + ":删除" + moduleName);
            }
        }

        /// <summary>
        /// 通过变量名称来获取输出变量
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="vName"></param>
        /// <returns></returns>
        public object GetVariableDataByVarName(string moduleName,string vName)
        {
            if (m_VariableDataDict.ContainsKey(moduleName))
            {
                Dictionary<string, object> dDict = m_VariableDataDict[moduleName];

                if (dDict.ContainsKey(vName))
                {
                    return CloneObject.DeepCopy(dDict[vName]);
                }
            }

            return null;
        }

        /// <summary>
        /// 通过变量模块、名称和赋值的量去修改变量值
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="vName"></param>
        /// <param name="vValue"></param>
        /// <returns></returns>
        public bool SetVariableDataByVarName(string moduleName, string vName, object vValue)
        {
            if (m_VariableDataDict.ContainsKey(moduleName))
            {
                Dictionary<string, object> dDict = m_VariableDataDict[moduleName];

                if (dDict.ContainsKey(vName))
                {
                    dDict[vName] = vValue;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取项目进入的ModuleEntryList
        /// </summary>
        /// <returns></returns>
        public List<string> GetEntryList()
        {          
            return m_ModuleEntryList.Select(c => c.Key).ToList();
        }

        /// <summary>
        /// 输入模块名称，作为控件的显示
        /// </summary>
        /// <param name="moduleName"></param>
        public void DisplayOutputTitle(string moduleName)
        {
            OutputTitle = moduleName;
            s_OutputLabel.Content = OutputTitle;
        }
        
        /// <summary>
        /// 通过模块名称，显示输出信息
        /// </summary>
        /// <param name="moduleName"></param>
        public void DisplayOutputData(string moduleName)
        {
            if (!m_VariableDataDict.ContainsKey(moduleName))
            {
                s_OutputListView.ItemsSource = null;
            }
            else
            {
                List<VariableInfo> vList = m_VariableInfoList[moduleName];

                if (vList.Count <= 0)
                {
                    s_OutputListView.ItemsSource = null;
                }
                else
                {
                    s_OutputListView.ItemsSource = m_VariableInfoList[moduleName];
                }

            }           
            s_OutputListView.Items.Refresh();
        }

        /// <summary>
        /// 将对应序号的画布上放置图像
        /// </summary>
        /// <param name="canvasIndex"></param>
        /// <param name="image"></param>
        public void AddDisplayImage(int canvasIndex, object image)//将对应序号的画布上放置图像
        {
            m_DisplayImages[canvasIndex] = image;
        }

        /// <summary>
        /// 将对应序号的画布上放置ROI信息20211019
        /// </summary>
        /// <param name="canvasIndex"></param>
        /// <param name="image"></param>
        public void AddDisplayRoi(int canvasIndex, List<MeasureROI> rois)//将对应序号的画布上放置图像
        {
            m_DisplayRois[canvasIndex] = rois;
        }

        /// <summary>
        /// 流程运行
        /// </summary>
        private void Process()
        {
            while (true)
            {
                if (m_ThreadStatus == false)
                {
                    m_AutoResetEvent.WaitOne();//阻塞等待
                }
                else
                {
                    if (RunMode == RunMode.RunCycle) Thread.Sleep(100);//连续运行设置延时时间,你面速度过快
                   
                    int curEntryIndex = 0;
                    int nextEntryFamilyIndex = 0;
                    bool cResult = true;
                    string cModuleName;
                    string cEntryName;
                    DateTime tStart;
                    DateTime tEnd;
                    double tCostTime;
                    double[] costTimeArray = new double[m_ModuleEntryList.Count];

                    Array.Clear(costTimeArray, 0, costTimeArray.Length);

                    ModuleTreeExt.Dispatcher.Invoke(new Action(
                    delegate
                    {
                        ModuleTreeExt.ClearAllNodeCostTime();
                        ModuleTreeExt.ClearAllNodeStateImage();
                    }));

                    foreach (ModuleObjBase item in m_ModuleObjList)
                    {
                        item.ResetState();
                    }


                    bool clearCanv = false;
                    while (curEntryIndex< m_ModuleEntryList.Count)
                    {
                        if (m_ThreadStatus == false) break;

                        cModuleName = m_ModuleEntryList[curEntryIndex].Value;
                        cEntryName = m_ModuleEntryList[curEntryIndex].Key;
                        ModuleObjBase moduleObjectBase = GetModuleByName(cModuleName);
                        if (!moduleObjectBase.Info.IsUse)
                        {
                            curEntryIndex++;
                            continue;
                        }
                        tStart = System.DateTime.Now;
                        if (moduleObjectBase.CanvasIndex != -1)
                        {
                            if (clearCanv == false)
                            {
                                if (RunMode == RunMode.RunOnce || RunMode == RunMode.RunCycle)
                                {
                                    if (moduleObjectBase.CanvasIndex != -1)
                                    {
                                        s_SplitHWindowFitExt.ClearAssignDisplayRoi(moduleObjectBase.CanvasIndex);
                                    }
                                }
                                else
                                {
                                    s_SplitHWindowFitExt.ClearAllDisplayRoi();
                                }
                                clearCanv = true;
                            }
                        }


                        ModuleTreeExt.Dispatcher.Invoke(new Action(
                        delegate
                        {
                            ModuleTreeExtNode cNode = (ModuleTreeExt.GetNodeByEntryName(cEntryName));
                            cNode.StateImage = WaitingIcon;     //此处缺少实例化new会报错
                            cNode.IsRunning = true;
                        }));

                        nextEntryFamilyIndex = moduleObjectBase.ExecuteModule(cEntryName);     //以此函数为入口，可找到调用刷新画面的函数方法 Execute执行

                        s_SplitHWindowFitExt.Dispatcher.Invoke(new Action(  //此部分用于流程有多个模块要显示时进行遍历，单独一个模块显示用nextEntryFamilyIndex = moduleObjectBase.ExecuteModule();
                        delegate
                        {
                            foreach (var item in m_DisplayImages)   //m_DisplayImages用于存储画布和对应放置图像的关系，key为画布序号，value为对应的图像
                            {
                                s_SplitHWindowFitExt.SetDisplayImage(item.Key, item.Value); //一直调用来刷新显示窗口
                            }
                            foreach (var item in m_DisplayRois)   //m_DisplayImages用于存储画布和对应放置图像的关系，key为画布序号，value为对应的图像
                            {
                                s_SplitHWindowFitExt.SetDisplayRoi(item.Key, (List<MeasureROI>)item.Value);
                            }
                        }));
                        m_DisplayImages.Clear();    //每次执行显示图像后就清空
                        m_DisplayRois.Clear();      //每次执行显示区域后就清空
                        tEnd = System.DateTime.Now;
                        tCostTime = (tEnd.Subtract(tStart)).TotalMilliseconds;

                        costTimeArray[curEntryIndex] += tCostTime;

                        if (nextEntryFamilyIndex < 0)
                        {
                            cResult = false;
                        }
                        else 
                        {
                            cResult = true;
                        }
                                               
                        ModuleTreeExt.Dispatcher.Invoke(new Action(
                        delegate
                        {
                            ModuleTreeExtNode cNode = (ModuleTreeExt.GetNodeByEntryName(cEntryName));
                            cNode.CostTime = costTimeArray[curEntryIndex].ToString("f0") + "ms";
                            cNode.IsRunning = false;
                            if (cResult)
                            {
                                cNode.StateImage = OKIcon;
                            } else 
                            {
                                cNode.StateImage = NGIcon;
                            }                                                       
                        }));

                        s_OutputListView.Dispatcher.Invoke(new Action(
                        delegate
                        {
                            if (OutputTitle == cModuleName)
                            {
                                DisplayOutputData(OutputTitle);
                            }
                        }));

                        int absNextEntryFamilyIndex = System.Math.Abs(nextEntryFamilyIndex);
                        if (absNextEntryFamilyIndex >= moduleObjectBase.Info.ModuleFamily.Count)
                        {
                            int fixedOffset = absNextEntryFamilyIndex - moduleObjectBase.Info.ModuleFamily.Count + 1;
                            curEntryIndex += fixedOffset;
                        }
                        else
                        {
                            string nextEntryName = moduleObjectBase.Info.ModuleFamily[absNextEntryFamilyIndex] + moduleObjectBase.Info.ModuleNumber.ToString();
                            curEntryIndex = m_ModuleEntryList.FindIndex(m => m.Key == nextEntryName);
                        }

                    }
                                   
                    if (RunMode == RunMode.RunOnce)
                    {
                        RunMode = RunMode.None;
                        Stop();
                    }
                }
            }
        }

        /// <summary>
        /// 有了[OnDeserialized()]标志的函数在反序列化时会被调用，与引用无关
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized()]
        internal void OnDeSerializedMethod(StreamingContext context)
        {
            m_ThreadStatus = new bool();
            m_ThreadStatus = false;
            //在序列化时，m_Thread和m_AutoResetEvent以为其类型的原因，并不能标记为序列化。故在反序列化时，要对这两个变量进行初始化
            if (m_Thread == null)
            {
                m_Thread = new Thread(Process);
                m_Thread.IsBackground = true;
                m_Thread.Start();
            }
            if (m_AutoResetEvent == null)
            {
                m_AutoResetEvent = new AutoResetEvent(false);//控制流程
            }


        }
    }
}
