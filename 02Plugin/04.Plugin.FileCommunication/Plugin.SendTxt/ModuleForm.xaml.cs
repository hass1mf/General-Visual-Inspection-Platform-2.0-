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
using Heart.Inward;
using Heart.Outward;
using MyOS.Common.communacation;

namespace Plugin.SendTxt
{
    /// <summary>
    /// ModuleForm.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleForm : ModuleFormBase
    {

        #region 变量定义及初始化

        private ModuleObj m_ModuleObj;
        public ModuleForm()
        {
            InitializeComponent();
        }

        #endregion


        #region 插件模块保存与执行

        /// <summary>
        /// 加载插件时初始化
        /// </summary>
        public override void LoadModule()  //双击模块
        {
            m_ModuleObj = (ModuleObj)ModuleObjBase; //把module实例拿回来
            Title = m_ModuleObj.Info.ModuleName;    //设置插件的标题

            _CommunicationSetting.Items.Clear();    //通讯设置列表清空
            List<string> listName = new List<string>();
            List<ECommunacation> Ecom = EComManageer.GetEcomList();
            foreach (ECommunacation _ecom in Ecom)
            {
                //if (_ecom.IsConnected)   //已经连接过的才赋值进来
                listName.Add(_ecom.Key);
            }
            _CommunicationSetting.ItemsSource = listName;   //把从EComManageer拿过来的，并且是连接上的端口配置上
            _CommunicationSetting.Items.Refresh();

            _CommunicationSetting.Text = m_ModuleObj.m_CurKey;                  //复原通讯设置名称key
            _CommunicationRemark.Text = m_ModuleObj.CommunicationRemark;        //复原通讯备注
            _Linksendtext.Text = m_ModuleObj.hv_txt_Str;                        //复原发送内容
            _CommunicationEndSymbol.Text = m_ModuleObj.CommunicationEndSymbol;  //复原结束符
        }

        /// <summary>
        /// 点击执行按钮后前序函数
        /// </summary>
        public override void RunModuleBefore()
        {

        }

        /// <summary>
        /// 点击执行按钮后前序函数，中间还包含着Module内的ExeModule执行函数
        /// </summary>
        public override void RunModuleAfter()
        {
            
        }

        /// <summary>
        /// 点击确定会执行的唯一函数
        /// </summary>
        public override void SaveModuleBefore()   //确定之前
        {
            m_ModuleObj.m_CurKey = _CommunicationSetting.Text;
            m_ModuleObj.CommunicationRemark = _CommunicationRemark.Text;
            m_ModuleObj.hv_txt_Str = _Linksendtext.Text;
            m_ModuleObj.CommunicationEndSymbol = _CommunicationEndSymbol.Text;
        }

        #endregion


        #region 插件UI事件

        /// <summary>
        /// 通讯设置--下拉框选择通信源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _CommunicationSetting_SelectionChanged(object sender, SelectionChangedEventArgs e)   //通讯设置选择
        {
            string Str_Setting = _CommunicationSetting.SelectedValue.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
            if(Str_Setting != "")
            {
                m_ModuleObj.m_CurKey = Str_Setting;
                _CommunicationRemark.Text = m_ModuleObj.CommunicationRemark;
            }
        }

        /// <summary>
        /// 发送内容--链接文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _LinkVariable_Click(object sender, RoutedEventArgs e)   //链接按钮
        {
            VariableTable variableTable = new VariableTable();
            variableTable.SetModuleInfo(m_ModuleObj.Info.ProjectID, m_ModuleObj.Info.ModuleName, "");
            if (variableTable.ShowDialog() == true)
            {
                //数据操作部分
                m_ModuleObj.hv_txt_Str = variableTable.SelectedVariableText;    //通过list对话框，已经选择了要引用的变量
                _Linksendtext.Text = m_ModuleObj.hv_txt_Str;                    //ui链接文本显示
            }
        }

        /// <summary>
        /// 发送内容--删除链接文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _DeleteVariable_Click(object sender, RoutedEventArgs e)   //删除按钮
        {
            _Linksendtext.Text = "";
            m_ModuleObj.hv_txt_Str = null;
        }

        /// <summary>
        /// 结束符--下拉框选择结束符号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _CommunicationEndSymbol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Str_EndSymbol = _CommunicationEndSymbol.SelectedValue.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
            if (Str_EndSymbol != "")
            {
                m_ModuleObj.CommunicationEndSymbol = Str_EndSymbol;     //将结束符信息写入过去moduleObj
            }
        }

        #endregion



    }
}
