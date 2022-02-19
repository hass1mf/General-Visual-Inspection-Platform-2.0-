using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Heart.Inward;
using Heart.Outward;

namespace Plugin.LogicCirculation
{
    [Category("02@逻辑工具")]
    [DisplayName("01@循环工具#开始循环.结束循环")]
    [Serializable]
    class ModuleObj : ModuleObjBase
    {
        public ExpressionScript EScript = new ExpressionScript();
        public byte[] JudgeExpressionAssembly = null;
        public string JudgeExpressionRegionStr = "";
        public byte[] TraversalExpressionAssembly = null;
        public string TraversalExpressionRegionStr = "";
        public bool IsTraversalChecked = true;
        public Dictionary<string,string> ExpressionDict = new Dictionary<string, string> { { "truth", "false" }, { "initial", "0" }, { "final", "0" } };

        public bool TruthValue = false;
        public int InitialValue = 0;
        public int FinalValue = 0;
        public int CurrentValue = 0;
        public int TraversalStep = 0;
        public int CycleCount = 0;

        public string TruthValueStr = "false";
        public string InitialValueStr = "0";
        public string FinalValueStr = "0";
        public string CurrentValueStr = "0";
        public string TraversalStepStr = "0";
        public string CycleCountStr = "0";

        public bool BreakCirculation = false;
        public bool WaitCirculation = false;

        public override int ExeModule(string entryName)
        {
            int index = Info.ModuleFamily.FindIndex(e => e + Info.ModuleNumber.ToString() == entryName);

            if (index == Info.ModuleFamily.Count - 1)
            {
                if (BreakCirculation == true)
                {
                    WaitCirculation = false;
                    return Info.ModuleFamily.Count;
                }
                else
                {
                    WaitCirculation = true;
                    return 0;
                }          
            }
            else
            {
                if (WaitCirculation == false)
                {
                    ResetState();
                }
                else
                {
                    WaitCirculation = false;
                }

                if ((entryName == "")||(CycleCount == 0))
                {
                    EScript.SetProjectID(Info.ProjectID);

                    if (IsTraversalChecked == true)
                    {
                        if (TraversalExpressionAssembly == null)
                        {
                            if (TraversalUpdateExpressionAssembly() == false)
                            {
                                return -(Info.ModuleFamily.Count - 1);
                            }
                        }

                        List<Object> vObjectList = EScript.Run(TraversalExpressionAssembly);
                        InitialValue = (int)vObjectList[0];
                        FinalValue = (int)vObjectList[1];

                        if (InitialValue == FinalValue)
                        {
                            CurrentValue = 0;
                            TraversalStep = 0;
                            BreakCirculation = true;
                            return Info.ModuleFamily.Count - 1;
                        }
                        else if (InitialValue < FinalValue)
                        {
                            CurrentValue = InitialValue;
                            TraversalStep = 1;
                        }
                        else
                        {
                            CurrentValue = InitialValue;
                            TraversalStep = -1;
                        }

                        InitialValueStr = InitialValue.ToString();
                        FinalValueStr = FinalValue.ToString();
                        CurrentValueStr = CurrentValue.ToString();
                        TraversalStepStr = TraversalStep.ToString();
                    }
                    else
                    {
                        if (JudgeExpressionAssembly == null)
                        {
                            if (JudgeUpdateExpressionAssembly() == false)
                            {
                                BreakCirculation = true;
                                return -(Info.ModuleFamily.Count - 1);
                            }
                        }

                        List<Object> vObjectList = EScript.Run(JudgeExpressionAssembly);
                        TruthValue = (bool)vObjectList[0];
                        TruthValueStr = TruthValue.ToString().ToLower();
                    }
                                 
                }

                if (entryName == "")
                {
                    return 1;
                }
               
                if (IsTraversalChecked == true)
                {
                    if (TraversalStep > 0)
                    {
                        if (CurrentValue < FinalValue)
                        {
                            CurrentValue += TraversalStep;
                            CurrentValueStr = CurrentValue.ToString();
                            CycleCount += 1;
                            CycleCountStr = CycleCount.ToString();
                            return Info.ModuleFamily.Count;
                        }
                        else
                        {
                            BreakCirculation = true;
                            return Info.ModuleFamily.Count - 1;
                        }
                    }
                    else if (TraversalStep < 0)
                    {
                        if (CurrentValue > FinalValue)
                        {
                            CurrentValue -= TraversalStep;
                            CurrentValueStr = CurrentValue.ToString();
                            CycleCount += 1;
                            CycleCountStr = CycleCount.ToString();
                            return Info.ModuleFamily.Count;
                        }
                        else
                        {
                            BreakCirculation = true;
                            return Info.ModuleFamily.Count - 1;
                        }
                    }
                }
                else
                {
                    if (CycleCount > 0)
                    {
                        if (JudgeExpressionAssembly == null)
                        {
                            if (JudgeUpdateExpressionAssembly() == false)
                            {
                                BreakCirculation = true;
                                return -(Info.ModuleFamily.Count - 1);
                            }
                        }

                        List<Object> vObjectList = EScript.Run(JudgeExpressionAssembly);
                        TruthValue = (bool)vObjectList[0];
                        TruthValueStr = TruthValue.ToString().ToLower();
                    }

                    if (TruthValue == true)
                    {
                        CycleCount += 1;
                        CycleCountStr = CycleCount.ToString();
                        return Info.ModuleFamily.Count;
                    }
                    else
                    {
                        BreakCirculation = true;
                        return Info.ModuleFamily.Count-1;
                    }
                }

            }
                      
            return -1;
        }

