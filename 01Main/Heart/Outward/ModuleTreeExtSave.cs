using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Heart.Inward;
//ModuleTreeExtSave是用于ModuleTreeExt : UserControl界面类的辅助序列化。类里面存放着与ModuleTreeExt相似的变量

namespace Heart.Outward
{
    [Serializable]
    public class ModuleTreeExtSave
    {
        static public Dictionary<string, ImageSource> PluginImageSourceDict { get; set; } = new Dictionary<string, ImageSource>();

        public Dictionary<string, bool> ExpandStatusDict = new Dictionary<string, bool>();
        public Dictionary<string, int> PluginNumberDict = new Dictionary<string, int>();    //插件字典，用于记录每一个插件ModuleNumber信息，实现插件计数和命名的功能

        public List<ModuleEntry> EntryList = new List<ModuleEntry>();//拖拽过来到流程栏的EntryList

        [field:NonSerialized]
        public List<ModuleTreeExtNode> NodeList = new List<ModuleTreeExtNode>();
        [field: NonSerialized]
        public List<ModuleTreeExtNode> TreeSourceList { get; set; } = new List<ModuleTreeExtNode>();

        //private Cursor m_DragCursor;//拖拽时候的光标
        private bool m_DragMoveFlag;//移动标志
        private double m_MousePressY;//鼠标点下时的y坐标
        private double m_MousePressX;//鼠标点下时的X坐标

        List<KeyValuePair<int, bool>> SelectedIndexList = new List<KeyValuePair<int, bool>>();
        [field: NonSerialized]
        public ModuleTreeExtNode ShiftSelectedNode { get; set; }
        [field: NonSerialized]
        public ModuleTreeExtNode SelectedNode { get; set; }
        [field: NonSerialized]
        public ModuleTreeExtNode RelativeNode { get; set; }
        public int ProjectID { get; set; }

        //public ContextMenu m_Menu = new ContextMenu();
        //public MenuItem disable_Item = new MenuItem();
    }
}
