using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//ModuleFormBase类用于Form相关的操作，是Window的基类。插件内的ModuleForm.cs包括页面都要集成于ModuleFormBase

namespace Heart.Inward
{
    public class ModuleFormBase : Window
    {
        /// <summary>
        /// ui对应的moduleobj
        /// </summary>
        public ModuleObjBase ModuleObjBase { get; set; }
        /// <summary>
        /// 备份 取消的时候还原
        /// </summary>
        private ModuleObjBase m_ModuleObjBaseBack;

        public int CanvasCount { get; set; } = 1;
        public ModuleFormBaseControl ModuleFormBaseControl { get; set; }        //ui交互:执行、确定、取消
        public ModuleFormBaseControlTwo ModuleFormBaseControlTwo { get; set; }  //ui交互:确定、取消
        public ModuleFormBase()
        {
            this.Closed += Window_Closed;          
        }

        /// <summary>
        /// 插件内点击执行按钮--在执行模块前事件
        /// </summary>
        public virtual void RunModuleAfter()
        { }

        /// <summary>
        /// 插件内点击执行按钮--在执行模块后事件
        /// </summary>
        public virtual void RunModuleBefore()
        { }

        /// <summary>
        /// 点击确定按钮时--被执行的唯一函数
        /// </summary>
        public virtual void SaveModuleBefore()
        { }

        /// <summary>
        /// 每个插件打开时，会先进每个插件的Load加载事件函数
        /// </summary>
        public virtual void LoadModule()
        { }

        /// <summary>
        /// 回滚函数，实现在每次打开插件时都会先回滚之前的插件信息
        /// </summary>
        public void BackupModule()
        {
            if (ModuleObjBase == null) return;
            m_ModuleObjBaseBack = CloneObject.DeepCopy(ModuleObjBase);
        }

        /// <summary>
        /// 应该是清除了回滚的信息
        /// </summary>
        public void BackupRelease()
        {
            m_ModuleObjBaseBack = null;
            GC.Collect();
        }

        //函数调用： 由UI form层到Obj层  ModuleFormBaseControl->ModuleFormBase->ModuleForm->ModuleObj
        //ModuleForm重载函数都与"执行确认取消"按钮绑定，来做保存或者执行模块前后的操作
        //所有的ModuleForm重载函数都是被ModuleFormBase内被调用，而这些重载函数的功能和变量，往往由ModuleObj内的函数来定义
        public void RunClick()//执行按钮
        {
            RunModuleBefore();      //这里是ModuleForm的函数!!!

            DateTime tStart;
            DateTime tEnd;
            tStart = System.DateTime.Now;
            if (ModuleObjBase.ExeModule("") < 0)    //此处用""来代替传参        //这里是ModuleObj的函数!!!
            {
                ModuleFormBaseControl.txtStatus.Text = "状态： 失败";
            }
            else
            {
                ModuleFormBaseControl.txtStatus.Text = "状态： 成功";
            }
            tEnd = System.DateTime.Now;
            double tCostTime = (tEnd.Subtract(tStart)).TotalMilliseconds;
            ModuleFormBaseControl.txtUseTime.Text = "耗时： " + tCostTime.ToString("f0") + "ms";

            RunModuleAfter();   //此时调用各个插件内的RunModuleAfter()事件  //这里是ModuleForm的函数!!!
        }

        /// <summary>
        /// 插件UI上确定函数，这里会调用唯一的SaveModuleBefore()
        /// </summary>
        public void SaveClick()//确定按钮
        {
            SaveModuleBefore();     //此时调用各个插件内的SaveModuleBefore()事件
            ModuleObjBase.UpdateOutputVariable();   //更新变量数据时要调用ModuleObjBase内的函数
            DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// 插件UI上取消按钮，这里只给了DialogResult信号为false
        /// </summary>
        public void CancelClick()//取消按钮
        {            
            
            DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// 窗体正式关闭函数，如果DialogResult = false就默认取消保存，此时就会回滚插件数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            if (DialogResult == false)
            {
                Project prj = Solution.Instance.GetProjectById(m_ModuleObjBaseBack.Info.ProjectID);
                prj.RecoverModuleObj(m_ModuleObjBaseBack);  //还原模块数据函数，相当于取消保存
            }            
        }
    }
}
