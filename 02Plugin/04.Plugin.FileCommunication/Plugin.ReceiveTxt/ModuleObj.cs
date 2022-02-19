using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Heart.Inward;
using Heart.Outward;
using System.Threading;
using MyOS.Common.communacation;

namespace Plugin.ReceiveTxt
{
    [Category("04@文件通讯")]
    [DisplayName("00@接收文本#接收文本")]
    [Serializable]
    public class ModuleObj : ModuleObjBase
    {
        public string m_CurKey { get; set; } = null;                //通讯设置名称key
        public string CommunicationRemark { get; set; } = null;     //通讯备注
        public string CommunicationEndSymbol { get; set; } = "无";  //结束符
        public bool IsOpenTimeOut { get; set; } = false;            //是否开启超时
        public int CommunicationTimeout { get; set; } = 0;          //超时时间
        public bool IsRecvSuccess { get; set; } = false;            //当前发送是否成功

        public string Str_Recv  = "";                               //当前发送的str
        [NonSerialized]
        Thread thread;                                              //接收超时线程

        /// <summary>
        /// ①执行模块
        /// </summary>
        /// <param name="entryName"></param>
        /// <returns></returns>
        public override int ExeModule(string entryName)  //执行按钮
        {
            Str_Recv = "";
            IsRecvSuccess = false;
            if (!string.IsNullOrEmpty(m_CurKey))
            {
                if (EComManageer.GetECommunacation(m_CurKey) == null || !EComManageer.GetECommunacation(m_CurKey).IsConnected) return -1;    //当前没有这个通讯或者通讯没连上就直接返回错误-1
                if (IsOpenTimeOut)  //是否开启超时
                {            
                    thread = new Thread(new ThreadStart(OpenThreadTimeout));
                    thread.IsBackground = true;     //后台执行线程
                    thread.Start();                 //开始线程
                    EComManageer.GetEcomRecStr(m_CurKey, out Str_Recv);    //会阻塞等待  可以通过调用 EComManageer.StopRecStrSignal(m_CurKey);    //停止
                }
                else
                {
                    EComManageer.GetEcomRecStr(m_CurKey, out Str_Recv);    //会阻塞等待  可以通过调用 EComManageer.StopRecStrSignal(m_CurKey);    //停止
                }
                if(Str_Recv != "")
                {
                    IsRecvSuccess = true;
                    return 1;
                }
            }
            return -1;
        }

        /// <summary>
        /// ②变量输出
        /// </summary>
        public override void UpdateOutput()   //拖进流程、确定、单次
        {
            if (thread != null) 
            {
                thread.Abort();                 //关掉线程
                thread = null;
            }
            
            if (IsRecvSuccess)
            {
                AddOutputVariable("接收到的文本", "string", Str_Recv, Str_Recv, CommunicationRemark);
            }
            else { AddOutputVariable("接收到的文本", "string", "", "null", CommunicationRemark); }

        }

        /// <summary>
        /// 辅助函数--超时线程
        /// </summary>
        public void OpenThreadTimeout()
        {
            int second = CommunicationTimeout;   //倒计一分钟
            bool flag = true;
            while (flag)
            {
                Thread.Sleep(100);  //要先延迟，然后再做减法做判断
                second = second - 100;
                if (second <= 0)
                {
                    //Application.Exit();//计时完成后推出应用程序
                    EComManageer.StopRecStrSignal(m_CurKey);    //停止
                    Application.ExitThread();   //关闭当前线程
                    flag = false;
                    //Thread.Sleep(10000);
                } 
            }
        }
    }
}
