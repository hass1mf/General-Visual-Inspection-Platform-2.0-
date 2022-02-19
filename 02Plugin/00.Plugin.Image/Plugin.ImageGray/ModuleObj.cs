using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HalconDotNet;
using Heart.Inward;

namespace Plugin.ImageGray
{
    [Category("00@图像处理")]
    [DisplayName("02@灰度变换#灰度变换")]
    [Serializable]
    class ModuleObj : ModuleObjBase
    {
        //变量必须赋初值    
        //定义变量，供数据输出
        public string ImagePath { get; set; } = null;
        public HImage Ho_Image { get; set; } = null;
        public int CanvasIndex { get; set; } = -1;


        public string hv_img_Str = "";
        public HObject hv_img = null;
        private HTuple hv_Width = -1, hv_Height = -1;
        public HObject R1 = null;
        public override int ExeModule(string entryName)     //载入图像路径
        {
            if (hv_img != null)
            {
                HOperatorSet.GetImageSize(hv_img, out hv_Width, out hv_Height);
                return 1;
            }
            else
            {
                return -1;
            }
        }

        //变量输出函数
        public override void UpdateOutput()
        {
            HTuple width;
            HTuple height;

            if (Ho_Image != null)
            {
                Ho_Image.GetImageSize(out width, out height);   //获取长宽
                AddOutputVariable("GrayImage", "Image", Ho_Image, width.ToString() + "x" + height.ToString(), "灰度图像");  //输出到变量表
            }
            else
            {
                AddOutputVariable("GrayImage", "Image", Ho_Image, "null", "采集到的图像");
            }
        }


        public override void UpdateDisplay()
        {
            if (Ho_Image != null)
            {
                AddDisplayImage(CanvasIndex, Ho_Image);     //显示图像，CanvasIndex为之前选择的画布
            }
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
