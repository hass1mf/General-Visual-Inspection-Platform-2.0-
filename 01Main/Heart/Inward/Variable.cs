using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
//Variable.cs类自己独立，无需其他引用
//Variable.cs类用定义相关在流程的变量的类

namespace Heart.Inward
{
    [Serializable]
    public class VariableInfo   //定义流程变量的类
    {
        public VariableInfo(string vtype,string vname,string vvalue,string vremark)
        {
            vType = vtype;
            vName = vname;
            vValue = vvalue;
            vRemark = vremark;
        }
        public string vType { get;set;}
        public string vName { get; set; }
        public string vRemark { get; set; }
        public string vValue { get; set; }
    }

    [Serializable]
    public class GlobalVariable     //定义全局变量的类
    {
        public List<VariableInfo> VariableInfoList = new List<VariableInfo>();
        public int HwindowNumber = 1;
        public Dictionary<string, object> VariableDataDict = new Dictionary<string, object>();

        private Dictionary<string, int> IntVariableDict = new Dictionary<string, int>();
        private Dictionary<string, double> DoubleVariableDict = new Dictionary<string, double>();
        private Dictionary<string, string> StringVariableDict = new Dictionary<string, string>();
        private Dictionary<string, bool> BoolVariableDict = new Dictionary<string, bool>();

        /// <summary>
        /// 更新全局变量，传参是List<VariableInfo> vInfoList 整个List信息
        /// </summary>
        /// <param name="vInfoList"></param>
        public void UpdateVariable(List<VariableInfo> vInfoList)
        {
            VariableInfoList = CloneObject.DeepCopy(vInfoList);

            IntVariableDict.Clear();
            DoubleVariableDict.Clear();
            StringVariableDict.Clear();
            BoolVariableDict.Clear();

            VariableDataDict.Clear();

            foreach (VariableInfo vInfo in VariableInfoList)
            {
                switch (vInfo.vType)
                {
                    case "int":
                        int intValue = int.Parse(vInfo.vValue);
                        IntVariableDict.Add(vInfo.vName, intValue);
                        VariableDataDict.Add(vInfo.vName, IntVariableDict[vInfo.vName]);
                        break;
                    case "double":
                        double doubleValue = double.Parse(vInfo.vValue);
                        DoubleVariableDict.Add(vInfo.vName, doubleValue);
                        VariableDataDict.Add(vInfo.vName, DoubleVariableDict[vInfo.vName]);
                        break;
                    case "string":
                        string stringValue = vInfo.vValue;
                        StringVariableDict.Add(vInfo.vName, stringValue);
                        VariableDataDict.Add(vInfo.vName, StringVariableDict[vInfo.vName]);
                        break;
                    case "bool":
                        bool boolValue = false;
                        if (vInfo.vValue == "true")
                        {
                            boolValue = true;
                        }
                        BoolVariableDict.Add(vInfo.vName, boolValue);
                        VariableDataDict.Add(vInfo.vName, BoolVariableDict[vInfo.vName]);
                        break;
                }
            }

        }
    }

}
