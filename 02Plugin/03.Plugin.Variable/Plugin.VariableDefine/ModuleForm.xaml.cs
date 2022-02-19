using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.IO;
using Heart.Inward;
using Heart.Outward;


namespace Plugin.VariableDefine
{
    /// <summary>
    /// ModuleForm.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleForm : ModuleFormBase
    {
        private ModuleObj m_ModuleObj;
        private ExpressionEdit m_ExpressionEdit;

        public ModuleForm()
        {
            InitializeComponent();
            this.ContentRendered += Window_ContentRendered;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            m_ExpressionEdit = new ExpressionEdit();
            m_ExpressionEdit.SetModuleInfo(m_ModuleObj.Info.ProjectID, m_ModuleObj.Info.ModuleName);
        }

        public override void LoadModule()
        {
            m_ModuleObj = (ModuleObj)ModuleObjBase;
            Title = m_ModuleObj.Info.ModuleName;                        
            _dataGrid.ItemsSource = m_ModuleObj.VariableInfoExCollection;
        }

        public override void RunModuleBefore()
        {
            m_ModuleObj.UpdateExpressionAssembly();
        }

        public override void RunModuleAfter()
        {
            _dataGrid.Items.Refresh();
        }

        public override void SaveModuleBefore()
        {
            string assemblyFileName = "pro" + m_ModuleObj.Info.ProjectID.ToString() + "." + m_ModuleObj.Info.ModuleName;

            int number = 0;
            bool found = false;
            int i = 0;
            while (!found)
            {
                number = i;
                try
                {
                    bool file_exist = false;
                    string dllFile = assemblyFileName + i.ToString() + ".dll";
                    if (File.Exists(dllFile))
                    {
                        file_exist = true;
                        File.SetAttributes(dllFile, FileAttributes.Normal);
                        File.Delete(dllFile);
                    }                 

                    string pdbFile = assemblyFileName + i.ToString() + ".pdb";
                    if (File.Exists(pdbFile))
                    {
                        file_exist = true;
                        File.SetAttributes(pdbFile, FileAttributes.Normal);
                        File.Delete(pdbFile);
                    }

                    if (file_exist == false)
                    {
                        found = true;
                        break;
                    }
                    
                }
                catch (System.UnauthorizedAccessException ue){}
                i++;
            }
            
            m_ModuleObj.UpdateExpressionAssembly();

        }

        private void _buttonAddInt_Click(object sender, RoutedEventArgs e)
        {
            AddingNewItem("int");
        }

        private void _buttonAddDouble_Click(object sender, RoutedEventArgs e)
        {
            AddingNewItem("double");
        }

        private void _buttonAddString_Click(object sender, RoutedEventArgs e)
        {
            AddingNewItem("string");
        }

        private void _buttonAddBool_Click(object sender, RoutedEventArgs e)
        {
            AddingNewItem("bool");
        }

        private void _buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            int index = _dataGrid.SelectedIndex;

            if (index < 0)
            {
                return;
            }

            string cName = m_ModuleObj.VariableInfoExCollection[index].vName;
            switch (m_ModuleObj.VariableInfoExCollection[index].vType)
            {
                case "int":
                    m_ModuleObj.IntVariableDict.Remove(cName);
                    break;
                case "double":
                    m_ModuleObj.DoubleVariableDict.Remove(cName);
                    break;
                case "string":
                    m_ModuleObj.StringVariableDict.Remove(cName);
                    break;
                case "bool":
                    m_ModuleObj.BoolVariableDict.Remove(cName);
                    break;
            }

            
            m_ModuleObj.VariableInfoExCollection.RemoveAt(index);
            _dataGrid.Items.Refresh();       
        }

        private void _dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(_dataGrid);
            IInputElement obj = _dataGrid.InputHitTest(pt);
            DependencyObject target = obj as DependencyObject;

