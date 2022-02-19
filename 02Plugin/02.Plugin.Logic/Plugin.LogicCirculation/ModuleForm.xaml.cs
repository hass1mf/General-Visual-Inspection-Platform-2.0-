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

namespace Plugin.LogicCirculation
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

            _truthTextBox.Text = m_ModuleObj.ExpressionDict["truth"];
            _initialTextBox.Text = m_ModuleObj.ExpressionDict["initial"];
            _finalTextBox.Text = m_ModuleObj.ExpressionDict["final"];

            _initialResultTextBlock.Text = m_ModuleObj.InitialValueStr;
            _finalResultTextBlock.Text = m_ModuleObj.FinalValueStr;
            _truthResultTextBlock.Text = m_ModuleObj.TruthValueStr;

            if (m_ModuleObj.IsTraversalChecked == true)
            {
                _judgeRadioButton.IsChecked = false;
                _traversalRadioButton.IsChecked = true;
            }
            else
            {
                _judgeRadioButton.IsChecked = true;
                _traversalRadioButton.IsChecked = false;
            }
            
        }

        public override void RunModuleBefore()
        {

        }

        public override void RunModuleAfter()
        {
            if (m_ModuleObj.IsTraversalChecked == true)
            {
                _initialResultTextBlock.Text = m_ModuleObj.InitialValueStr;
                _finalResultTextBlock.Text = m_ModuleObj.FinalValueStr;
            }
            else
            {
                _truthResultTextBlock.Text = m_ModuleObj.TruthValueStr;
            }                    
        }

        public override void SaveModuleBefore()
        {
            if (m_ModuleObj.IsTraversalChecked == true)
            {
                m_ModuleObj.TraversalUpdateExpressionAssembly();
            }
            else
            {
                m_ModuleObj.JudgeUpdateExpressionAssembly();
            }                         
        }

        private void _truthTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            HandleExpressionEdit("bool", "truth", _truthTextBox);
        }

        private void _initialTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            HandleExpressionEdit("int", "initial", _initialTextBox);
        }

        private void _finalTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            HandleExpressionEdit("int", "final", _finalTextBox);
        }

        private void _judgeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            m_ModuleObj.IsTraversalChecked = false;
            _truthTextBox.IsEnabled = true;
            _initialTextBox.IsEnabled = false;
            _finalTextBox.IsEnabled = false;
        }

        private void _traversalRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            m_ModuleObj.IsTraversalChecked = true;
            _initialTextBox.IsEnabled = true;
            _finalTextBox.IsEnabled = true;
            _truthTextBox.IsEnabled = false;
        }

        public void HandleExpressionEdit(string etype,string target,TextBox targetTextBox)
        {
            m_ExpressionEdit.SetExpression(etype, m_ModuleObj.ExpressionDict[target]);
            m_ExpressionEdit.ShowDialog();
            if (m_ExpressionEdit.IsEditSucceed() == true)
            {
                m_ExpressionEdit.ClearEditResult();
                m_ModuleObj.ExpressionDict[target] = m_ExpressionEdit.GetExpressionText();
                targetTextBox.Text = m_ModuleObj.ExpressionDict[target];
            }

        }



    }
}
