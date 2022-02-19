using Heart.Inward;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//ModuleTreeExt用于工具拖拽放置区的代码操作

namespace Heart.Outward
{
    
    /// <summary>
    /// ModuleTreeExt.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleTreeExt : UserControl
    {
        static public Dictionary<string, ImageSource> PluginImageSourceDict { get; set; } = new Dictionary<string, ImageSource>();
        
        public Dictionary<string, bool> ExpandStatusDict = new Dictionary<string, bool>();
        public Dictionary<string, int> PluginNumberDict = new Dictionary<string, int>();    //插件字典，用于记录每一个插件ModuleNumber信息，实现插件计数和命名的功能

        public List<ModuleEntry> EntryList = new List<ModuleEntry>();//拖拽过来到流程栏的EntryList

        public List<ModuleTreeExtNode> NodeList = new List<ModuleTreeExtNode>();
        public List<ModuleTreeExtNode> TreeSourceList { get; set; } = new List<ModuleTreeExtNode>();

        private Cursor m_DragCursor;//拖拽时候的光标
        private bool m_DragMoveFlag;//移动标志
        private double m_MousePressY;//鼠标点下时的y坐标
        private double m_MousePressX;//鼠标点下时的X坐标

        List<KeyValuePair<int,bool>> SelectedIndexList = new List<KeyValuePair<int, bool>>();
        public ModuleTreeExtNode ShiftSelectedNode { get; set; }
        public ModuleTreeExtNode SelectedNode { get; set; }
        public ModuleTreeExtNode RelativeNode { get; set; }
        public int ProjectID { get; set; }

        public ContextMenu m_Menu = new ContextMenu();
        public MenuItem disable_Item = new MenuItem();
        public MenuItem delete_Item = new MenuItem();   //1022右键菜单--删除
        public ModuleTreeExt()
        {
            InitializeComponent();

            disable_Item.Header = "禁用";
            disable_Item.Click += new RoutedEventHandler((s, ev) => DisableItem(s, ev));
            delete_Item.Header = "删除";
            delete_Item.Click += new RoutedEventHandler((s, ev) => DeleteItem(s, ev));  //绑定路由事件
            m_Menu.Items.Add(disable_Item);
            m_Menu.Items.Add(delete_Item);

            _moduleTree.ContextMenu = m_Menu;
        }

        private void TreeView_Drop(object sender, DragEventArgs e)
        {           
            if (RelativeNode != null)// 恢复之前的下划线
            {
                RelativeNode.OverSelected = false;
                RelativeNode.DragOverHeight = 1;
            }

            if (e.AllowedEffects == DragDropEffects.Copy)//表示从工具栏拖动 需要创建新的模块
            {
                string defination = e.Data.GetData("Text").ToString();

                string[] definationArray = defination.Split('#');

                string pluginName = definationArray[0];
                List<string> moduleFamily = new List<string>(definationArray[1].Split('.'));

                if (RelativeNode != null && EntryList.Count != 0)
                {
                    AddModule(RelativeNode.EntryName, pluginName, moduleFamily);
                }
                else if (EntryList.Count == 0)
                {
                    AddModule("", pluginName, moduleFamily);
                }
                else
                {
                    AddModule(EntryList.Last().EntryName, pluginName, moduleFamily);
                }

                UpdateTree();
            }
            else if (e.AllowedEffects == DragDropEffects.Move)
            {
                if (SelectedIndexList.Count > 1)
                {
                    if (SelectedIndexList.Last().Value == false)
                    {
                        for (int j = SelectedIndexList.First().Key + 1; j <= SelectedIndexList.Last().Key; ++j)
                        {
                            NodeList[j].InSelected = false;
                        }
                    }

                }
                
                if (e.Data.GetData("Text") != null && RelativeNode != null)
                {
                    string entryName = e.Data.GetData("Text").ToString();

                    if (entryName != RelativeNode.EntryName)//自己不能移动到自己下面
                    {
                        ChangeModulePosition();
                        UpdateTree();
                    }
                }
            }

            if (RelativeNode != null)
            {
                RelativeNode = null;
            }

        }

        private void TreeView_DragOver(object sender, DragEventArgs e)
        {
            //获取鼠标位置的TreeViewItem 然后选中
            Point pt = e.GetPosition(_moduleTree);
            HitTestResult result = VisualTreeHelper.HitTest(_moduleTree, pt);
            if (result == null) return;
            TreeViewItem selectedItem = WPFElementTool.FindVisualParent<TreeViewItem>(result.VisualHit);

            if (selectedItem != null)
            {
                ModuleTreeExtNode node = selectedItem.DataContext as ModuleTreeExtNode;
                node.OverSelected = true;

                if (RelativeNode != null)
                {
                    if (RelativeNode.EntryName != node.EntryName)//名称不一样说明更换了module  恢复之前的下划线
                    {
                        RelativeNode.OverSelected = false;
                        RelativeNode.DragOverHeight = 1;
                    }
                }
                RelativeNode = node;
                RelativeNode.DragOverHeight = 5;

            }

            //获取treeview本身的 ScrollViewer
            TreeViewAutomationPeer lvap = new TreeViewAutomationPeer(_moduleTree);
            ScrollViewerAutomationPeer svap = lvap.GetPattern(PatternInterface.Scroll) as ScrollViewerAutomationPeer;
            ScrollViewer scroll = svap.Owner as ScrollViewer;

            pt = e.GetPosition(_moduleTree);

            if (_moduleTree.ActualHeight - pt.Y <= 50)
            {
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset + 10);
            }
            if (Math.Abs(pt.Y) <= 50)
            {
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset - 10);
            }

        }

        private void TreeView_DragLeave(object sender, DragEventArgs e)
        {
            if (RelativeNode != null)
            {
                RelativeNode.DragOverHeight = 1; // 恢复之前的下划线
            }
        }

        private void TreeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_DragMoveFlag == true)
            {
                Point pt = e.GetPosition(_moduleTree);
                if (Math.Abs(pt.Y - m_MousePressY) > 10 || Math.Abs(pt.X - m_MousePressX) > 10)//在y方向差异10像素 才开始拖动
                {
                    string showText = "";
                    int width = 0;                    
                    if (SelectedIndexList.Count > 1)
                    {
                        if (SelectedIndexList.Last().Value == false)
                        {
                            for (int j = SelectedIndexList.First().Key + 1; j <= SelectedIndexList.Last().Key; ++j)
                            {
                                NodeList[j].InSelected = true;
                            }
                        }
                        showText = $"[{EntryList[SelectedIndexList.First().Key].EntryName}] ~ [{EntryList[SelectedIndexList.Last().Key].EntryName}]";
                        width = 400;
                    }
                    else
                    {
                        width = 200;
                        showText = SelectedNode.EntryName;
                    }
                    m_DragCursor = WPFCursorTool.CreateCursor(width, 30, 13, ImageTool.ImageSourceToBitmap(SelectedNode.IconImage), 32, showText);
                    m_DragMoveFlag = false;
                    DragDrop.DoDragDrop(_moduleTree, $"{EntryList[SelectedIndexList.First().Key].EntryName}", DragDropEffects.Move);
                }
            }

        }

        private void TreeView_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
            Mouse.SetCursor(m_DragCursor);
            e.Handled = true;
        }

        private void TreeView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(_moduleTree);
            HitTestResult result = VisualTreeHelper.HitTest(_moduleTree, pt);
            if (result == null)
            {
                return;
            }

            TreeViewItem selectedItem = WPFElementTool.FindVisualParent<TreeViewItem>(result.VisualHit);
            if (selectedItem == null)
            {
                return;
            }

            ModuleTreeExtNode node = selectedItem.DataContext as ModuleTreeExtNode;
            if (Keyboard.Modifiers != ModifierKeys.Shift)
            {
                foreach (KeyValuePair<int, bool> kv in SelectedIndexList)
                {
                    if ((kv.Value == true) && (NodeList[kv.Key].EntryName == node.EntryName))
                    {
                        SelectedNode = node;
                        SingleSelect();
                        break;
                    }
                }
            }

        }

        private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_moduleTree.Items.Count == 0)
            {
                _moduleTree.Focus();//在没有任何元素的时候 需要这几句来获得焦点
                return;
            }

            //获取鼠标位置的TreeViewItem 然后选中
            Point pt = e.GetPosition(_moduleTree);
            HitTestResult result = VisualTreeHelper.HitTest(_moduleTree, pt);
            if (result == null)
            {
                return;
            } 

            TreeViewItem selectedItem = WPFElementTool.FindVisualParent<TreeViewItem>(result.VisualHit);
            if (selectedItem == null)
            {
                ClearSelected();
                SelectedNode = null;
                ShiftSelectedNode = null;
                return;
            }

            bool ignore = false;
            ModuleTreeExtNode node = selectedItem.DataContext as ModuleTreeExtNode;
            if (Keyboard.Modifiers != ModifierKeys.Shift) {
                foreach (KeyValuePair<int, bool> kv in SelectedIndexList)
                {
                    if ((kv.Value == true) && (NodeList[kv.Key].EntryName == node.EntryName))
                    {
                        ignore = true;
                        break;
                    }
                }
            }
            
            if (ignore == false)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift && SelectedNode != null)
                {
                    ShiftSelectedNode = node;
                    MultipleSelect();
                    e.Handled = true;
                }
                else
                {
                    SelectedNode = node;
                    SingleSelect();
                }
            }
            
            if (_moduleTree.ActualWidth - pt.X > (_moduleTree.ActualWidth * 0.05))
            {
                if ((ShiftSelectedNode != null) || (SelectedIndexList.First().Value == true))
                {
                    m_MousePressY = pt.Y;
                    m_MousePressX = pt.X;
                    m_DragMoveFlag = true;
                }
            }

        }

        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //获取鼠标位置的TreeViewItem 然后选中
            Point pt = e.GetPosition(_moduleTree);
            HitTestResult result = VisualTreeHelper.HitTest(_moduleTree, pt);
            if (result == null) return;
            TreeViewItem selectedItem = WPFElementTool.FindVisualParent<TreeViewItem>(result.VisualHit);

            if (selectedItem != null)
            {
                ModuleTreeExtNode node = selectedItem.DataContext as ModuleTreeExtNode;
                if ((SelectedNode == null) ||(SelectedNode.EntryName != node.EntryName)||(ShiftSelectedNode != null))
                {
                    SelectedNode = node;
                    SingleSelect();
                }

                if (SelectedIndexList.First().Value == true)
                {
                    if (node.IsUse)
                    {
                        disable_Item.Header = "禁用";
                    }
                    else
                    {
                        disable_Item.Header = "启用";
                    }
                    m_Menu.IsOpen = true;
                }
                delete_Item.Visibility = Visibility.Visible; //先显示一下
                if (node.EntryName.Contains("结束"))
                {
                    delete_Item.Visibility = Visibility.Hidden; //这样隐藏
                }
            }
        }

        private void TreeView_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            m_DragMoveFlag = false;
        }

        private void Tree_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(_moduleTree);
            HitTestResult result = VisualTreeHelper.HitTest(_moduleTree, pt);
            if (result == null) return;
            TreeViewItem selectedItem = WPFElementTool.FindVisualParent<TreeViewItem>(result.VisualHit);
            if (selectedItem == null) return;

            ModuleTreeExtNode node = selectedItem.DataContext as ModuleTreeExtNode;

            Project prj = Solution.Instance.GetProjectById(ProjectID);
            
            int selectedIndex = EntryList.FindIndex(c => c.EntryName == node.EntryName);
            ModuleObjBase moduleObjBase = prj.GetModuleByName(EntryList[selectedIndex].Info.ModuleName);

            if (moduleObjBase.Info.ModuleFamily.Count > 1)
            {
                if (EntryList[selectedIndex].RawEntryName == moduleObjBase.Info.ModuleFamily.Last())
                {
                    int entryIndex = moduleObjBase.AddEntry();
                    if (entryIndex > 0)
                    {
                        string refEntryName = moduleObjBase.Info.ModuleFamily[entryIndex + 1] + moduleObjBase.Info.ModuleNumber.ToString();
                        int relIndex = EntryList.FindIndex(c => c.EntryName == refEntryName) + 1;
                        AddEntry(relIndex-1, moduleObjBase.Info, entryIndex);
                        UpdateTree();
                    }
                    return;
                }
            }
                          
            PluginContent pluginContent = Plugin.s_PluginContentDict[moduleObjBase.Info.PluginName];
            if (pluginContent.ModuleFormType != null)
            {
                ModuleFormBase moduleFormBase = (ModuleFormBase)pluginContent.ModuleFormType.Assembly.CreateInstance(pluginContent.ModuleFormType.FullName);
                moduleFormBase.ModuleObjBase = moduleObjBase;
                moduleFormBase.CanvasCount = Project.s_SplitHWindowFitExt.GetDisplayNumber();
                moduleFormBase.ShowDialog();
            }
        }

        /// <summary>
        /// 右键菜单--禁用--核心函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisableItem(object sender, EventArgs e)
        {
            bool uValue;
            if (SelectedNode.IsUse)
            {
                uValue = false;
            }
            else
            {
                uValue = true;
            }

            foreach (KeyValuePair<int, bool> kv in SelectedIndexList)
            {
                EntryList[kv.Key].Info.IsUse = uValue;
                NodeList[kv.Key].IsUse = uValue;
                NodeList[kv.Key].CostTime = null;
                if (uValue)
                {
                    NodeList[kv.Key].StateImage = null;              
                }
                else
                {
                    NodeList[kv.Key].StateImage = Project.DisableIcon;
                }               
            }
        }

        /// <summary>
        /// 右键菜单--删除--核心函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteItem(object sender, EventArgs e)  //删除模块
        {
            if (SelectedNode.IsUse)
            {
                Project prj = Solution.Instance.GetProjectById(ProjectID);                                  //通过int ProjectID 项目ID来获取对应的流程栏
                int selectIndex = EntryList.FindIndex(c => c.EntryName == SelectedNode.EntryName);          //通过当前插件的名字在EntryList内获取插件的索引
                ModuleObjBase moduleObjBase = prj.GetModuleByName(EntryList[selectIndex].Info.ModuleName);  //获取选中的item属于什么模块
                if (moduleObjBase == null) { return; }                                                      //判断如果插件为空则直接跳出                                          

                if (moduleObjBase.Info.ModuleName.Contains("条件分支") || moduleObjBase.Info.ModuleName.Contains("循环工具"))  //如果删除的是逻辑工具"如果"，就要删除整个如果下的东西包括其对应的结束
                {             
                    int indexend = 0;
                    int IsRightEnd = 1;
                    for (int i = selectIndex + 1; IsRightEnd != 0; i++)  //1008这里要使用!= 才能保证循环等待IsRightEnd==0才停止
                    {
                        if (EntryList[i].EntryName.Contains("结束")) IsRightEnd--;
                        if (EntryList[i].EntryName.Contains("如果") || EntryList[i].EntryName.Contains("开始循环")) IsRightEnd++;
                        indexend = i;   //得到当前如果模块对应的结束
                    }
                    int finalDelectNum = 0;     //记录着当前的进入了几次的结束，方便在最后把这些结束的EntryList信息全部遍历删除
                    for (int i = selectIndex; i <= indexend; i++)
                    {
                        if (EntryList[selectIndex].EntryName.Contains("结束"))    //而且这个判断的模块，需要放在prj.GetModuleByName(EntryList[i].Info.ModuleName)之前，防止拿不到object //这里必须换为EntryName，用EntryList[selectIndex].Info.ModuleName就不包含结束了，只有条件分支
                        {
                            finalDelectNum++;
                            continue;        //1023如果当前的模块是结束模块，就要跳出此次循环体，继续下一次的循环判断
                        }
                        ModuleObjBase _moduleObjBase = prj.GetModuleByName(EntryList[selectIndex].Info.ModuleName);   //获取选中的item属于什么模块                     
                        prj.DeleteVariableData(_moduleObjBase.Info.ModuleName);                             //清空对应模块的输出数据
                        prj.DeleteVariableInfo(_moduleObjBase.Info.ModuleName);                             //删除当前模块变量信息
                        prj.m_ModuleObjList.Remove(_moduleObjBase);                                         //删除对应的模块参数 //m_ModuleObjList 当前工程对应的模块列表List
                        EntryList.RemoveAt(selectIndex);                                                    //拖拽过来到流程栏的EntryList
                        //tring name = moduleObjBase.Info.ModuleName.Remove(moduleObjBase.Info.ModuleName.Length - 1, 1);       //原邓的方法，弃用。直接去掉名字的最后一项，如果插件后有两位数就不行
                        //string name = Regex.Replace(_moduleObjBase.Info.ModuleName, "[0 - 9]", "", RegexOptions.IgnoreCase);  //获取这个要指定删除的变量的纯名字，用代替的方式去掉名字的所有数字
                        string name = _moduleObjBase.Info.PluginName;                                                //其实直接拿出PluginName即可拿到模块纯名称 
                        PluginNumberDict[name] -= 1;                                                                 //遍历删除的过程中，把每个插件的ModuleNumber计数信息减1
                        if(name.Contains("条件分支") || name.Contains("循环工具"))
                        {
                            //indexend--;     //OS体系内，逻辑工具拖出来看起来两个，其实在m_ModuleObjList内部只有一个变量。所以在删除的过程中，如果遇到了逻辑工具，删除的数量就该重复-1   //1023因为在上面加了会判断当前模块是否是结束模块，所以这里就不用indexend来减少循环次数了                      
                        }
                    }
                    for (int i = 0; i < finalDelectNum; i++)    //最后这一层for循环，为了是把前面记录到的结束的数量，再删掉。因为EntryList在前面已经遍历删除了，所以在selectIndex这个位置上，剩下的都是"结束"类的插件，直接删掉即可
                    {
                        EntryList.RemoveAt(selectIndex);    //这里注意，EntryList为流程栏树的内容，故逻辑工具其实还是存在两个模块的，所以这里要删除多一次
                    }
                }
                else   //如果删除的是普通插件，就直接删除即可
                {
                    prj.DeleteVariableData(moduleObjBase.Info.ModuleName);                             //清空对应模块的输出数据
                    prj.DeleteVariableInfo(moduleObjBase.Info.ModuleName);                             //删除当前模块变量信息
                    prj.m_ModuleObjList.Remove(moduleObjBase);                                         //删除对应的模块参数 //m_ModuleObjList 当前工程对应的模块列表List
                    EntryList.RemoveAt(selectIndex);                                                   //拖拽过来到流程栏的EntryList
                    //string name = Regex.Replace(moduleObjBase.Info.ModuleName, "[0 - 9]", "", RegexOptions.IgnoreCase);    //获取这个要指定删除的变量的纯名字，用代替的方式去掉名字的所有数字
                    string name = moduleObjBase.Info.PluginName;                                                 //其实直接拿出PluginName即可拿到模块纯名称 
                    PluginNumberDict[name] -= 1;                                                                 //遍历删除的过程中，把每个插件的ModuleNumber计数信息减1
                }
                UpdateTree();
            }
        }

        private void ChangeModulePosition()
        {
            string relativeEntryName = RelativeNode.EntryName;
     
            List<string> entryNameList = EntryList.Select(c => c.EntryName).ToList();
            int firstIndex = SelectedIndexList.First().Key;
            int lastIndex;

            if (SelectedIndexList.Count == 1)
            {
                lastIndex = firstIndex;
            }
            else
            {
                lastIndex = SelectedIndexList.Last().Key;
            }
                
            int relIndex = entryNameList.IndexOf(relativeEntryName);
            if((relIndex>= firstIndex)&&(relIndex <= lastIndex))
            {
                return;
            }

            List<string> tempList = entryNameList.GetRange(firstIndex, lastIndex + 1 - firstIndex);
            entryNameList.RemoveRange(firstIndex, lastIndex+1- firstIndex);
            relIndex = entryNameList.IndexOf(relativeEntryName);
            entryNameList.InsertRange(relIndex+1, tempList);

            List<ModuleEntry> tempEntryList = new List<ModuleEntry>();

            foreach (string entryName in entryNameList)
            {
                tempEntryList.Add(EntryList.FirstOrDefault(c => c.EntryName == entryName));              
            }

            EntryList = tempEntryList;
        }

        private void SingleSelect()
        {
            ModuleEntry entry = EntryList.Find(c => c.EntryName == SelectedNode.EntryName);

            ShiftSelectedNode = null;

            ClearSelected();

            if (entry.Info.ModuleFamily.Count > 1)
            {
                List<int> tIndexList = new List<int>();
                int tIndexMin = EntryList.FindIndex(c => c.EntryName == entry.Info.ModuleFamily.First() + entry.Info.ModuleNumber.ToString());
                int tIndexMax = EntryList.FindIndex(c => c.EntryName == entry.Info.ModuleFamily.Last() + entry.Info.ModuleNumber.ToString());
                int tIndex = EntryList.FindIndex(c => c.EntryName == entry.EntryName);

                for (int j = tIndexMin; j <= tIndexMax; ++j)
                {
                    if (tIndex == j)
                    {
                        SelectedIndexList.Add(new KeyValuePair<int, bool>(j, true));
                        NodeList[j].InSelected = true;
                    }
                    else
                    {
                        SelectedIndexList.Add(new KeyValuePair<int, bool>(j, false));
                    }                 
                }
            }
            else
            {
                int index = EntryList.FindIndex(c => c.EntryName == SelectedNode.EntryName);
                SelectedIndexList.Add(new KeyValuePair<int, bool>(index, true));
                NodeList[index].InSelected = true;
            }

            Project prj = Solution.Instance.GetProjectById(ProjectID);
            prj.DisplayOutputTitle(entry.Info.ModuleName);
            prj.DisplayOutputData(entry.Info.ModuleName);
        }

        private void MultipleSelect()
        {        
            List<int> sIndexList = new List<int>();
            sIndexList.Add(EntryList.FindIndex(c => c.EntryName == SelectedNode.EntryName));
            sIndexList.Add(EntryList.FindIndex(c => c.EntryName == ShiftSelectedNode.EntryName));

            List<int> tIndexList = new List<int>();
            for (int i = sIndexList.Min(); i <= sIndexList.Max(); ++i)
            {
                ModuleEntry entry = EntryList[i];
                tIndexList.Add(EntryList.FindIndex(c => c.EntryName == entry.Info.ModuleFamily.First() + entry.Info.ModuleNumber.ToString()));
                tIndexList.Add(EntryList.FindIndex(c => c.EntryName == entry.Info.ModuleFamily.Last() + entry.Info.ModuleNumber.ToString()));
            }
          
            int tIndexMin = tIndexList.Min();
            int tIndexMax = tIndexList.Max();

            ClearSelected();

            for (int j = tIndexMin; j <= tIndexMax; ++j) {
                NodeList[j].InSelected = true;
                SelectedIndexList.Add(new KeyValuePair<int, bool>(j, true));
            }

        }

        public void ClearSelected()
        {
            foreach (KeyValuePair<int, bool> kv in SelectedIndexList)
            {
                NodeList[kv.Key].InSelected = false;
            }

            SelectedIndexList.Clear();
        }

        /// <summary>
        /// 添加一个模块
        /// </summary>
        private void AddModule(string relativeEntryName, string pluginName, List<string> moduleFamily)
        {                   
            if (!PluginNumberDict.ContainsKey(pluginName))
            {
                PluginNumberDict[pluginName] = 0;
            }
            else
            {
                //PluginNumberDict[pluginName] += 1;  //1023以前这里是强制+1的，不合理。要改为从0开始遍历上去，有空就填充
                //获取新不重复的id  如果已经存在  1,2,4   那么久获得的id 是 3
                bool flag = true;
                int id = 0;
                do
                {
                    flag = true;
                    foreach (ModuleEntry item in EntryList)
                    {
                        if (item.Info.PluginName == pluginName && item.Info.ModuleNumber == id)     //遍历循环EntryList来判断当前的模块是否是要将要加载的插件，且模块的序号是否对应有以前的编号
                        {
                            id++;
                            flag = false;
                            break;
                        }
                    }
                    if (flag == true)
                    {
                        break;
                    }
                } while (true);

                PluginNumberDict[pluginName] = id;
            }
            
            int relIndex = 0;
            if (String.IsNullOrEmpty(relativeEntryName))
            {
                relIndex = EntryList.Count;
            }
            else
            {
                relIndex = EntryList.FindIndex(c => c.EntryName == relativeEntryName) + 1;
            }

            ModuleInfo info = new ModuleInfo();
            info.PluginName = pluginName;
            info.ModuleNumber = PluginNumberDict[info.PluginName];
            info.ModuleName = info.PluginName + info.ModuleNumber.ToString();
            info.ModuleFamily = moduleFamily;
            info.ProjectID = ProjectID;

            Project prj = Solution.Instance.GetProjectById(ProjectID);
            PluginContent pluginContent = Plugin.s_PluginContentDict[pluginName];
            ModuleObjBase moduleObj = (ModuleObjBase)pluginContent.ModuleObjType.Assembly.CreateInstance(pluginContent.ModuleObjType.FullName);
            moduleObj.Info = info;
            moduleObj.UpdateOutputVariable();
            prj.AddModuleObj(moduleObj);
            
            for (int i = 0; i < moduleFamily.Count; ++i)
            {
                AddEntry(relIndex, info, i);
                relIndex++;
            }                             
        }

        private void AddEntry(int relativeEntryIndex, ModuleInfo info, int entryIndex)
        {
            ModuleEntry entry = new ModuleEntry();
            entry.RawEntryName = info.ModuleFamily[entryIndex];
            entry.EntryName = info.ModuleFamily[entryIndex] + info.ModuleNumber.ToString();
            entry.Info = info;
            EntryList.Insert(relativeEntryIndex, entry);
        }

        /// <summary>
        /// 获取结构树的展开状态
        /// </summary>
        /// <param name="nodes"></param>
        private void GetTreeExpandStatus(ItemsControl control)
        {
            if (control != null)
            {
                foreach (object item in control.Items)
                {
                    TreeViewItem treeItem = control.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                    if (treeItem != null && treeItem.HasItems)
                    {
                        ModuleTreeExtNode moduleNode = treeItem.DataContext as ModuleTreeExtNode;
                        ExpandStatusDict[moduleNode.EntryName] = treeItem.IsExpanded;

                        GetTreeExpandStatus(treeItem as ItemsControl);
                    }
                }
            }
        }

        public ModuleTreeExtNode GetNodeByEntryName(string entryName)
        {
            return NodeList.FirstOrDefault(c => c.EntryName == entryName);
        }

        public void ClearAllNodeCostTime()
        {
            foreach (ModuleTreeExtNode item in NodeList)
            {
                item.CostTime = "";
            }
        }

        public void ClearAllNodeStateImage()
        {
            foreach (ModuleTreeExtNode item in NodeList)
            {
                if (item.IsUse)
                {
                    item.StateImage = null;
                }             
            }
        }

        public void UpdateTree()
        {
            Stack<ModuleTreeExtNode> parentItemStack = new Stack<ModuleTreeExtNode>();
            List<ModuleEntry> entryList = EntryList;

            string SelectedNodeName = null;
            string ShiftSelectedNodeName = null;

            if (SelectedNode != null)
            {
                SelectedNodeName = SelectedNode.EntryName;
            }

            if (ShiftSelectedNode != null)
            {
                ShiftSelectedNodeName = ShiftSelectedNode.EntryName;
            }

            SelectedNode = null;
            ShiftSelectedNode = null;
            SelectedIndexList.Clear();

            if(NodeList == null)
            {
                NodeList = new List<ModuleTreeExtNode>();
            }
            NodeList.Clear();   //NodeList在刷新树时会在这里被清空，表示其实是不用被序列化的

            ExpandStatusDict.Clear();
            GetTreeExpandStatus(_moduleTree);

            if (TreeSourceList == null)
            {
                TreeSourceList = new List<ModuleTreeExtNode>();
            }
            TreeSourceList.Clear();

            Project prj = Solution.Instance.GetProjectById(ProjectID);
            prj.ClearModuleEntryList();

            for (int i = 0; i < entryList.Count; i++)
            {
                ModuleEntry entry = entryList[i];

                prj.AddModuleEntry(entry.EntryName, entry.Info.ModuleName);

                ModuleTreeExtNode nodeItem = new ModuleTreeExtNode(entry.EntryName);
                nodeItem.IconImage = PluginImageSourceDict[entry.Info.PluginName];      //这里会把插件的图标信息进行传递，加载插件对应的图标
                nodeItem.IsExpanded = ExpandStatusDict.ContainsKey(entry.EntryName) ? ExpandStatusDict[entry.EntryName] : true;
                NodeList.Add(nodeItem);

                if (i == 0) nodeItem.IsFirstNode = true;

                int index = entry.Info.ModuleFamily.IndexOf(entry.RawEntryName);
                int count = entry.Info.ModuleFamily.Count;

                if ((count>1) && (index>0))
                {
                    parentItemStack.Pop();
                }

                if (parentItemStack.Count > 0)
                {
                    nodeItem.Hierarchy = parentItemStack.Count;
                    ModuleTreeExtNode parentItem = parentItemStack.Peek();

                    nodeItem.ParentModuleNode = parentItem;
                    parentItem.Children.Add(nodeItem);
                }
                else
                {
                    nodeItem.Hierarchy = 0;

                    nodeItem.ParentModuleNode = null;
                    TreeSourceList.Add(nodeItem);
                }

                if ((count > 1) && (index < (count-1)))
                {
                    parentItemStack.Push(nodeItem);
                }

                //最后一个node如果层级大于0 则需要补划最后一条横线
                if (i == entryList.Count - 1 && nodeItem.Hierarchy > 0)
                {
                    nodeItem.LastNodeMargin = $"{nodeItem.Hierarchy * -14},0,0,0";
                }
            }
            _moduleTree.ItemsSource = TreeSourceList.ToList();

            //选中之前选中的节点  
            SelectedNode = NodeList.FirstOrDefault(c => c.EntryName == SelectedNodeName);
            ShiftSelectedNode = NodeList.FirstOrDefault(c => c.EntryName == ShiftSelectedNodeName);

            if (SelectedNode != null)
            {
                if (ShiftSelectedNode != null)
                {
                    MultipleSelect();
                }
                else
                {
                    SingleSelect();
                }
            }
            else
            {
                ShiftSelectedNode = null;
            }
            
        }

        
    }

    [Serializable]
    public class ModuleTreeExtNode : DependencyObject
    {
        public ModuleTreeExtNode(string entryName)
        {
            Children = new List<ModuleTreeExtNode>();
            EntryName = entryName;
        }
        public string EntryName { get; set; }
        public ImageSource IconImage { get; set; }   
        public int Hierarchy { get; set; } = 0;// 层级  0是第一层 孙子层级是2  
        public ModuleTreeExtNode ParentModuleNode { get; set; }
        public List<ModuleTreeExtNode> Children { get; set; }
        public bool IsExpanded { get; set; } = true;
        public string ModuleForeground { get; set; } = "#000000";
        public bool IsHideExpanded { get; set; } = false;

        //DragOver的时候 下划线加粗
        public static readonly DependencyProperty DragOverHeightProperty =
        DependencyProperty.Register("DragOverHeight", typeof(int), typeof(ModuleTreeExtNode),
        new PropertyMetadata((int)1));

        public int DragOverHeight
        {
            get { return (int)GetValue(DragOverHeightProperty); }
            set
            {
                if (value != DragOverHeight)
                {
                    SetValue(DragOverHeightProperty, value);
                }
            }
        }

        //当最后一个元素是子集的时候,下划线要往左移动
        public static readonly DependencyProperty LastNodeMarginProperty =
        DependencyProperty.Register("LastNodeMargin", typeof(string), typeof(ModuleTreeExtNode),
        new PropertyMetadata((string)"0,0,0,0"));

        public string LastNodeMargin
        {
            get { return (string)GetValue(LastNodeMarginProperty); }
            set
            { SetValue(LastNodeMarginProperty, value); }
        }

        //模块运行状态
        public static readonly DependencyProperty StateImageProperty =
        DependencyProperty.Register("StateImage", typeof(ImageSource), typeof(ModuleTreeExtNode),
        new PropertyMetadata(null));

        public ImageSource StateImage
        {
            get { return (ImageSource)GetValue(StateImageProperty); }
            set
            {

                SetValue(StateImageProperty, value);
            }
        }

        //模块运行 时间
        public static readonly DependencyProperty CostTimeProperty =
        DependencyProperty.Register("CostTime", typeof(string), typeof(ModuleTreeExtNode),
        new PropertyMetadata(""));

        public string CostTime
        {
            get { return (string)GetValue(CostTimeProperty); }
            set
            {

                SetValue(CostTimeProperty, value);
            }
        }

        //是否正在运行
        public static readonly DependencyProperty IsRunningProperty =
        DependencyProperty.Register("IsRunning", typeof(bool), typeof(ModuleTreeExtNode),
        new PropertyMetadata(false));

        public bool IsRunning
        {
            get { return (bool)GetValue(IsRunningProperty); }
            set
            { SetValue(IsRunningProperty, value); }
        }


        //是否第一个元素  要补画上划线
        public static readonly DependencyProperty IsFirstNodeProperty =
        DependencyProperty.Register("IsFirstNode", typeof(bool), typeof(ModuleTreeExtNode),
        new PropertyMetadata(false));

        public bool IsFirstNode
        {
            get { return (bool)GetValue(IsFirstNodeProperty); }
            set
            { SetValue(IsFirstNodeProperty, value); }

        }

        //是否被选中
        public static readonly DependencyProperty InSelectedProperty =
        DependencyProperty.Register("InSelected", typeof(bool), typeof(ModuleTreeExtNode),
        new PropertyMetadata(false));

        public bool InSelected
        {
            get { return (bool)GetValue(InSelectedProperty); }
            set
            { SetValue(InSelectedProperty, value); }

        }

        public static readonly DependencyProperty OverSelectedProperty =
        DependencyProperty.Register("OverSelected", typeof(bool), typeof(ModuleTreeExtNode),
        new PropertyMetadata(false));

        public bool OverSelected
        {
            get { return (bool)GetValue(OverSelectedProperty); }
            set
            { SetValue(OverSelectedProperty, value); }

        }

        public static readonly DependencyProperty IsUseProperty =
        DependencyProperty.Register("IsUse", typeof(bool), typeof(ModuleTreeExtNode),
        new PropertyMetadata(true));

        public bool IsUse
        {
            get { return (bool)GetValue(IsUseProperty); }
            set
            { SetValue(IsUseProperty, value); }

        }

    }
}