            while (target != null)
            {
                if (target is DataGridCell)
                {
                    DataGridCell dCell = target as DataGridCell;
                    DataGridColumn dColumn = dCell.Column;
                    string headerStr = dColumn.Header as string;
                    if ((headerStr.Trim() == "名称") || (headerStr.Trim() == "备注"))
                    {
                        dCell.IsEditing = true;
                    }
                    else if (headerStr.Trim() == "表达式")
                    {                       
                        while ((target != null) && !(target is DataGridRow))
                        {
                            target = VisualTreeHelper.GetParent(target);
                        }
                        DataGridRow dRow = target as DataGridRow;

                        int row = dRow.GetIndex();
                        int col = dColumn.DisplayIndex;


                        m_ExpressionEdit.SetExpression(m_ModuleObj.VariableInfoExCollection[row].vType,m_ModuleObj.VariableInfoExCollection[row].vExpression);
                        m_ExpressionEdit.ShowDialog();
                        if (m_ExpressionEdit.IsEditSucceed() == true)
                        {
                            m_ExpressionEdit.ClearEditResult();
                            m_ModuleObj.VariableInfoExCollection[row].vExpression = m_ExpressionEdit.GetExpressionText();
                            _dataGrid.Items.Refresh();
                        }

                    }
                    break;
                }
                target = VisualTreeHelper.GetParent(target);
            }
        }

        private void _dataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(_dataGrid);
            IInputElement obj = _dataGrid.InputHitTest(pt);
            DependencyObject target = obj as DependencyObject;

            while (target != null)
            {
                if (target is DataGridRow)
                {
                    (target as DataGridRow).IsSelected = true;
                    return;
                }
                target = VisualTreeHelper.GetParent(target);
            }

            _dataGrid.CommitEdit();
            _dataGrid.UnselectAll();
        }

        private void _dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string newValue = (e.EditingElement as TextBox).Text;

            int row = e.Row.GetIndex();
            int col = e.Column.DisplayIndex;

            bool IsUpdate = true;
            if (col == 1)
            {
                List<string> vNameList = m_ModuleObj.VariableInfoExCollection.Select(c => c.vName).ToList();
                if (vNameList.Contains(newValue))
                {
                    IsUpdate = false;
                }
            }

            if (IsUpdate)
            {
                switch (col)
                {
                    case 1:
                        string cName = m_ModuleObj.VariableInfoExCollection[row].vName;
                        switch (m_ModuleObj.VariableInfoExCollection[row].vType)
                        {
                            case "int":
                                int intValue = m_ModuleObj.IntVariableDict[cName];
                                m_ModuleObj.IntVariableDict.Remove(cName);
                                m_ModuleObj.IntVariableDict.Add(newValue, intValue);
                                break;
                            case "double":
                                double doubleValue = m_ModuleObj.DoubleVariableDict[cName];
                                m_ModuleObj.DoubleVariableDict.Remove(cName);
                                m_ModuleObj.DoubleVariableDict.Add(newValue, doubleValue);
                                break;
                            case "string":
                                string stringValue = m_ModuleObj.StringVariableDict[cName];
                                m_ModuleObj.StringVariableDict.Remove(cName);
                                m_ModuleObj.StringVariableDict.Add(newValue, stringValue);
                                break;
                            case "bool":
                                bool boolValue = m_ModuleObj.BoolVariableDict[cName];
                                m_ModuleObj.BoolVariableDict.Remove(cName);
                                m_ModuleObj.BoolVariableDict.Add(newValue, boolValue);
                                break;
                        }
                        m_ModuleObj.VariableInfoExCollection[row].vName = newValue;
                        break;
                    case 4:
                        m_ModuleObj.VariableInfoExCollection[row].vRemark = newValue;
                        break;
                    default:
                        break;
                }
                
            }
            _dataGrid.Items.Refresh();
        }

        public void AddingNewItem(string typeStr)
        {
            List<string> vNameList = m_ModuleObj.VariableInfoExCollection.Select(c => c.vName).ToList();

            string vName = "value";
            for (int i = 0; i <= vNameList.Count; ++i)
            {
                if (vNameList.FindIndex(c => c == vName + i.ToString()) < 0)
                {
                    vName += i.ToString();
                    break;
                }
            }

            string expressionValue = "";
            string initValue = "";

            switch (typeStr)
            {
                case "int":
                    m_ModuleObj.IntVariableDict.Add(vName, 0);
                    expressionValue = "0";
                    initValue = "0";
                    break;
                case "double":
                    m_ModuleObj.DoubleVariableDict.Add(vName, 0);
                    expressionValue = "0";
                    initValue = "0";
                    break;
                case "string":
                    m_ModuleObj.StringVariableDict.Add(vName, "");
                    expressionValue = "\"\"";
                    initValue = "";
                    break;
                case "bool":
                    m_ModuleObj.BoolVariableDict.Add(vName, false);
                    expressionValue = "false";
                    initValue = "false";
                    break;
            }

            VariableInfoEx gVariable = new VariableInfoEx(typeStr, vName, expressionValue, initValue, "");

            m_ModuleObj.VariableInfoExCollection.Add(gVariable);
            m_ModuleObj.UpdateOutputVariable();
            m_ModuleObj.UpdateOutput();
            _dataGrid.Items.Refresh();         
        }
    }

}
