using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Heart.Inward;
using Heart.Outward;

namespace Plugin.LogicCondition
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

            _conditionDataGrid.ItemsSource = m_ModuleObj.m_ConditionVariableCollection;
            _conditionDataGrid.Items.Refresh();
        }

        public override void RunModuleBefore()
        {
            m_ModuleObj.UpdateExpressionAssembly();
        }

        public override void RunModuleAfter()
        {
            _conditionDataGrid.Items.Refresh();
        }

        public override void SaveModuleBefore()
        {
            m_ModuleObj.UpdateExpressionAssembly();
        }

        private void _conditionDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point pt = e.GetPosition(_conditionDataGrid);
            IInputElement obj = _conditionDataGrid.InputHitTest(pt);
            DependencyObject target = obj as DependencyObject;

            while (target != null)
            {
                if (target is System.Windows.Controls.DataGridCell)
                {
                    System.Windows.Controls.DataGridCell dCell = target as System.Windows.Controls.DataGridCell;
                    DataGridColumn dColumn = dCell.Column;
                    string headerStr = dColumn.Header as string;

                    if (headerStr.Trim() == "真值表达式")
                    {
                        while ((target != null) && !(target is DataGridRow))
                        {
                            target = VisualTreeHelper.GetParent(target);
                        }
                        DataGridRow dRow = target as DataGridRow;

                        int row = dRow.GetIndex();
                        int col = dColumn.DisplayIndex;

                        m_ExpressionEdit.SetExpression("bool", m_ModuleObj.m_ConditionVariableCollection[row].vExpression);
                        m_ExpressionEdit.ShowDialog();
                        if (m_ExpressionEdit.IsEditSucceed() == true)
                        {
                            m_ExpressionEdit.ClearEditResult();
                            m_ModuleObj.m_ConditionVariableCollection[row].vExpression = m_ExpressionEdit.GetExpressionText();
                            _conditionDataGrid.Items.Refresh();
                        }

                    }
                    break;
                }
                target = VisualTreeHelper.GetParent(target);
            }

        }
    }
}
