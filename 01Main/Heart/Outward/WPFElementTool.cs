using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
//WPFElementTool类自己独立，无需其他引用
//WPFElementTool类用于遍历加载dll内控件
//WPFElementTool类被ToolBoxExt.xaml.cs调用  ListBoxItem selectedItem = WPFElementTool.FindVisualParent<ListBoxItem>(result.VisualHit); 第43行

namespace Heart.Outward
{
    public class WPFElementTool
    {
        /// <summary>
        /// 在父类元素中查找子元素  
        /// this.findName 只能找到注册的name,  而对于模板里的name 无效,故需要使用该方法!magical20181115
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // 如果子控件不是需查找的控件类型
                T childType = child as T;
                if (childType == null)
                {
                    // 在下一级控件中递归查找
                    foundChild = FindChild<T>(child, childName);

                    // 找到控件就可以中断递归操作 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // 如果控件名称符合参数条件
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // 查找到了控件
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// 查找父控件的子控件集合
        /// </summary>
        /// <typeparam name="T">子控件类型</typeparam>
        /// <param name="obj">父控件</param>
        /// <param name="typename">子控件类型名</param>
        /// <returns></returns>
        public static List<T> GetChildObjects<T>(DependencyObject obj, Type typename) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);


                if (child is T && (((T)child).GetType() == typename))
                {
                    childList.Add((T)child);
                }
                childList.AddRange(GetChildObjects<T>(child, typename));
            }
            return childList;
        }


        /// <summary>
        /// TreeView节点的展开和收缩
        /// </summary>
        /// <param name="control">TreeView控件</param>
        /// <param name="expandNode">true:展开 false:收缩</param>
        public static void SetNodeExpandedState(ItemsControl control, bool expandNode)
        {
            try
            {
                if (control != null)
                {
                    foreach (object item in control.Items)
                    {
                        TreeViewItem treeItem = control.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                        if (treeItem != null && treeItem.HasItems)
                        {
                            treeItem.IsExpanded = expandNode;

                            if (treeItem.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                            {
                                treeItem.UpdateLayout();
                            }

                            SetNodeExpandedState(treeItem as ItemsControl, expandNode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T FindVisualParent<T>(DependencyObject obj) where T : class
        {
            while (obj != null)
            {
                if (obj is T)
                    return obj as T;

                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }

        //根据父控件对象查找指定类型的子控件
        public static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
    }
}
