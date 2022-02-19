using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Heart.Inward;
//ExpressionScript类自己独立，无需其他引用
//ExpressionScript类用于编辑表达式时处理表达式的编译
//ExpressionScript类被ExpressionEdit.xaml.cs调用  private ExpressionScript EScript = new ExpressionScript(); 第26行

namespace Heart.Outward
{
    [Serializable]
    public class ExpressionScript
    {
        public int ProjectID = -1;

        private string m_ErrorText;     //编译时返回的错误信息
        private string m_Source;        //从编辑表达式区域拿到的字符串

        //引用的dll
        private ArrayList RelativeAssembliesName = new ArrayList {  //RelativeAssembliesName 相关引用程序集名称
                                            "System.dll",
                                            "System.Core.dll",
                                            "mscorlib.dll",
                                            "System.Data.dll",                                           
                                            };

        public ExpressionScript()
        {
            //脚本里需要用到哪个类，就在这里注册
            AddReferenceAssemblyByType(this.GetType());
            AddReferenceAssemblyByType(typeof(List<object>));
            AddReferenceAssemblyByType(typeof(Math));
            AddReferenceAssemblyByType(typeof(Thread));
            AddReferenceAssemblyByType(typeof(AutoResetEvent));
            AddReferenceAssemblyByType(typeof(ExpressionScriptData));           
        }

        public void SetProjectID(int projectID)//获得流程ID
        {
            ProjectID = projectID;
        }

        public void SetExpressionLine(string typeStr,string expressionStr)//创建，从编辑表达式区域拿到的字符串；  expressionStr：获取到的表达式串
        {
            if ((!String.IsNullOrEmpty(typeStr))&&(!String.IsNullOrEmpty(expressionStr)))
            {              
                m_Source = s_RawScript.Replace("#ExpressionRegion#", s_ExpressionMask.Replace("(typeStr)(expressionStr)", "(" + typeStr + ")(" + expressionStr + ")"));
            }
        }

        public string GenerateExpressionLine(string typeStr, string expressionStr)//返回了表达式字符串m_Source；GenerateExpressionLine 生成表达式行
        {
            return s_ExpressionMask.Replace("(typeStr)(expressionStr)", "(" + typeStr + ")(" + expressionStr + ")");
        }

        public void SetExpressionRegion(string expressionRegionStr)//返回了表达式字符串m_Source；expressionRegionStr 表达式区域
        {
            if (!String.IsNullOrEmpty(expressionRegionStr))
            {
                m_Source = s_RawScript.Replace("#ExpressionRegion#", expressionRegionStr);
            }
        }

        public string GetError()//返回错误信息
        {
            return m_ErrorText;
        }
        
        private void AddReferenceAssemblyByType(Type SourceType)
        {

            if (SourceType == null)
            {
                throw new ArgumentNullException("SourceType为空");
            }
            System.Uri uri = new Uri(SourceType.Assembly.CodeBase);
            string path = null;
            if (uri.Scheme == System.Uri.UriSchemeFile)
            {
                path = uri.LocalPath;
            }
            else
            {
                path = uri.AbsoluteUri;
            }

            if (this.RelativeAssembliesName.Contains(path) == false)
            {
                this.RelativeAssembliesName.Add(path);
            }
            string str = SourceType.Namespace;//命名空间

        }
    
        public byte[] Compile()//执行表达式编译，编译成功则保存且返回保存的文本，失败则返回null
        {
            if (String.IsNullOrEmpty(m_Source))     //判断m_Source是空还是null  m_Source 从编辑表达式区域拿到的字符串
            {
                return null;
            }

            bool Compiled = false;  //单纯内部的变量
            Exception exception;    //Exception类 表示在应用程序执行过程中发生的错误

            CompilerParameters options = new CompilerParameters     //CompilerParameters类 表示用于调用编译器的参数
            {
                GenerateExecutable = false,         //获取或设置一个值，该值指示是否生成可执行文件。返回结果:如果应生成可执行文件，则为 true；否则为 false。
                GenerateInMemory = false,           //获取或设置一个值，该值指示是否在内存中生成输出
                IncludeDebugInformation = false,    //获取或设置一个值，该值指示是否在已编译的可执行文件中包含调试信息
                WarningLevel = 4,                   //获取或设置使编译器中止编译的警告等级
                TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true),   //TempFiles 获取或设置包含临时文件的集合；GetEnvironmentVariable 从当前进程检索环境变量的值
            };

