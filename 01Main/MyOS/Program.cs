using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Heart.Inward;

namespace MyOS
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
                {
            MyOS.App app = new MyOS.App();


            //设置临时目录为路径
            Heart.Inward.Solution.Init(); 
            Heart.Inward.Project.Init();
            Heart.Inward.Plugin.Init();

            app.InitializeComponent();
            //MainWindow windows = new MainWindow();
            //app.MainWindow = windows;
            app.Run();

        }
    }

}
