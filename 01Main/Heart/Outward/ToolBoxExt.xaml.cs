using Heart.Inward;
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
//ToolBoxExt为实现放置dll控件工具的控件，在主窗口放置与左侧。可加载指定目录下的dll，将这些控件放置在工具区

namespace Heart.Outward
{
    /// <summary>
    /// ToolBoxExt.xaml 的交互逻辑
    /// </summary>
    public partial class ToolBoxExt : UserControl
    {
        public ToolBoxExt()
        {
            InitializeComponent();
        }

        private Cursor m_DragCursor;
        private static ImageSource toolIcon { get; set; }
        public List<KeyValuePair<string, List<ToolBoxExtNode>>> ToolBoxList { get; set; } = new List<KeyValuePair<string, List<ToolBoxExtNode>>>();

        private void ToolBoxPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ListBox listbox = (ListBox)sender;

            //获取鼠标位置的TreeViewItem 然后选中
            Point pt = e.GetPosition(listbox);
            HitTestResult result = VisualTreeHelper.HitTest(listbox, pt);
            if (result == null) return;

            ListBoxItem selectedItem = WPFElementTool.FindVisualParent<ListBoxItem>(result.VisualHit);      //此处调用了WPFElementTool类，用于遍历dll内控件

            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;

                ToolBoxExtNode toolBoxExtNode = listbox.SelectedItem as ToolBoxExtNode;
                if (toolBoxExtNode != null)
                {
                    m_DragCursor = WPFCursorTool.CreateCursor(200, 30, 13, ImageTool.ImageSourceToBitmap(toolBoxExtNode.IconImage), 24, toolBoxExtNode.Name);   //此处调用了WPFCursorTool，用于光标事件；调用ImageTool，用于图像空间转换。
                    DragDrop.DoDragDrop(listbox, toolBoxExtNode.Defination, DragDropEffects.Copy);//增加模块是 copy的方法，因为鼠标在左侧工具栏
                    //DragDrop.DoDragDrop(listbox, "图像采集#图像采集" , DragDropEffects.Copy);//增加模块是 copy ，此处为强行绑定模块
                }
            }
        }

        private void ToolBoxGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
            Mouse.SetCursor(m_DragCursor);
            e.Handled = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_stackPanel.Children.Count > 0)
            {
                return;
            }

            toolIcon = new BitmapImage(new Uri("pack://application:,,,/" + "Heart" + ";Component/Icon/tool.png"));

            int Counter = 0;
            string Category = "";
            List<ToolBoxExtNode> NodeList = new List<ToolBoxExtNode>();
            ModuleTreeExt.PluginImageSourceDict.Clear();
            ToolBoxList.Clear();
            foreach (PluginInfo info in Plugin.s_PluginInfoList)
            {
                ModuleTreeExt.PluginImageSourceDict[info.Name] = info.IconImage;
                if (Counter != info.CategoryNumber)
                {
                    ToolBoxList.Add(new KeyValuePair<string, List<ToolBoxExtNode>>(Category, NodeList));
                    NodeList = new List<ToolBoxExtNode>();
                    Counter = info.CategoryNumber;
                }

                Category = info.Category;
                ToolBoxExtNode Node = new ToolBoxExtNode();
                Node.Name = info.Name;
                Node.Defination = info.Defination;
                Node.IconImage = info.IconImage;
                NodeList.Add(Node);
            }
            if (Category != "")
            {
                ToolBoxList.Add(new KeyValuePair<string, List<ToolBoxExtNode>>(Category, NodeList));
            }

            foreach (KeyValuePair < string, List<ToolBoxExtNode> > item in ToolBoxList)
            {
                Expander ep = new Expander();
                

                StackPanel sp= new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                Image img = new Image();
                img.Source = toolIcon;
                img.Width = 40;
                img.Height = 40;
                TextBlock text = new TextBlock();
                text.Text = item.Key;
                text.FontSize = 20;
                text.Padding = new Thickness(20,0,0,0);
                text.HorizontalAlignment = HorizontalAlignment.Stretch;
                text.VerticalAlignment = VerticalAlignment.Center;
                
                sp.Children.Add(img);
                sp.Children.Add(text);

                ep.Header = sp;

                ListBox lb = new ListBox();
                lb.PreviewMouseDown += ToolBoxPreviewMouseDown;
                lb.GiveFeedback += ToolBoxGiveFeedback;
                lb.ItemsSource = item.Value;
                ep.Content = lb;
                _stackPanel.Children.Add(ep);
            }         
        }
    }

    public class ToolBoxExtNode
    {
        public ImageSource IconImage { get; set; }
        public string Name { get; set; }
        public string Defination { get; set; }  //其他姿态选项
    }
}
