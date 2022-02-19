using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Heart.Inward;
using Heart.Outward;
using HalconDotNet;

namespace Plugin.ImageScript
{
    [Category("00@图像处理")]
    [DisplayName("01@图像脚本#图像脚本")]
    [Serializable]
    class ModuleObj : ModuleObjBase
    {
        public List<EProcedure> m_EProcedureList = null;   
        public string m_ExecuteProcedureName = "";
        public string m_EditProcedureName = "";

        public List<string> m_ColorProcedureNameList = new List<string>();//记录内部函数的名称,用以染色

        public List<ProcedureItem> m_EditProcedureNameList = new List<ProcedureItem>();
        public List<ProcedureItem> m_ExecuteProcedureNameList = new List<ProcedureItem>();

        public ObservableCollection<InputVariable> m_InputVariableCollection = new ObservableCollection<InputVariable>();
        public ObservableCollection<OutputVariable> m_OutputVariableCollection = new ObservableCollection<OutputVariable>();

        public Dictionary<string, object> m_OutputVariableDict = new Dictionary<string, object>();

        public List<string> m_OutputTypeList = new List<string>{ "int","double","string","bool","Image","Region"};

        public override int ExeModule(string entryName)
        {
            int result = 1;

            string tempName = Environment.GetEnvironmentVariable("TEMP") + $"/{Info.ProjectID}.{Info.ModuleName}.hdev";
            EProcedure.SaveToFile(tempName, m_EProcedureList);
          
            try
            {
                m_OutputVariableDict.Clear();
                                          
                HDevProgram program = new HDevProgram(tempName);
                HDevProcedure procedure = new HDevProcedure(program, m_ExecuteProcedureName);
                HDevProcedureCall m_HDevProcedureCall = new HDevProcedureCall(procedure);              
                
                foreach (InputVariable item in m_InputVariableCollection)
                {
                    switch (item.vGroup)
                    {
                        case "Ctrl":
                            if (item.vLink == "")
                            {
                                result = -1;
                            }
                            m_HDevProcedureCall.SetInputCtrlParamTuple(item.vName, (HTuple)GetVariableDataByVarName(item.vLink));
                            break;
                        case "Iconic":
                            if (item.vLink == "")
                            {
                                result = -1;
                            }
                            m_HDevProcedureCall.SetInputIconicParamObject(item.vName, (HObject)GetVariableDataByVarName(item.vLink));
                            break;
                    }
                }

                m_HDevProcedureCall.Execute();

                foreach (OutputVariable item in m_OutputVariableCollection)
                {
                    switch (item.vType)
                    {
                        case "int":
                            int outputInt = m_HDevProcedureCall.GetOutputCtrlParamTuple(item.vName);
                            m_OutputVariableDict[item.vName] = outputInt;
                            item.vValue = outputInt.ToString();
                            break;
                        case "double":
                            double outputDouble = m_HDevProcedureCall.GetOutputCtrlParamTuple(item.vName);
                            m_OutputVariableDict[item.vName] = outputDouble;
                            item.vValue = outputDouble.ToString();
                            break;
                        case "string":
                            string outputString = m_HDevProcedureCall.GetOutputCtrlParamTuple(item.vName);
                            m_OutputVariableDict[item.vName] = outputString;
                            item.vValue = outputString.ToString();
                            break;
                        case "bool":
                            bool outputBool = m_HDevProcedureCall.GetOutputCtrlParamTuple(item.vName);
                            m_OutputVariableDict[item.vName] = outputBool;
                            item.vValue = outputBool.ToString();
                            break;
                        case "Image":
                            HImage outputImage = m_HDevProcedureCall.GetOutputIconicParamImage(item.vName);
                            m_OutputVariableDict[item.vName] = outputImage;
                            item.vValue = outputImage.ToString();
                            break;
                        case "Region":
                            HRegion outputRegion = m_HDevProcedureCall.GetOutputIconicParamRegion(item.vName);
                            m_OutputVariableDict[item.vName] = outputRegion;
                            item.vValue = outputRegion.ToString();
                            break;
                        case "Object":
                            HObject outputObject = m_HDevProcedureCall.GetOutputIconicParamObject(item.vName);
                            m_OutputVariableDict[item.vName] = outputObject;
                            item.vValue = outputObject.ToString();
                            break;
                    }
                }


            }
            catch (HDevEngineException he)
            {
                result = -1;
            }

            File.Delete(tempName);

            return result;
        }

        public bool Compile()
        {
            bool compiled = true;

            string tempName = Environment.GetEnvironmentVariable("TEMP") + $"/{Info.ProjectID}.{Info.ModuleName}.hdev";
            EProcedure.SaveToFile(tempName, m_EProcedureList);
           
            try
            {              
                HDevProgram program = new HDevProgram(tempName);
                HDevProcedure procedure = new HDevProcedure(program, m_ExecuteProcedureName);
                HDevProcedureCall m_HDevProcedureCall = new HDevProcedureCall(procedure);         
            }
            catch (HDevEngineException he)
            {
                compiled = false;
            }

            File.Delete(tempName);

            return compiled;
        }

        public override void UpdateOutput()
        {
            foreach (OutputVariable item in m_OutputVariableCollection)
            {
                if (m_OutputVariableDict.ContainsKey(item.vName))
                {
                    AddOutputVariable(item.vName, item.vType, m_OutputVariableDict[item.vName], item.vValue, item.vRemark);
                }                        
            }
        }

        public override void UpdateDisplay()
        {
            
        }

        public void UpdateInputVariableCollection()
        {
            m_InputVariableCollection.Clear();

            EProcedure eProcedure = m_EProcedureList.Find(e => e.Name == m_ExecuteProcedureName);
            if (eProcedure != null)
            {
                foreach (string item in eProcedure.IconicInputList)
                {
                    m_InputVariableCollection.Add(new InputVariable(item, "Iconic", "")); 
                }

                foreach (string item in eProcedure.CtrlInputList)
                {
                    m_InputVariableCollection.Add(new InputVariable(item, "Ctrl", ""));
                }

            }
            
        }

        public void UpdateOutputVariableCollection()
        {
            m_OutputVariableCollection.Clear();

            EProcedure eProcedure = m_EProcedureList.Find(e => e.Name == m_ExecuteProcedureName);
            if (eProcedure != null)
            {
                foreach (string item in eProcedure.IconicOutputList)
                {
                    m_OutputVariableCollection.Add(new OutputVariable(item, "Iconic", "", "", ""));
                }

                foreach (string item in eProcedure.CtrlOutputList)
                {
                    m_OutputVariableCollection.Add(new OutputVariable(item, "Ctrl", "", "", ""));
                }
            }

        }
    }

    [Serializable]
    public class InputVariable
    {
        public InputVariable(string vname, string vgroup, string vlink)
        {
            vName = vname;
            vGroup = vgroup;
            vLink = vlink;
        }
        public string vName { get; set; }
        public string vLink { get; set; }
        public string vGroup { get; set; }
    }

    [Serializable]
    public class OutputVariable 
    {
        public OutputVariable(string vname, string vgroup, string vtype, string vvalue, string vremark)
        {
            vType = vtype;
            vName = vname;
            vGroup = vgroup;
            vValue = vvalue;
            vRemark = vremark;
        }
        public string vType { get; set; }
        public string vName { get; set; }
        public string vGroup { get; set; }
        public string vValue { get; set; }
        public string vRemark { get; set; }

    }

    [Serializable]
    class ProcedureItem
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }

}
