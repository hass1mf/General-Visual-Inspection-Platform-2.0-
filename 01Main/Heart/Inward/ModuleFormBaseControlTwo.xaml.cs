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
using System.ComponentModel;    //手动添加的引用，用于if (!DesignerProperties.GetIsInDesignMode(this))
//ModuleFormBaseControl是包含执行、确定、取消的控件。所有插件都必须添加，通过该控件来触发保存等事件

namespace Heart.Inward
{
    /// <summary>
    /// ModuleFormBaseControl.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleFormBaseControlTwo : UserControl
    {
        ///public Visibility RunBtnVisibility { get; set; }

        private ModuleFormBase m_ModuleFormBase;
        public ModuleFormBaseControlTwo()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// 选项框加载Load事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            m_ModuleFormBase = (ModuleFormBase)Window.GetWindow(this);
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                m_ModuleFormBase.ModuleFormBaseControlTwo = this;
                m_ModuleFormBase.BackupModule();    //回滚函数，实现在每次打开插件时都会先回滚之前的插件信息
                m_ModuleFormBase.LoadModule();      //这里实现了每个插件打开时，会先进每个插件的Load加载事件函数   注意这里是顺序是先回滚后操作
            }
        }

        //函数调用： 由UI form层到Obj层  ModuleFormBaseControl->ModuleFormBase->ModuleForm->ModuleObj
        //ModuleForm重载函数都与"执行确认取消"按钮绑定，来做保存或者执行模块前后的操作
        //所有的ModuleForm重载函数都是被ModuleFormBase内被调用，而这些重载函数的功能和变量，往往由ModuleObj内的函数来定义
        ///// <summary>
        ///// 按钮--执行
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void RunBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    m_ModuleFormBase.RunClick();    //执行按钮，该控件的三个按钮事件都是绑定调用与ModuleFormBase
        //}

        /// <summary>
        /// 按钮--确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            m_ModuleFormBase.SaveClick();
        }

        /// <summary>
        /// 按钮--取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            //  m_ModuleFormBase.CancelModule();
            m_ModuleFormBase.CancelClick();
        }

        public static implicit operator ModuleFormBaseControl(ModuleFormBaseControlTwo v)
        {
            throw new NotImplementedException();
        }
    }
}
