using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyOS.Common.communacation
{
    [Serializable]
    public class EComManageer
    {     
        public static Dictionary<string, ECommunacation> s_ECommunacationDic = new Dictionary<string, ECommunacation>();    //设为公开，可能有风险!

        static EComManageer()
        {
        }

        public static List<ECommunacation> GetEcomList()
        {
            return s_ECommunacationDic.Values.ToList();
        }

        //反序列化后刷新
        public static void setEcomList(List<ECommunacation> eComList)
        {
            foreach (string key in s_ECommunacationDic.Keys)
            {
                s_ECommunacationDic[key].DisConnect();
            }
            s_ECommunacationDic.Clear();

            if (eComList != null)
            {
                foreach (ECommunacation eCom in eComList)
                {
                    s_ECommunacationDic[eCom.Key] = eCom;
                    eCom.Connect();//开始连接
                }
            }
        }

        public static List<EComInfo> GetKeyList()
        {
            List<EComInfo> eComInfoList = new List<EComInfo>();
            foreach (string key in s_ECommunacationDic.Keys.ToList())
            {
                EComInfo eComInfo = new EComInfo(key, s_ECommunacationDic[key].IsConnected, s_ECommunacationDic[key].CommunicationModel);
                eComInfoList.Add(eComInfo);
            }
            return eComInfoList;
        }

        /// <summary>
        /// 获取对应的通讯备注
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetRemarks(string key)
        {

            ECommunacation eCommunacation = s_ECommunacationDic.Values.FirstOrDefault(c => c.Key == key);
            if (eCommunacation != null)
            {
                return eCommunacation.Remarks;
            }
            return "";
        }

        public static ECommunacation GetECommunacation(string key)
        {
            if (s_ECommunacationDic.ContainsKey(key))
            {
                return s_ECommunacationDic[key];
            }
            return null;
        }

        //创建
        public static string CreateECom(CommunicationModel communicationModel)
        {
            ECommunacation ec = new ECommunacation();
            ec.CommunicationModel = communicationModel;
            string key = "";
            switch (communicationModel)
            {
                case CommunicationModel.TcpClient:
                    key = "TCP客户端";
                    break;
                case CommunicationModel.TcpServer:
                    key = "TCP服务端";
                    break;
                case CommunicationModel.UDP:
                    key = "UDP";
                    break;
                case CommunicationModel.COM:
                    key = "串口";
                    break;
                default:
                    break;
            }

            //获取编码
            bool flag = false;
            int encode = 0;
            do
            {
                flag = true;
                foreach (ECommunacation tempEC in s_ECommunacationDic.Values)
                {
                    if (tempEC.Encode == encode)
                    {
                        encode++;
                        flag = false;
                        break;
                    }
                }

                if (flag == true)
                {
                    break;
                }
            } while (true);

            key = key + encode;
            ec.Key = key;
            ec.Encode = encode;
            s_ECommunacationDic[key] = ec;

            return key;
        }

        //删除
        public static void DeleteECom(string key)
        {
            if (!s_ECommunacationDic.ContainsKey(key)) return;
            ECommunacation ec = s_ECommunacationDic[key];
            ec.DisConnect();
            s_ECommunacationDic.Remove(key);
        }

        //连接
        public static bool Connect(string key)
        {
            if (!s_ECommunacationDic.ContainsKey(key)) return false;
            ECommunacation ec = s_ECommunacationDic[key];
            return ec.Connect();
        }

        //断开
        public static void DisConnect(string key)
        {
            if (!s_ECommunacationDic.ContainsKey(key)) return;
            ECommunacation ec = s_ECommunacationDic[key];
            ec.DisConnect();
        }

        //断开所有
        public static void DisConnectAll()
        {
            foreach (ECommunacation item in s_ECommunacationDic.Values)
            {
                item.DisConnect();
            }
        }

        //发送
        public static bool SendStr(string key, string str)
        {
            if (!s_ECommunacationDic.ContainsKey(key)) return false;
            ECommunacation ec = s_ECommunacationDic[key];
            return ec.SendStr(str);
        }

        //获取文本
        public static void GetEcomRecStr(string key, out string pReturnStr)
        {
            pReturnStr = "";
            if (!s_ECommunacationDic.ContainsKey(key)) return ;
            ECommunacation ec = s_ECommunacationDic[key];
            ec.GetStr(out  pReturnStr);
        }

        //停止阻塞
        public static void StopRecStrSignal(string key)
        {
            if (!s_ECommunacationDic.ContainsKey(key)) return ;
            ECommunacation ec = s_ECommunacationDic[key];
            ec.StopRecStrSignal();
        }

    }
}
