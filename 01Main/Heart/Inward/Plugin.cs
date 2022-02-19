using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//Plugin.cs类自己独立，无需其他引用
//Plugin.cs类用于相关的插件信息加载
//Plugin.cs类被ToolBoxExt.xaml.cs类调用  foreach (PluginInfo info in Plugin.s_PluginInfoList) 第80行

namespace Heart.Inward
{
    /// <summary>
    /// 插件信息 
    /// </summary>
    [Serializable]
    public class PluginContent
    {
        public Type ModuleObjType { get; set; }//处理类
        public Type ModuleFormType { get; set; } = null;//ui   
    }

    public class PluginInfo : IComparable
    {
        public int CategoryNumber { get; set; }//分类序号
        public string Category { get; set; }//分类
        public int NameNumber { get; set; }//插件名称序号
        public string Name { get; set; }
        public string Defination { get; set; }//定义
        public ImageSource IconImage { get; set; }//图标显示
        public int CompareTo(object obj)	// s_PluginInfoList.Sort()时系统会自动寻找并进入到这个函数里面来
        {
            int result;		//sort有三种结果 1,-1,0分别是大，小，相等
            try
            {
                PluginInfo info = obj as PluginInfo;
                if ((this.CategoryNumber == info.CategoryNumber && this.NameNumber < info.NameNumber) || this.CategoryNumber < info.CategoryNumber)
                {
                    result = 1;
                }
                else
                    result = -1;
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public class Plugin
    {
        public static Dictionary<string, PluginContent> s_PluginContentDict = new Dictionary<string, PluginContent>();
        public static List<PluginInfo> s_PluginInfoList = new List<PluginInfo>();
        public static ImageSource moduleIcon;

		/// <summary>
        /// 初始化读取插件信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static void Init()
        {
            string PlugInsDir = System.Environment.CurrentDirectory;
            if (Directory.Exists(PlugInsDir) == false) return;//判断是否存在

            moduleIcon = new BitmapImage(new Uri("pack://application:,,,/" + "Heart" + ";Component/Icon/module.png"));

            //判断是否是UI.dll
            foreach (var dllFile in Directory.GetFiles(PlugInsDir))
            {
                try
                {
                    FileInfo fi = new FileInfo(dllFile);
                    //判断是否是Plugin.xxxxxxx.dll
                    if (!fi.Name.StartsWith("Plugin.") || !fi.Name.EndsWith(".dll")) continue;	//通过判断dll的字符名字来判断

                    Assembly assemPlugIn = AppDomain.CurrentDomain.Load(Assembly.LoadFile(fi.FullName).GetName());// 该方法会占用文件 但可以调试

                    //判断是否包含ModuleObjBase
                    foreach (Type type in assemPlugIn.GetTypes())	//这里应该是遍历了每个插件的内部cs类
                    {
                        if (typeof(ModuleObjBase).IsAssignableFrom(type))//是ModuleObjBase的子类	//通过判断插件内部的.cs类是否所属ModuleObjBase来判断是不是插件
                        {
                            PluginContent content = new PluginContent();
                            PluginInfo info = new PluginInfo();
                            //获取插件名称
                            string dllName = fi.Name.Substring(0, fi.Name.Length - 4);	//这里-4实现去掉名字后面的.dll
                            if (GetPluginContent(assemPlugIn, type, dllName, ref content, ref info))
                            {
                                s_PluginInfoList.Add(info);
                                s_PluginContentDict[info.Name] = content;
                            }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            s_PluginInfoList.Sort();
            s_PluginInfoList.Reverse(); 
        }

        /// <summary>
        /// 获取插件类别
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool GetPluginContent(Assembly assemPlugIn, Type type, string dllName, ref PluginContent content, ref PluginInfo info)
        {
            try
            {
                object[] categoryObjs = type.GetCustomAttributes(typeof(CategoryAttribute), true);
                object[] dispNameObjs = type.GetCustomAttributes(typeof(DisplayNameAttribute), true);

                string categoryStr = ((CategoryAttribute)categoryObjs[0]).Category;
                info.CategoryNumber = Convert.ToInt32(categoryStr.Split('@')[0]);
                info.Category = categoryStr.Split('@')[1];
                string nameStr = ((DisplayNameAttribute)dispNameObjs[0]).DisplayName;
                string[] nameStrArray = nameStr.Split('@');
                info.NameNumber = Convert.ToInt32(nameStrArray[0]);
                info.Name = nameStrArray[1].Split('#')[0];
                info.Defination = nameStrArray[1];

                bool iconExist = false;
                foreach (string resName in assemPlugIn.GetManifestResourceNames())
                {
                    Stream stream = assemPlugIn.GetManifestResourceStream(resName);
                    ResourceReader rr = new ResourceReader(stream);
                    IDictionaryEnumerator enumerator = rr.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        DictionaryEntry de = (DictionaryEntry)enumerator.Current;
                        string resourceName = (string)de.Key;
                        if (resourceName == "icon.png")
                        {
                            iconExist = true;
                        }
                    }
                }
                if (iconExist == true)
                {
                    info.IconImage = new BitmapImage(new Uri("pack://application:,,,/" + dllName + ";Component/icon.png"));
                }
                else
                {
                    info.IconImage = moduleIcon;
                }
                            
                content.ModuleObjType = type;

                //判断是否包含 ModuleFormBase
                foreach (Type tempType in assemPlugIn.GetTypes())
                {
                    if (typeof(ModuleFormBase).IsAssignableFrom(tempType))//是ModuleFormBase的子类
                    {
                        content.ModuleFormType = tempType;                       
                    }
                }

                return true;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return false;
        }
    }
}
