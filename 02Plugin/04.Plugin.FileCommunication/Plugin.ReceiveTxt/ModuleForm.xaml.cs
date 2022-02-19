using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Heart.Outward;
using Heart.Inward;
using System.IO;
using MyOS.Common.communacation;

namespace Plugin.ReceiveTxt
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
            _timeout.Visibility = Visibility.Collapsed;     //超时选项数字
            _Label_ms.Visibility = Visibility.Collapsed;    //标签--"毫秒"
        }

        #endregion


        #region 插件模块保存与执行

        /// <summary>
        /// 加载插件时初始化
        /// </summary>
        public override void LoadModule()   //双击模块
        {
            m_ModuleObj = (ModuleObj)ModuleObjBase;
            Title = m_ModuleObj.Info.ModuleName;

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
            _CommunicationEndSymbol.Text = m_ModuleObj.CommunicationEndSymbol;  //复原结束符
            _IsOpenTimeOut.IsChecked = m_ModuleObj.IsOpenTimeOut;               //复原是否开启超时
            _timeout.Text = m_ModuleObj.CommunicationTimeout.ToString();        //复原超时时间
        }

        /// <summary>
        /// 点击确定会执行的唯一函数
        /// </summary>
        public override void SaveModuleBefore()   //确定之前
        {
            m_ModuleObj.m_CurKey = _CommunicationSetting.Text;                  //通讯设置名称key
            m_ModuleObj.CommunicationRemark = _CommunicationRemark.Text;        //通讯备注
            m_ModuleObj.CommunicationEndSymbol = _CommunicationEndSymbol.Text;  //结束符
            m_ModuleObj.IsOpenTimeOut = _IsOpenTimeOut.IsChecked.Value;         //是否开启超时
            m_ModuleObj.CommunicationTimeout = int.Parse(_timeout.Text);        //超时时间
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
            string Str_Setting = _CommunicationSetting.SelectedValue.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");  //获取ui上选择的当前通讯是哪个选项
            if (Str_Setting != "")
            {
                m_ModuleObj.m_CurKey = Str_Setting;
                m_ModuleObj.CommunicationRemark = EComManageer.GetECommunacation(m_ModuleObj.m_CurKey).Remarks;     //备注
                _CommunicationRemark.Text = m_ModuleObj.CommunicationRemark;
            }
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

        /// <summary>
        /// 启用超时--开启
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _IsOpenTimeOut_Checked(object sender, RoutedEventArgs e)   //启用超时
        {
            //只做ui即可，因为SaveModuleBefore()内有数据传输
            _timeout.Visibility = Visibility.Visible;
            _Label_ms.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 启用超时--关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _IsOpenTimeOut_Unchecked(object sender, RoutedEventArgs e)   //不启用超时
        {
            //只做ui即可，因为SaveModuleBefore()内有数据传输
            _timeout.Visibility = Visibility.Collapsed;
            _Label_ms.Visibility = Visibility.Collapsed;
        }

        #endregion

    }
}