            foreach (String str in RelativeAssembliesName)  //RelativeAssembliesName 相关引用程序集名称
            {
                options.ReferencedAssemblies.Add(str);  //ReferencedAssemblies 获取当前项目所引用的程序集；ReferencedAssemblies是CompilerParameters类下的函数方法；此时的Add函数方法在更高层的类中被定义，而不是CompilerParameters类
            }

            CompilerResults results = null;     //CompilerResults类 表示从编译器返回的编译结果
            CSharpCodeProvider objCSharpCodePrivoder = new CSharpCodeProvider();    //CSharpCodeProvider类 提供对 C# 代码生成器和代码编译器的实例的访问权限

            try
            {
                results = objCSharpCodePrivoder.CompileAssemblyFromSource(options, new string[] { m_Source });  //CompileAssemblyFromSource函数方法：从包含源代码的字符串的指定数组，使用指定的编译器设置编译程序集
            }
            catch (Exception exception1)    //Exception类 表示在应用程序执行过程中发生的错误的类
            {
                exception = exception1;
                m_ErrorText = "编译错误：可能未正确添加引用集";
                return null;
            }
            Compiled = true;    //编译结果信号先至true

            int errorNum = 0;
            StringBuilder sb1 = new StringBuilder();    //StringBuilder类 表示可变字符字符串。此类不能被继承
            for (int i = 0; i < results.Errors.Count; i++)  //遍历错误结果
            {
                if (!results.Errors[i].IsWarning)
                {
                    Compiled = false;   //编译结果信号至false

                    errorNum++;
                    sb1.Append($"[{i}]:  {results.Errors[i].ErrorText}");   //Errors 获取编译器错误和警告的集合；返回结果:System.CodeDom.Compiler.CompilerErrorCollection，指示由编译产生的错误和警告（如果有）。
                    sb1.AppendLine();   //将错误信息遍历写入可变字符字符串sb1
                }
            }
            if (errorNum > 0)
            {
                m_ErrorText = $"编译错误:\r\n" + sb1.ToString();
            }

            if (Compiled)   //判断编译信号是否成功
            {
                return File.ReadAllBytes(results.CompiledAssembly.Location);   //编译成功则将表达式内容写入字节数组
            }

            return null;
        }

        public List<object> Run(byte[] assemblyData)//传入ExpressionAssembly，执行编译传入的表达式程序集
        {            
            try
            {
                object _objectClass = null;
                Assembly assembly = Assembly.Load(assemblyData);
                _objectClass = assembly.CreateInstance("ExpressionScriptDataEx");
                if (_objectClass == null)
                {
                    return null;
                }

                object[] _params = { ProjectID };

                return (List<object>)_objectClass.GetType().InvokeMember("Process", BindingFlags.InvokeMethod, null, _objectClass, _params);
            }
            catch (Exception /*e*/)
            {
                return null;
            }
        }

        public static string s_RawScript = @"
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Text;
        using System.Threading.Tasks;
        using Heart.Outward;
        
        [Serializable]
        public class ExpressionScriptDataEx:ExpressionScriptData
        {
            public List<object> Process(int projectID)
            {
                ProjectID = projectID;
                List<object> VarObjectDict = new List<object>();
                #ExpressionRegion#
                return VarObjectDict;
            }
        }
        ";

        public static string s_ExpressionMask = "VarObjectDict.Add((typeStr)(expressionStr));";

    }

    [Serializable]
    public class ExpressionScriptData
    {
        public int ProjectID = -1;
        //下列的四个函数都可在编辑表达式中被使用，以获取对应的数据
        public int GetInt(string linkName)//从系统变量中获取拿到int值
        {
            string[] strArray = linkName.Split('.');
            Project prj = Solution.Instance.GetProjectById(ProjectID);
            return (int)prj.GetVariableDataByVarName(strArray[0], strArray[1]);
        }

        public double GetDouble(string linkName)
        {
            string[] strArray = linkName.Split('.');
            Project prj = Solution.Instance.GetProjectById(ProjectID);
            return (double)prj.GetVariableDataByVarName(strArray[0], strArray[1]);
        }

        public string GetString(string linkName)
        {
            string[] strArray = linkName.Split('.');
            Project prj = Solution.Instance.GetProjectById(ProjectID);
            return (string)prj.GetVariableDataByVarName(strArray[0], strArray[1]);
        }

        public bool GetBool(string linkName)
        {
            string[] strArray = linkName.Split('.');
            Project prj = Solution.Instance.GetProjectById(ProjectID);
            return (bool)prj.GetVariableDataByVarName(strArray[0], strArray[1]);
        }

    }

}

    

