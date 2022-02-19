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
using Heart.Inward;
using Heart.Outward;
using MyOS.Common.communacation;

namespace Plugin.SendTxt
{
    [Category("04@文件通讯")]
    [DisplayName("01@发送文本#发送文本")]
    [Serializable]
    public class ModuleObj : ModuleObjBase
    {
        public string m_CurKey { get; set; } = null;                //通讯设置名称key
        public string CommunicationRemark { get; set; } = null;     //通讯备注       
        public string hv_txt_Str { get; set; } = "";                //链接信息
        public string CommunicationEndSymbol { get; set; } = "无";  //结束符
        public string str_link { get; set; } = "";     //链接到的字符串信息
        public bool IsSendSuccess { get; set; } = false;    //当前发送是否成功

        /// <summary>
        /// ①执行模块
        /// </summary>
        /// <param name="entryName"></param>
        /// <returns></returns>
        public override int ExeModule(string entryName)     //执行
        {
            ReadStr();  //通过m_CurKey链接信息来获取str_link最终链接到的文本信息

            if (!string.IsNullOrEmpty(m_CurKey) && str_link != "")
            {
                string str;
                str = str_link;
                if (CommunicationEndSymbol != "无")   str += CommunicationEndSymbol;    //添加结束符信息
                if(EComManageer.GetECommunacation(m_CurKey) == null || !EComManageer.GetECommunacation(m_CurKey).IsConnected) return -1;    //当前没有这个通讯或者通讯没连上就直接返回错误-1
                if (EComManageer.SendStr(m_CurKey, str))    //针对m_CurKey来赋值 //m_CurKey没被序列化到！！！  //SendStr()函数自带返回执行结果
                {
                    IsSendSuccess = true;
                    return 1;
                }

            }
            IsSendSuccess = false;
            return -1;
        }

        /// <summary>
        /// ②变量输出
        /// </summary>
        public override void UpdateOutput()    //拖进流程、确定、单次
        {
            if (IsSendSuccess && !string.IsNullOrEmpty(hv_txt_Str))
            {
                AddOutputVariable("发送的文本", "bool", IsSendSuccess, IsSendSuccess.ToString(), CommunicationRemark);
            }
            else { AddOutputVariable("发送的文本", "bool", IsSendSuccess, IsSendSuccess.ToString(), CommunicationRemark); }
        }

        /// <summary>
        /// 给moduleform用的文本函数
        /// </summary>
        public void ReadStr()
        {
            if (hv_txt_Str != "")
            {
                str_link = GetVariableDataByVarName(hv_txt_Str).ToString();   //用文件索引来加载文件,并转换格式（IObject）   //20211022系统的传输图像变量改回为HImage，如果插件内部需要ROI内容，可自己将(himage)object实例为HImageExt
            }
        }
    }
}