        public override void ResetState()
        {
            CycleCount = 0;
            BreakCirculation = false;
        }

        public override void UpdateOutput()
        {
            AddOutputVariable("CycleCount", "int", CycleCount, CycleCountStr, "循环计数");
            if (IsTraversalChecked == true)
            {
                AddOutputVariable("InitialValue", "int", InitialValue, InitialValueStr, "遍历初值");
                AddOutputVariable("FinalValue", "int", FinalValue, FinalValueStr, "遍历终值");
                AddOutputVariable("TraversalStep", "int", TraversalStep, TraversalStepStr, "遍历步长");
                AddOutputVariable("CurrentValue", "int", CurrentValue, CurrentValueStr, "遍历当前值");
            }
            else
            {
                AddOutputVariable("TruthValue", "bool", TruthValue, TruthValueStr, "判断真值");
            }                                           
        }

        public bool TraversalUpdateExpressionAssembly()
        {
            string expressionRegionStr = "";
            expressionRegionStr += EScript.GenerateExpressionLine("int", ExpressionDict["initial"]) + "\r\n";
            expressionRegionStr += EScript.GenerateExpressionLine("int", ExpressionDict["final"]) + "\r\n";

            if (expressionRegionStr != TraversalExpressionRegionStr)
            {
                TraversalExpressionRegionStr = expressionRegionStr;
                EScript.SetExpressionRegion(TraversalExpressionRegionStr);
                TraversalExpressionAssembly = EScript.Compile();
            }

            if (TraversalExpressionAssembly != null)
            {
                return true;
            }

            return false;
        }

        public bool JudgeUpdateExpressionAssembly()
        {
            string expressionRegionStr = "";
            expressionRegionStr += EScript.GenerateExpressionLine("bool", ExpressionDict["truth"]) + "\r\n";

            if (expressionRegionStr != JudgeExpressionRegionStr)
            {
                JudgeExpressionRegionStr = expressionRegionStr;
                EScript.SetExpressionRegion(JudgeExpressionRegionStr);
                JudgeExpressionAssembly = EScript.Compile();
            }

            if (JudgeExpressionAssembly != null)
            {
                return true;
            }

            return false;
        }

    }
}
