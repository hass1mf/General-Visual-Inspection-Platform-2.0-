using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Heart.Inward;
using Heart.Outward;
using System.CodeDom.Compiler;


namespace Plugin.VariableDefine
{
    [Category("03@变量工具")]
    [DisplayName("00@变量定义#变量定义")]
    [Serializable]
    class ModuleObj : ModuleObjBase
    {
        public Dictionary<string, int> IntVariableDict = new Dictionary<string, int>();
        public Dictionary<string, double> DoubleVariableDict = new Dictionary<string, double>();
        public Dictionary<string, string> StringVariableDict = new Dictionary<string, string>();
        public Dictionary<string, bool> BoolVariableDict = new Dictionary<string, bool>();

        public byte[] ExpressionAssembly = null;
        public string ExpressionRegionStr = "";

        public ObservableCollection<VariableInfoEx> VariableInfoExCollection = new ObservableCollection<VariableInfoEx>();

        public ExpressionScript EScript = new ExpressionScript();
        public override int ExeModule(string entryName)
        {
            EScript.SetProjectID(Info.ProjectID);

            if (ExpressionAssembly == null)
            {
                return -1;
            }

            List<Object> vObjectList = EScript.Run(ExpressionAssembly);

            for (int i = 0; i < VariableInfoExCollection.Count; ++i)
            {
                VariableInfoEx vInfoEx = VariableInfoExCollection[i];
                switch (VariableInfoExCollection[i].vType)
                {
                    case "int":
                        IntVariableDict[vInfoEx.vName] = (int)vObjectList[i];
                        vInfoEx.vValue = IntVariableDict[vInfoEx.vName].ToString();
                        break;
                    case "double":
                        DoubleVariableDict[vInfoEx.vName] = (double)vObjectList[i];
                        vInfoEx.vValue = DoubleVariableDict[vInfoEx.vName].ToString();
                        break;
                    case "string":
                        StringVariableDict[vInfoEx.vName] = (string)vObjectList[i];
                        vInfoEx.vValue = StringVariableDict[vInfoEx.vName];
                        break;
                    case "bool":
                        BoolVariableDict[vInfoEx.vName] = (bool)vObjectList[i];
                        vInfoEx.vValue = BoolVariableDict[vInfoEx.vName].ToString().ToLower();
                        break;
                }
            }

            return 1;
        }

        public override void UpdateOutput()
        {
            foreach (VariableInfoEx vInfoEx in VariableInfoExCollection)
            {
                switch (vInfoEx.vType)
                {
                    case "int":
                        AddOutputVariable(vInfoEx.vName, vInfoEx.vType, IntVariableDict[vInfoEx.vName], vInfoEx.vValue, vInfoEx.vRemark);
                        break;
                    case "double":
                        AddOutputVariable(vInfoEx.vName, vInfoEx.vType, DoubleVariableDict[vInfoEx.vName], vInfoEx.vValue, vInfoEx.vRemark);
                        break;
                    case "string":
                        AddOutputVariable(vInfoEx.vName, vInfoEx.vType, StringVariableDict[vInfoEx.vName], vInfoEx.vValue, vInfoEx.vRemark);
                        break;
                    case "bool":
                        AddOutputVariable(vInfoEx.vName, vInfoEx.vType, BoolVariableDict[vInfoEx.vName], vInfoEx.vValue, vInfoEx.vRemark);
                        break;
                }
            }
        }

        public void UpdateExpressionAssembly()
        {
            string expressionRegionStr = "";
            for (int i = 0; i < VariableInfoExCollection.Count; ++i)
            {
                expressionRegionStr += EScript.GenerateExpressionLine(VariableInfoExCollection[i].vType, VariableInfoExCollection[i].vExpression) + "\r\n";
            }

            if (expressionRegionStr != ExpressionRegionStr)
            {
                ExpressionRegionStr = expressionRegionStr;
                EScript.SetExpressionRegion(ExpressionRegionStr);
                ExpressionAssembly = EScript.Compile();
            }

        }
    }

    [Serializable]
    public class VariableInfoEx
    {
        public VariableInfoEx(string vtype, string vname, string vexpression, string vvalue, string vremark)
        {
            vType = vtype;
            vName = vname;
            vExpression = vexpression;
            vValue = vvalue;
            vRemark = vremark;
        }
        public string vType { get; set; }
        public string vName { get; set; }
        public string vExpression { get; set; }
        public string vValue { get; set; }
        public string vRemark { get; set; }

    }
}
