using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Heart.Inward;
using Heart.Outward;

namespace Plugin.LogicCondition
{
    [Category("02@逻辑工具")]
    [DisplayName("00@条件分支#如果.结束")]
    [Serializable]
    class ModuleObj : ModuleObjBase
    {
        public ObservableCollection<ConditionVariable> m_ConditionVariableCollection = new ObservableCollection<ConditionVariable>();

        public ExpressionScript EScript = new ExpressionScript();
        public byte[] ExpressionAssembly = null;
        public string ExpressionRegionStr = "";
        public bool BranchExecuted = false;
        public int BranchCounter = -1;

        public ModuleObj()
        {
            m_ConditionVariableCollection.Add(new ConditionVariable("如果", "true", true));
        }

        public override int ExeModule(string entryName)
        {
            int index = Info.ModuleFamily.FindIndex(e => e + Info.ModuleNumber.ToString() == entryName);

            if (index == Info.ModuleFamily.Count - 1)
            {
                return Info.ModuleFamily.Count;
            }
            else
            {
                if (index < 1)
                {
                    BranchExecuted = false;

                    EScript.SetProjectID(Info.ProjectID);

                    if (ExpressionAssembly == null)
                    {
                        if (UpdateExpressionAssembly() == false)
                        {
                            return -(Info.ModuleFamily.Count - 1);
                        }                  
                    }

                    List<Object> vObjectList = EScript.Run(ExpressionAssembly);
                    for (int i = 0; i < m_ConditionVariableCollection.Count; ++i)
                    {
                        m_ConditionVariableCollection[i].Value = (bool)vObjectList[i];
                    }
                }

                if (index < 0)
                {
                    return 1;
                }

                if (BranchExecuted == true)
                {
                    return Info.ModuleFamily.Count - 1;
                }
                else
                {
                    if (m_ConditionVariableCollection[index].Value == true)
                    {
                        BranchExecuted = true;
                        return Info.ModuleFamily.Count;
                    }
                    else
                    {
                        return index + 1;
                    }
                }

            }

        }

        public bool UpdateExpressionAssembly()
        {
            string expressionRegionStr = "";
            for (int i = 0; i < m_ConditionVariableCollection.Count; ++i)
            {
                expressionRegionStr += EScript.GenerateExpressionLine("bool", m_ConditionVariableCollection[i].vExpression) + "\r\n";
            }

            if (expressionRegionStr != ExpressionRegionStr)
            {
                ExpressionRegionStr = expressionRegionStr;
                EScript.SetExpressionRegion(ExpressionRegionStr);
                ExpressionAssembly = EScript.Compile();
            }

            if (ExpressionAssembly != null)
            {
                return true;
            }

            return false;
        }

        public override void UpdateOutput()
        {
            foreach (ConditionVariable cVar in m_ConditionVariableCollection)
            {
                string cVarName = cVar.vBranch.Replace("#", "").Replace("否则", "Else").Replace("如果", "If") + "Value";
                AddOutputVariable(cVarName, "bool", cVar.Value, cVar.vValue, cVar.vBranch + "真值");
            }
        }

        public override int AddEntry()
        {
            BranchCounter++;
            int index = Info.ModuleFamily.Count - 1;
            string branchName = "否则" + BranchCounter.ToString() + "#";
            Info.ModuleFamily.Insert(index, branchName);
            m_ConditionVariableCollection.Add( new ConditionVariable(branchName, "true", true));
            UpdateExpressionAssembly();
            return index;
        }

    }

    [Serializable]
    public class ConditionVariable
    {
        public ConditionVariable(string vbranch, string vexpression, bool value)
        {
            vBranch = vbranch;
            vExpression = vexpression;
            Value = value;
        }
        public string vBranch { get; set; }
        public string vExpression { get; set; }
        public string vValue { get; set; }

        private bool _Value;
        public bool Value{
            get { return _Value; }
            set {
                _Value = value;
                vValue = _Value.ToString().ToLower();
            }
        }
    }
}
