using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Heart.Inward;

namespace Plugin.LogicBreakCirculation
{
    [Category("02@逻辑工具")]
    [DisplayName("02@停止循环#停止循环")]
    [Serializable]
    public class ModuleObj : ModuleObjBase
    {
        public override int ExeModule(string entryName)
        {
            Project prj = Solution.Instance.GetProjectById(Info.ProjectID);
            List<string> entryList = prj.GetEntryList();
            int curIndex = entryList.IndexOf(entryName);

            int targetOffset = -1;
            string targetStr = "结束循环";
            for (int i = curIndex+1; i < entryList.Count; ++i)
            {
                if ((entryList[i].Length> targetStr.Length)&&(entryList[i].Substring(0, targetStr.Length) == targetStr))
                {
                    targetOffset = i - curIndex+1;
                    break;
                }
            }

            return targetOffset;
        }

        public override void UpdateOutput()
        {
            
        }

    }
}
