using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Heart.Outward
{
    [Serializable]
    public class EProcedure
    {
        public string Name { get; set; } = "EProcedure"; //函数名称

        public List<string> IconicInputList = new List<string>();  //iconic变量输入  io
        public List<string> IconicOutputList = new List<string>();  //iconic变量输出 oo

        public List<string> CtrlInputList = new List<string>();  //基础变量输入 ic
        public List<string> CtrlOutputList = new List<string>();  //基础变量输出 oc

        public string Body { get; set; } = "";//主要内容

        //添加 HObject输入
        public void AddIconInput(string paraName)
        {
            IconicInputList.Add(paraName);
        }

        //添加 HObject输出
        public void AddIconOutput(string paraName)
        {
            IconicOutputList.Add(paraName);
        }

        //添加 Htuple输入
        public void AddCtrlInput(string paraName)
        {
            CtrlInputList.Add(paraName);
        }

        //添加 Htuple输出
        public void AddCtrlOutput(string paraName)
        {
            CtrlOutputList.Add(paraName);
        }

        //获取方面名 类似 add_matrix( : : MatrixAID, MatrixBID : MatrixSumID)
        public string GetProcedureMethod()
        {
            string io = string.Join(",", IconicInputList);
            string oo = string.Join(",", IconicOutputList);
            string ic = string.Join(",", CtrlInputList);
            string oc = string.Join(",", CtrlOutputList);
            string method = $" {Name} ( {io} : {oo} : {ic} : {oc} )";
            return method;
        }

        //获取xml格式
        public static string GetXMLString(List<EProcedure> eProcedureList)
        {

            if (eProcedureList == null)
            {
                return "";
            }
            XmlDocument myXmlDoc = new XmlDocument();
            //<?xml version="1.0" encoding="UTF-8"?>
            myXmlDoc.AppendChild(myXmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null));

            //<hdevelop file_version="1.1" halcon_version="12.0">
            XmlElement hdevelop = myXmlDoc.CreateElement("hdevelop");
            hdevelop.SetAttribute("file_version", "1.1");
            hdevelop.SetAttribute("halcon_version", "12.0");
            myXmlDoc.AppendChild(hdevelop);

            foreach (EProcedure eProcedure in eProcedureList)
            {
                //  <procedure name="detect_fin">
                XmlElement procedure = myXmlDoc.CreateElement("procedure");
                procedure.SetAttribute("name", eProcedure.Name);
                hdevelop.AppendChild(procedure);

                //  <interface>
                XmlElement interfaceNode = myXmlDoc.CreateElement("interface");
                procedure.AppendChild(interfaceNode);

                //参数输入
                //<io> icon输入
                if (eProcedure.IconicInputList.Count > 0)
                {
                    XmlElement io = myXmlDoc.CreateElement("io");
                    foreach (string item in eProcedure.IconicInputList)
                    {
                        XmlElement par = myXmlDoc.CreateElement("par");
                        par.SetAttribute("name", item);
                        par.SetAttribute("base_type", "iconic");
                        par.SetAttribute("dimension", "0");
                        io.AppendChild(par);
                    }
                    interfaceNode.AppendChild(io);
                }

                // oo  icon输出 
                if (eProcedure.IconicOutputList.Count > 0)
                {
                    XmlElement oo = myXmlDoc.CreateElement("oo");
                    foreach (string item in eProcedure.IconicOutputList)
                    {
                        XmlElement par = myXmlDoc.CreateElement("par");
                        par.SetAttribute("name", item);
                        par.SetAttribute("base_type", "iconic");
                        par.SetAttribute("dimension", "0");
                        oo.AppendChild(par);
                    }
                    interfaceNode.AppendChild(oo);
                }
                //ic输入 
                if (eProcedure.CtrlInputList.Count > 0)
                {
                    XmlElement ic = myXmlDoc.CreateElement("ic");
                    foreach (string item in eProcedure.CtrlInputList)
                    {
                        XmlElement par = myXmlDoc.CreateElement("par");
                        par.SetAttribute("name", item);
                        par.SetAttribute("base_type", "ctrl");
                        par.SetAttribute("dimension", "0");
                        ic.AppendChild(par);
                    }
                    interfaceNode.AppendChild(ic);
                }
                // oc输出
                if (eProcedure.CtrlOutputList.Count > 0)
                {
                    XmlElement oc = myXmlDoc.CreateElement("oc");
                    foreach (string item in eProcedure.CtrlOutputList)
                    {
                        XmlElement par = myXmlDoc.CreateElement("par");
                        par.SetAttribute("name", item);
                        par.SetAttribute("base_type", "ctrl");
                        par.SetAttribute("dimension", "0");
                        oc.AppendChild(par);
                    }
                    interfaceNode.AppendChild(oc);
                }

                //body主体
                XmlElement body = myXmlDoc.CreateElement("body");
                procedure.AppendChild(body);

                string[] strArr = eProcedure.Body.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                foreach (string str in strArr)
                {
                    if (str.Trim().StartsWith("*"))
                    {
                        XmlElement line = myXmlDoc.CreateElement("c");//注释
                        line.InnerText = str;
                        body.AppendChild(line);
                    }
                    else
                    {
                        XmlElement line = myXmlDoc.CreateElement("l");//正常行
                        line.InnerText = str;
                        body.AppendChild(line);
                    }
                }
            }

            string xmlStr = "";
            using (StringWriter sw = new StringWriter())
            {
                XmlTextWriter xmlTextWriter = new XmlTextWriter(sw);
                myXmlDoc.WriteTo(xmlTextWriter);
                xmlStr = sw.ToString();
            }
            //  myXmlDoc.Save("detect_fin.hdvp");
            return xmlStr;
        }

        /// <summary>
        /// 返回读取xml里的的EProcedure
        /// </summary>
        /// <param name="path">xml路径</param>
        /// <returns></returns>
        public static List<EProcedure> LoadXmlByFile(string path)
        {

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            return GetEProcedureList(doc);
        }

        /// <summary>
        /// 根据 xmlstring 得到List<EProcedure> 
        /// </summary>
        /// <param name="xmlString">xml文本</param>
        /// <returns></returns>
        public static List<EProcedure> LoadXmlByString(string xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString)) return null;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlString);

            return GetEProcedureList(doc);
        }

        /// <summary>
        /// 导出hdev
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <param name="eProcedureList">list</param>
        public static void SaveToFile(string fileName, List<EProcedure> eProcedureList)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            sw.Write(GetXMLString(eProcedureList));
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            fs.Close();
        }

        /// <summary>
        ///解析xmldocument 获取 List<EProcedure>
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static List<EProcedure> GetEProcedureList(XmlDocument doc)
        {
            try
            {

                List<EProcedure> eProcedureList = new List<EProcedure>();

                //获取根节点
                XmlNodeList hdevelopList = doc.GetElementsByTagName("hdevelop");

                //获得所有 procedure节点
                XmlNodeList procedureList = hdevelopList[0].ChildNodes;

                //创建对应EProcedure

                foreach (XmlElement procedure in procedureList)
                {
                    EProcedure eProcedure = new EProcedure();

                    //获取名称
                    eProcedure.Name = procedure.GetAttribute("name");

                    //判断interface是否包含子元素
                    XmlNode interface1 = procedure.SelectSingleNode("interface");
                    if (interface1.ChildNodes.Count > 0)
                    {
                        //判断io
                        XmlNode io = interface1.SelectSingleNode("io");
                        if (io != null)
                        {
                            XmlNodeList parList = io.SelectNodes("par");
                            foreach (XmlElement par in parList)
                            {
                                eProcedure.AddIconInput(par.GetAttribute("name"));
                            }
                        }

                        //判断oo
                        XmlNode oo = interface1.SelectSingleNode("oo");
                        if (oo != null)
                        {
                            XmlNodeList parList = oo.SelectNodes("par");
                            foreach (XmlElement par in parList)
                            {
                                eProcedure.AddIconOutput(par.GetAttribute("name"));
                            }
                        }

                        //判断ic
                        XmlNode ic = interface1.SelectSingleNode("ic");
                        if (ic != null)
                        {
                            XmlNodeList parList = ic.SelectNodes("par");
                            foreach (XmlElement par in parList)
                            {
                                eProcedure.AddCtrlInput(par.GetAttribute("name"));
                            }
                        }

                        //判断oc
                        XmlNode oc = interface1.SelectSingleNode("oc");
                        if (oc != null)
                        {
                            XmlNodeList parList = oc.SelectNodes("par");
                            foreach (XmlElement par in parList)
                            {
                                eProcedure.AddCtrlOutput(par.GetAttribute("name"));
                            }
                        }
                    }

                    //获取body
                    XmlNode body = procedure.SelectSingleNode("body");

                    StringBuilder sb = new StringBuilder();
                    foreach (XmlNode line in body.ChildNodes)
                    {
                        sb.Append(line.InnerText + "\r\n");
                    }

                    eProcedure.Body = sb.ToString();

                    eProcedureList.Add(eProcedure);
                }

                return eProcedureList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载脚本文件失败" + ex.ToString());
                return null;
            }
        }
    }
}
