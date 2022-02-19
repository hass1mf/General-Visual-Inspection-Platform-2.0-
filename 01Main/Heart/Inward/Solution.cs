using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Heart.Outward;
using MyOS.Common.communacation;
using AcqDevice;

namespace Heart.Inward
{
    public delegate void UpdataIteamInfoSolution(string info);
    public class Solution
    {
        //单例模式
        private static Solution s_Instance = null;
        //Halcon引擎
     //   private static HDevEngine s_HDevEngine = new HDevEngine();
        //流程列表
        public static List<Project> m_ProjectList = new List<Project>();
        //全局变量
        private static GlobalVariable m_GlobalVariable = new GlobalVariable();
        //默认工程名字
        public static string sConfigPath = @"MeasureSys.os";
        //用于保存ModuleTreeExtSave信息的字典
        public static Dictionary<int, ModuleTreeExtSave> DicModuleTreeExtSave = new Dictionary<int, ModuleTreeExtSave>();
        ////用于保存ModuleTreeExtSave信息字典的List
        //private static List<Dictionary<int, ModuleTreeExtSave>>  list_DicModuleTreeExtSave = new List<Dictionary<int, ModuleTreeExtSave>>();

        //注册委托用于返回信息     
        public event UpdataIteamInfoSolution UpdataIteam;

        /// <summary>
        /// 最终的采集设备列表
        /// </summary>
        public static List<AcqAreaDeviceBase> g_AcqDeviceList = new List<AcqAreaDeviceBase>();

        private Solution() { }

