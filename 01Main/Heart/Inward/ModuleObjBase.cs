using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Heart.Outward;
//ModuleObjBase类用于数据操作部分，插件的ModuleObj要集成于ModuleObjBase

namespace Heart.Inward
{
    [Serializable]
    public class ModuleInfo
    {
        public string ModuleName { get; set; } //ModuleName = PluginName + ModuleNumber       
        public string PluginName { get; set; }  //PluginName是插件的纯名字
        public int ModuleNumber { get; set; }   //ModuleNumber记录着当前插件的数量序号，因为模块会一直添加，模块信息会变
        public List<string> ModuleFamily { get; set; }
        public int ProjectID { get; set; }
        public bool IsUse { get; set; } = true;          
    }

    [Serializable]
    public class ModuleEntry    //用于记录模块拖动到流程栏时的基本模块信息
    {
        public string EntryName { get; set; } //EntryName = RawEntryName + ModuleNumber
        public string RawEntryName { get; set; }
        public bool Deletable { get; set; } = false;
        public ModuleInfo Info { get; set; }
    }

    /// <summary>
    /// 抽象类
    /// </summary>
    [Serializable]
    public abstract class ModuleObjBase
    {
        /// <summary>
        /// 模块参数
        /// </summary>
        public ModuleInfo Info { get; set; } = new ModuleInfo();

        private List<VariableInfo> OutputVarInfoList = new List<VariableInfo>();

        private Dictionary<string, object> OutputVarDataDict = new Dictionary<string, object>();
        /// <summary>
        /// 
        /// </summary>
        public int CanvasIndex { get; set; } = -1;   //画布

        /// <summary>
        /// 执行模块
        /// </summary>
        /// <returns></returns>
        public abstract int ExeModule(string entryName);    //执行模块函数  包含公共的属性和行为，被子类所共享–代码重用。抽象类是需要被子类继承的，包括传入的参数也必须一致

        /// <summary>
        /// 更新模块输出
        /// </summary>
        public abstract void UpdateOutput();    //更新数据函数  包含公共的属性和行为，被子类所共享–代码重用。抽象类是需要被子类继承的

        //abstract public void UpdateDisplay(); //去掉该抽象类，对应的子类也要同步修改
        
        /// <summary>
        /// 执行当前模块
        /// </summary>
        /// <param name="entryName"></param>
        /// <returns></returns>
        public int ExecuteModule(string entryName)      //运行流程时执行模块函数  Execute执行 
        {          
            int result = ExeModule(entryName);  //执行函数，拿到AcquiredImage

            UpdateOutputVariable();     //与更新输出变量相关

            UpdateDisplay();

            return result;
        }

        /// <summary>
        /// 重置插件状态
        /// </summary>
        public virtual void ResetState()
        {

        }

        /// <summary>
        /// AddEntry?
        /// </summary>
        /// <returns></returns>
        public virtual int AddEntry()
        {
            return -1;
        }

        /// <summary>
        /// DeleteEntry?
        /// </summary>
        /// <param name="index"></param>
        public virtual void DeleteEntry(int index)
        {

        }

        /// <summary>
        /// 插件对系统来输出变量信息，这个函数接口是针对给插件内部使用输出的
        /// </summary>
        /// <param name="vName"></param>
        /// <param name="vType"></param>
        /// <param name="vObject"></param>
        /// <param name="vValue"></param>
        /// <param name="vRemark"></param>
        /// <returns></returns>
        public bool AddOutputVariable(string vName,string vType, object vObject,string vValue,string vRemark)   //该函数作为图像变量写入系统的唯一函数，在插件内被多次调用
        {
            if ((OutputVarInfoList.FindIndex(c => c.vName == vName)<0) && (!OutputVarDataDict.ContainsKey(vName)))
            {
                OutputVarInfoList.Add(new VariableInfo(vType, vName, vValue, vRemark));//图像变量传入的第一组的四个数据
                OutputVarDataDict.Add(vName, vObject);//图像变量传入的第二组的两个数据
                return true;
            }          
            return false;
        }

        /// <summary>
        /// 更新输出变量
        /// </summary>
        public void UpdateOutputVariable()//被ModuleFormBase内的确定按钮函数调用，用于写入变量
        {
            OutputVarInfoList.Clear();
            OutputVarDataDict.Clear();

            UpdateOutput();     //调用的是插件内的ModuleObj类的UpdateOutput，用于插件模块信息的输出

            Project prj = Solution.Instance.GetProjectById(Info.ProjectID);     //ProjectID是流程id
            prj.UpdateVariableInfo(Info.ModuleName, OutputVarInfoList);         //更新变量信息，函数在projec类定义
            prj.UpdateVariableData(Info.ModuleName, OutputVarDataDict);
        }

        /// <summary>
        /// 通过模块名和变量类型来获取变量信息
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="vType"></param>
        /// <returns></returns>
        public List<VariableInfo> GetVariableInfoByModuleWithType(string moduleName, string vType)
        {
            Project prj = Solution.Instance.GetProjectById(Info.ProjectID);
            return prj.GetVariableInfoByModuleWithType(moduleName, vType);
        }

        /// <summary>
        /// 通过变量名称来获取变量
        /// </summary>
        /// <param name="linkName"></param>
        /// <returns></returns>
        public object GetVariableDataByVarName(string linkName)
        {
            string[] strArray = linkName.Split('.');
            if (strArray.Length != 2)
            {
                return null;
            }
            Project prj = Solution.Instance.GetProjectById(Info.ProjectID);
            return prj.GetVariableDataByVarName(strArray[0], strArray[1]);
        }

        /// <summary>
        /// 添加显示图像信息,index为要显示的画布
        /// </summary>
        /// <param name="index"></param>
        /// <param name="obj"></param>
        public void AddDisplayImage(int index, object obj)
        {
            Project prj = Solution.Instance.GetProjectById(Info.ProjectID);     //由ID来获取得到项目名
            prj.AddDisplayImage(index, obj);                                    //index默认还是选择0，该函数用于绑定画布序列和对于显示的图像
        }

        /// <summary>
        /// 添加显示轮廓信息,index为要显示的画布
        /// </summary>
        /// <param name="index"></param>
        /// <param name="obj"></param>
        public void AddDisplayRois(int index, List<MeasureROI> obj)
        {
            Project prj = Solution.Instance.GetProjectById(Info.ProjectID);     //由ID来获取得到项目名
            prj.AddDisplayRoi(index, obj);                                    //index默认还是选择0，该函数用于绑定画布序列和对于显示的图像
        }

        /// <summary>
        /// 虚接口，更新显示
        /// </summary>
        public virtual void UpdateDisplay()
        {

        }

        /// <summary>
        /// 20211028通过图像变量的来源获取对应的显示画布
        /// </summary>
        /// <param name="linkName"></param>
        /// <returns></returns>
        public int ConfirmCanvasByImageVariable(string linkName)
        {
            if(linkName != "")
            {
                string[] strArray = linkName.Split('.');
                Project prj = Solution.Instance.GetProjectById(Info.ProjectID);
                ModuleObjBase obj = prj.GetModuleByName(strArray[0]);
                if (obj.CanvasIndex != -1)
                {
                    return obj.CanvasIndex;
                }
                return -1;
            }
            return -1;
        }
    }
}