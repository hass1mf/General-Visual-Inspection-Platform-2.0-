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
//VariableTable.xaml用于变量表设计，当模块需要调用变量时被调用并弹出

namespace Heart.Outward
{
    /// <summary>
    /// VariableTable.xaml 的交互逻辑
    /// </summary>
    public partial class VariableTable : Window
    {
        private int ProjectID = -1;
        private string vType = "";
        public string SelectedVariableText = "";
        public VariableTable()//初始化
        {
            InitializeComponent();
        }

        private void _moduleListView_SelectionChanged(object sender, SelectionChangedEventArgs e)//选择模块对应变量事件
        {
            string moduleName = (_moduleListView.SelectedItem as ModuleCell).mName;
            Project prj = Solution.Instance.GetProjectById(ProjectID);
            _variableListView.ItemsSource = prj.GetVariableInfoByModuleWithType(moduleName, vType);
            if (_variableListView.Items.Count > 0)
            {
                _variableListView.SelectedIndex = 0;
            }
        }

        private void _buttonSave_Click(object sender, RoutedEventArgs e)//确定按钮
        {
            if (_moduleListView.SelectedItem != null)
            { 
                string moduleName = (_moduleListView.SelectedItem as ModuleCell).mName;
                VariableInfo vInfo = _variableListView.SelectedItem as VariableInfo;

                SelectedVariableText = moduleName + "." + vInfo.vName;
            }

            DialogResult = true;

            this.Close();
        }

        private void _buttonCancel_Click(object sender, RoutedEventArgs e)//取消按钮
        {
            SelectedVariableText = "";

            DialogResult = false;
            this.Close();
        }

        public void SetModuleInfo(int projectID, string moduleName, string vtype)//加载&设置模块信息
        {
            ProjectID = projectID;
            vType = vtype;

            Project prj = Solution.Instance.GetProjectById(ProjectID);//此时系统筛选出了例如Image的类型出来
            List<string> moduleList = prj.GetAboveVariableModuleListByType(moduleName, vType);      //moduleList 相对于当前模块来说，前面的模块信息。通过这个过程，实现了对当前模块的前面模块信息的获取，因为模块拿其他模块的输出，只能拿上面之前模块的

            foreach (string m in moduleList)
            {
                _moduleListView.Items.Add(new ModuleCell(m));   //通过当前模块的前面模块列表List来重新映射出一个_moduleListView
            }

            if (_moduleListView.Items.Count > 0)
            {
                _moduleListView.SelectedIndex = 0;
                _variableListView.ItemsSource = prj.GetVariableInfoByModuleWithType(moduleList.First(), vType);     //public List<VariableInfo> GetVariableInfoByModuleWithType(string moduleName,string vType)//通过模块获取变量信息(变量名)  拿出当前模块的变量名
                if (_variableListView.Items.Count > 0)
                {
                    _variableListView.SelectedIndex = 0;
                }
            }
        }
    }
}