        public static Solution Instance //Instance实例    //1023因为解决方案只有一个，所以直接给Solution一个实例的唯一对象即可
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new Solution();
                }
                return s_Instance;
            }
        }

        /// <summary>
        /// 更新条款
        /// </summary>
        /// <param name="up"></param>
        private void UpData(string  up)
        {
            if (UpdataIteam !=null)
            {
                if (up != null)
                {
                    UpdataIteam(up);
                }
                else
                {
                    UpdataIteam("更新失败");
                }
            }
     
        }
        /// <summary>
        /// 解决方案初始化
        /// </summary>
        public static void Init()
        {
           // s_HDevEngine.SetProcedurePath(Environment.GetEnvironmentVariable("TEMP"));  //1023这里的初始化针对的是halcon引擎
            //增加预编译,在脚本里有大量的循环的时候 速度会提示,否则没什么效果
          //  s_HDevEngine.SetEngineAttribute("execute_procedures_jit_compiled", "true");
        }

        /// <summary>
        /// 创建流程
        /// </summary>
        public int CreateProject()
        {
            Project project = new Project();

            //获取新不重复的id  如果已经存在  1,2,4   那么久获得的id 是 3
            bool flag = false;
            int id = 0;
            do
            {
                flag = true;

                foreach (Project prj in m_ProjectList)
                {
                    if (prj.ProjectInfo.ProjectID == id)
                    {
                        id++;
                        flag = false;
                        break;
                    }
                }

                if (flag == true)
                {
                    break;
                }
            } while (true);

            project.ProjectInfo.ProjectID = id;
            project.UpdateVariableInfo("全局变量", m_GlobalVariable.VariableInfoList);
            project.UpdateVariableData("全局变量", m_GlobalVariable.VariableDataDict);
            project.UpdataIteam += UpData;
            m_ProjectList.Add(project);
            //20211025每添加一个流程，就对应生产一个字典来记录ModuleTreeExtSave
            //Dictionary<int, ModuleTreeExtSave> DicModuleTreeExtSave = new Dictionary<int, ModuleTreeExtSave>();
            DicModuleTreeExtSave.Add(id, new ModuleTreeExtSave());  //添加key是当前的流程id，value是new出来的一份ModuleTreeExtSave
            UpData("新建流程"+ id);
            return id;
        }

        /// <summary>
        /// 存储窗体个数
        /// </summary>
        /// <param name="number"></param>
        public void SetHWindowNumber(int number)
        {

            m_GlobalVariable.HwindowNumber = number;
        }

        /// <summary>
        /// 返回窗体个
        /// </summary>
        /// <returns></returns>
        public int GetHWindowNumber()
        {
          return  m_GlobalVariable.HwindowNumber;
        }



        /// <summary>
        /// 删除流程
        /// </summary>
        /// <param name="projectID"></param>
        public void DeleteProject(int projectID)
        {
            m_ProjectList.Remove(GetProjectById(projectID));
            DicModuleTreeExtSave.Remove(projectID);     //直接根据projectID这个id key去删除字典内对于的值
            UpData("流程"+ projectID+"已删除");
        }

        /// <summary>
        /// 清空项目内容
        /// </summary>
        /// <param name="projectID"></param>
        public void DeleteSolution()
        {
            m_ProjectList.Clear();
            DicModuleTreeExtSave.Clear();     //直接根据projectID这个id key去删除字典内对于的值
        }

        /// <summary>
        /// 执行一次
        /// </summary>
        /// <param name="projectID"></param>
        public void ExecuteOnce(int projectID)
        {
            m_ProjectList.FirstOrDefault(c => c.ProjectInfo.ProjectID == projectID).RunMode = RunMode.RunOnce;//循环
            m_ProjectList.FirstOrDefault(c => c.ProjectInfo.ProjectID == projectID).Start();
        }

        /// <summary>
        /// 连续运行
        /// </summary>
        /// <param name="projectID"></param>
        public void StartRun(int projectID)
        {
            m_ProjectList.FirstOrDefault(c => c.ProjectInfo.ProjectID == projectID).RunMode = RunMode.RunCycle;//循环
            m_ProjectList.FirstOrDefault(c => c.ProjectInfo.ProjectID == projectID).Start();
        }

        /// <summary>
        /// 停止运行
        /// </summary>
        /// <param name="projectID"></param>
        public void StopRun(int projectID)
        {
            m_ProjectList.FirstOrDefault(c => c.ProjectInfo.ProjectID == projectID).RunMode = RunMode.None;//停止
            m_ProjectList.FirstOrDefault(c => c.ProjectInfo.ProjectID == projectID).Stop();
        }

        /// <summary>
        /// 根据id获取对应的project
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Project GetProjectById(int id)
        {
            return m_ProjectList.FirstOrDefault(c => c.ProjectInfo.ProjectID == id);
        }

        /// <summary>
        /// 获取所有项目运行状态
        /// </summary>
        /// <returns></returns>
        public bool GetStates()
        {
            foreach (Project prj in m_ProjectList)
            {
                if (prj.GetThreadStatus() == true) return true;
            }
            return false;
        }

        /// <summary>
        /// 根据流程id获取流程运行状态
        /// </summary>
        /// <returns></returns>
        public bool GetStatesById(int id)
        {
            Project prj = m_ProjectList.FirstOrDefault(c => c.ProjectInfo.ProjectID == id);
            if(prj != null)
            {
                if (prj.GetThreadStatus() == true) return true;
                else return false;
            }
            return false;   //每一个函数只有一个返回值，所以上面的进入到了，这里就不会也放回false了
        }

        /// <summary>
        /// 获取全局变量
        /// </summary>
        /// <returns></returns>
        public List<VariableInfo> GetGlobalVariableInfoList()
        {
            return m_GlobalVariable.VariableInfoList;
        }



        
        /// <summary>
        /// 传入更新全局变量
        /// </summary>
        /// <param name="vInfoList"></param>
        public void UpdateGlobalVariable(List<VariableInfo> vInfoList)
        {
            m_GlobalVariable.UpdateVariable(vInfoList);

            foreach (Project prj in m_ProjectList)
            {
                prj.UpdateVariableInfo("全局变量", m_GlobalVariable.VariableInfoList);
                prj.UpdateVariableData("全局变量", m_GlobalVariable.VariableDataDict);
            }
        }

        #region 20211023项目序列化相关

        /// <summary>
        /// 保存工程项目文件辅助函数
        /// </summary>
        /// <param name="filePath"></param>
        public void SaveConfig(string filePath)//保存工程，而且是先保存一个tempMeasureSys.cfg然后在根据是保存还是另存为来复制到新的文件去
        {
            SaveModuleTreeExtBefore();  //先保存ModuleTreeExtSave

            //添加相对路径判断 如果是相对路径 必须指定到exe目录 否则可能因为打开文件等对话框影响存储路径  yoga 2018-8-30 11:06:48
            string ThePath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            string tempFile = "tempMeasureSys.os";    //tempFile临时文件
            tempFile = System.IO.Path.Combine(ThePath, tempFile);   //将两个字符串组合成一个路径
            try
            {
                System.GC.Collect();    //强制对所有代进行即时垃圾回收
                using (FileStream fs = new FileStream(tempFile, FileMode.Create))
                {
                    BinaryFormatter binaryFmt = new BinaryFormatter();
                    fs.Seek(0, SeekOrigin.Begin);
                    binaryFmt.Serialize(fs, m_GlobalVariable);    //系统变量列表和常量
                    binaryFmt.Serialize(fs, g_AcqDeviceList);   //采集设备列表
                    //binaryFmt.Serialize(fs, AcqDeviceBase.m_LastDeviceID);  //用于记录采集设备的数量
                    //binaryFmt.Serialize(fs, EComManageer.s_ECommunacationDic);       //新加的通讯设置
                    binaryFmt.Serialize(fs, m_ProjectList);     //工程列表
                    //binaryFmt.Serialize(fs, CProject.m_LastProjectID);      //用于记录工程相关的数量
                    //binaryFmt.Serialize(fs, CProject.m_IsMangerGTCard);     //可能作为末尾结束符
                    //fs.Close();
                    binaryFmt.Serialize(fs, DicModuleTreeExtSave);      //ModuleTreeExtSave流程栏树
                    binaryFmt.Serialize(fs, EComManageer.s_ECommunacationDic);       //新加的通讯设置
                }
                string outPath = filePath;
                if (filePath.Contains(@":\") == false)
                {
                    outPath = System.IO.Path.Combine(ThePath, filePath);
                }
                FileHelper.FileCoppy(tempFile, outPath);    //OrignFile:原始文件,NewFile:新文件路径
                MessageBox.Show("保存成功！");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("保存配置文件失败" + ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //LogHandler.Instance.VTLogError(ex.ToString());
            }
        }

        /// <summary>
        /// 打开工程项目辅助函数
        /// </summary>
        /// <param name="filePath"></param>
        public void ReadConfig(string filePath)
        {
            try
            {// Deserialize.反序列化
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    BinaryFormatter binaryFmt = new BinaryFormatter();
                    m_GlobalVariable = (GlobalVariable)binaryFmt.Deserialize(fs);
                    g_AcqDeviceList = (List<AcqAreaDeviceBase>)binaryFmt.Deserialize(fs);
                    //AcqDeviceBase.m_LastDeviceID = (int)binaryFmt.Deserialize(fs);
                    //EComManageer.s_ECommunacationDic = (Dictionary<string, ECommunacation>)binaryFmt.Deserialize(fs);
                    m_ProjectList = (List<Project>)binaryFmt.Deserialize(fs);
                    //CProject.m_LastProjectID = (int)binaryFmt.Deserialize(fs);
                    //CProject.m_IsMangerGTCard = (bool)binaryFmt.Deserialize(fs);
                    //fs.Close();
                    //GT_MotionControl.Helper.InitMotionControl();
                    DicModuleTreeExtSave = (Dictionary<int, ModuleTreeExtSave>)binaryFmt.Deserialize(fs);
                    EComManageer.s_ECommunacationDic = (Dictionary<string, ECommunacation>)binaryFmt.Deserialize(fs);
                    MessageBox.Show("加载成功");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Measure.HMeasureSYS.ReadConfig:" + ex.ToString());
                //LogHandler.Instance.VTLogError(ex.ToString());
            }
        }

        /// <summary>
        ///  初始化视觉工程项目
        /// </summary>
        /// <param name="filepath">初始化文件所在路径</param>
        /// <returns></returns>
        public bool InitialVisionProgram(string filepath = @"MeasureSys.os")
        {
            try
            {
                string ThePath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                if (filepath.Contains(@":\") == false)
                {
                    filepath = System.IO.Path.Combine(ThePath, filepath);
                }

                if (filepath.Trim() == "" || System.IO.File.Exists(filepath) == false)
                {
                    System.Windows.Forms.MessageBox.Show("输入文件名错误！");
                    throw new Exception("视觉测量模块报错：" + filepath + "不存在！");
                }
                else
                {
                    sConfigPath = filepath;
                }

                //设备也要同步先关闭  yoga 2018-8-30 15:34:29
                ///HMeasureSYS.DisposeDev();
                //由于可能之前已经打开了  此处要先将tcp服务器关闭
                ///HMeasureSYS.g_TcpServer.tcp.Stop();

                ReadConfig(sConfigPath);    //经过ReadConfig函数后二进制文件已经反序列化为类
                //HMeasureSYS.ReadSpaceInfo(out HMeasureSYS.g_ProjectList, out HMeasureSYS.g_VariableList, out HMeasureSYS.g_AcqDeviceList, out HMeasureSYS.g_TcpServer, Application.StartupPath + sProject_Path);

                ///HMeasureSYS.g_TcpServer.tcp.Start();
                //没次读取新的配置文件时重新检测VB代码状态和设备连接状态
                ////HMeasureSYS.InitDevStatus();    //打开初始化采集设备可能很有必要
                ////HMeasureSYS.InitCommunication();    //初始化通讯设备 magic20210821
                InitModuleTreeExt();
                InitDevStatus();    //打开初始化采集设备可能很有必要
                InitCommunication();    //初始化通讯设备 magic20210821           
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Measure.HMeasureSYS.InitialVisionProgram:" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 序列化项目之前，先保存ModuleTreeExt信息
        /// </summary>
        private void SaveModuleTreeExtBefore()
        {
            if(m_ProjectList.Count != 0)
            {
                foreach (Project item in m_ProjectList)
                {
                    DicModuleTreeExtSave[item.ProjectInfo.ProjectID].ExpandStatusDict = item.ModuleTreeExt.ExpandStatusDict;
                    DicModuleTreeExtSave[item.ProjectInfo.ProjectID].PluginNumberDict = item.ModuleTreeExt.PluginNumberDict;
                    DicModuleTreeExtSave[item.ProjectInfo.ProjectID].EntryList = item.ModuleTreeExt.EntryList;
                    DicModuleTreeExtSave[item.ProjectInfo.ProjectID].NodeList = item.ModuleTreeExt.NodeList;
                    DicModuleTreeExtSave[item.ProjectInfo.ProjectID].TreeSourceList = item.ModuleTreeExt.TreeSourceList;
                    DicModuleTreeExtSave[item.ProjectInfo.ProjectID].ProjectID = item.ModuleTreeExt.ProjectID;
                }
            }
        }

        /// <summary>
        /// 初始化流程栏树的初始化还原
        /// </summary>
        private void InitModuleTreeExt()
        {
            if (m_ProjectList.Count != 0)
            {
                foreach (Project item in m_ProjectList)
                {
                    item.ModuleTreeExt = new ModuleTreeExt();   //ModuleTreeExt没有被序列化标记，所以在这里先new一份，然后添加信息进去
                    item.ModuleTreeExt.ExpandStatusDict = DicModuleTreeExtSave[item.ProjectInfo.ProjectID].ExpandStatusDict;
                    item.ModuleTreeExt.PluginNumberDict = DicModuleTreeExtSave[item.ProjectInfo.ProjectID].PluginNumberDict;
                    item.ModuleTreeExt.EntryList = DicModuleTreeExtSave[item.ProjectInfo.ProjectID].EntryList;
                    item.ModuleTreeExt.NodeList = DicModuleTreeExtSave[item.ProjectInfo.ProjectID].NodeList;
                    item.ModuleTreeExt.TreeSourceList = DicModuleTreeExtSave[item.ProjectInfo.ProjectID].TreeSourceList;
                    item.ModuleTreeExt.ProjectID = DicModuleTreeExtSave[item.ProjectInfo.ProjectID].ProjectID;
                    item.ModuleTreeExt.UpdateTree();    //希望赋值后了可以强制刷新页面
                }
            }
        }

        /// <summary>
        /// 初始化通讯设备
        /// </summary>
        public static void InitCommunication()
        {
            List<string> listName = new List<string>();
            List<ECommunacation> Ecom = EComManageer.GetEcomList();
            foreach (ECommunacation _ecom in Ecom)
            {
                _ecom.IsConnected = false;  //要先把连接信号置为false,不然系统不会去尝试连接
                _ecom.Connect();    //先全部尝试连接一遍
            }
        }

        /// <summary>
        /// 初始化采集设备--尝试连接相机
        /// </summary>
        public static void InitDevStatus()
        {
            foreach (AcqAreaDeviceBase dev in g_AcqDeviceList)
            {
                if (dev.m_bConnected)
                {
                    dev.m_bConnected = false;
                    dev.ConnectDev();   //文件夹采集图像也要先连接一下
                    dev.setSetting();
                }
                else
                {
                    dev.DisConnectDev();
                }
            }
        }

        /// <summary>
        /// 初始化采集设备--尝试连接相机
        /// </summary>
        public static void CloseAllDev()
        {
            foreach (AcqAreaDeviceBase dev in g_AcqDeviceList)
            {
                try
                {
                    dev.ConnectDev();   //文件夹采集图像也要先连接一下
                    dev.m_bConnected = false;
                    dev.setSetting();
                }
                catch {  } 
            }
        }

        #endregion
    }
}
