using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HalconDotNet;
using Heart.Inward;

namespace Plugin.ImageGraytesting
{
    [Category("01@检测识别")]
    [DisplayName("01@灰度检测#灰度检测")]
    [Serializable]
    class ModuleObj : ModuleObjBase
    {
        //变量必须赋初值
        //public string ImagePath { get; set; } = null;     
        //public HImage TestdImage { get; set; } = null;
        //public int CanvasIndex { get; set; } = -1;

        //定义变量，供数据输出
        public string ImagePath { get; set; } = null;
        public HImage ho_Image { get; set; } = null;

        public string hv_img_Str="";
        public HObject hv_img=null;
        private HTuple hv_Width=-1,hv_Height=-1;
        public HObject R1=null;
        public override int ExeModule(string entryName)
        {
            if (!string.IsNullOrEmpty(ImagePath))
            {
                ho_Image = new HImage(ImagePath);
                return 1;
            }
            else
            {
                ho_Image = null;
                return -1;
            }

        }

        //变量输出函数
        public override void UpdateOutput()
        {
            //输出两个数据到数据库列表,同时带有输出到数据框的功能
            //   AddOutputVariable(hv_img_Str.Split('.')[0] + ".Width", "double", hv_Width.D, hv_Width.D.ToString(), "图像宽度");
            //   AddOutputVariable(hv_img_Str.Split('.')[0] + ".Height", "double", hv_Height.D, hv_Height.D.ToString(), "图像高度");
            AddOutputVariable("Image", "Image", ho_Image, "ho_Image", "采集到的图像");
        }

        public override void UpdateDisplay()
        {
           
        }

        public void ReadImg()
        {
            if (hv_img_Str != "")
            {
                hv_img = (HObject)GetVariableDataByVarName(hv_img_Str);     //用文件索引来加载文件,并转换格式（IObject）
            }
        }
    }
}
