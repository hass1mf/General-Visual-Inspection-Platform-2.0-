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
//一开始创建只为了提供ModuleCell类型供VariableTable.xaml使用
//ExpressionEdit 表达式编辑  此控件窗体被变量定义插件时的页面单元格双击时触发弹开

namespace Heart.Outward
{
    /// <summary>
    /// ExpressionEdit.xaml 的交互逻辑
    /// </summary>
    public partial class ExpressionEdit : Window
    {
        private ExpressionScript EScript = new ExpressionScript();  //引用了ExpressionScript.cs类，用于编辑表达式时的编译问题
        private string EType;
        private int ProjectID;
        private string CompiledText = "";   //用于存放编辑区域文本，后续比较可能用得到
        private bool EditResult = false;    //编辑表达式编译结果判断
        public ExpressionEdit()//初始化
        {
            InitializeComponent();
        
            _codeTextEditor.Text = "";
        }
      
        private void _moduleListView_SelectionChanged(object sender, SelectionChangedEventArgs e)//左侧的模块列表选择发送改变时，触发变化函数来显示对应模块的变量
        {
            string moduleName = (_moduleListView.SelectedItem as ModuleCell).mName;
            Project prj = Solution.Instance.GetProjectById(ProjectID);          
            _variableListView.ItemsSource = prj.GetVariableInfoByModule(moduleName);
        }

        protected void _variableListView_DoubleClick(object sender, MouseButtonEventArgs e)//双击右侧单元格触发事件
        {
            string moduleName = (_moduleListView.SelectedItem as ModuleCell).mName;
            VariableInfo vInfo = _variableListView.SelectedItem as VariableInfo;

            List<string> filter = new List<string>{"int", "double", "string", "bool"};
            if (filter.Contains(vInfo.vType))
            {
                string text = "Get" + vInfo.vType.Substring(0, 1).ToUpper() + vInfo.vType.Substring(1) + "(\"" + moduleName + "." + vInfo.vName + "\")";
                _codeTextEditor.AppendText(text);
            }          
        }

        private void _buttonCheck_Click(object sender, RoutedEventArgs e)//检查按钮，仅做表达式编译和提示编译成功与否作用
        {
            if (CompiledText == _codeTextEditor.Text)//如果和上次一样就直接编译成功 _codeTextEditor.Text 编辑区域的文本
            {
                MessageBox.Show("编译成功");
            }
            else
            {
                EScript.SetExpressionLine(EType, _codeTextEditor.Text); //创建，从编辑表达式区域拿到的字符串给到m_Source
                if (EScript.Compile() != null)  //Compile()函数方法： 执行表达式编译，编译成功则保存且返回保存的文本，失败则返回null
                {
                    CompiledText = _codeTextEditor.Text;
                    MessageBox.Show("编译成功");
                }
                else
                {
                    MessageBox.Show(EScript.GetError());    //显示返回的错误信息
                }
            }
                
        }

        private void _buttonSave_Click(object sender, RoutedEventArgs e)//确定按钮，做表达式编译和提示编译失败信息，而且有EditResult结果的修改赋值
        {
            if (CompiledText == _codeTextEditor.Text)
            {
                EditResult = true;  //编辑表达式编译结果至true
                this.Close();
            }
            else
            {
                EScript.SetExpressionLine(EType, _codeTextEditor.Text);//创建，从编辑表达式区域拿到的字符串给到m_Source
                if (EScript.Compile() != null)//Compile()执行表达式编译，编译成功则保存且返回保存的文本，失败则返回null；使用非null即为编译成功
                {
                    EditResult = true;//编辑表达式编译结果至true
                    this.Close();
                }
                else
                {
                    MessageBox.Show(EScript.GetError());//弹框提示错误信息，并保持EditResult为false的结果
                }

            }                   
        }

        private void _buttonCancel_Click(object sender, RoutedEventArgs e)//取消按钮
        {
            this.Close();
        }

        public bool IsEditSucceed()//返回编辑编译是否成功，其中EditResult的值在_buttonSave_Click事件被修改
        {
            return EditResult;
        }

        public void ClearEditResult()//清除编辑编译结果，将EditResult至false
        {
            EditResult = false;
        }

        public string GetExpressionText()//获得返回编辑表达式的文本，可以用来显示到表达式单元格
        {
            return _codeTextEditor.Text;
        }

        public void SetExpression(string typeStr,string text)//建立获取表达式相关内容，获取文本框的内容并放置在CompiledText
        {
            EType = typeStr;
            _codeTextEditor.Text = text;
            CompiledText = _codeTextEditor.Text;
        }

        public void SetModuleInfo(int projectID, string moduleName)//建立显示设置模块信息
        {
            ProjectID = projectID;

            Project prj = Solution.Instance.GetProjectById(ProjectID);  //根据id获取对应的project
            List<string> moduleList = prj.GetAboveVariableModuleList(moduleName);

            foreach (string m in moduleList)
            {
                _moduleListView.Items.Add(new ModuleCell(m));
            }

            if (moduleList.Count > 0)
            {
                _moduleListView.SelectedIndex = 0;
                _variableListView.ItemsSource = prj.GetVariableInfoByModule(moduleList.First());
            }
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)//关闭
        {
            e.Cancel = true;     
            this.Hide();      //使窗口不可见
        }
    }

    class ModuleCell//模块单元
    {
        public ModuleCell(string name)
        {
            mName = name;
        }

        public string mName { get; set; }
    }
}
