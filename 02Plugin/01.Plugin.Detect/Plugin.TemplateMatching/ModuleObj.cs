using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HalconDotNet;
using Heart.Inward;
using Heart.Outward;

namespace Plugin.TemplateMatching
{
    [Category("01@检测识别")]
    [DisplayName("00@模板匹配#模板匹配")]
    [Serializable]
    class ModuleObj : ModuleObjBase
    {
        //变量必须赋初值
        //定义变量，供数据输出
        //public string ImagePath { get; set; } = null;
        //public HImage Ho_Image { get; set; } = null;
        public int CanvasIndex { get; set; } = -1;
        /// <summary>
        /// 工具对象
        /// </summary>
        public MatchingTool Obj_shapeMatchTool = new MatchingTool();

        public string hv_img_Str = "";
        public HImageExt hv_img = null;     //1015这里改为了HImageExt!!!原HObject
        MatchingTool.MatchResult finalResult;  //最后
        bool ExeResult;     //模板匹配最后的执行结果
        List<MeasureROI> OutputROIs = new List<MeasureROI>();   //执行最后输出到系统的Roi信息

        /// <summary>
        /// ①执行模块
        /// </summary>
        /// <param name="entryName"></param>
        /// <returns></returns>
        public override int ExeModule(string entryName)     //载入图像路径 
        {
            ReadImg();  //执行的第一步先读取图像进来
            
            if (hv_img != null && Obj_shapeMatchTool != null)
            {
                Obj_shapeMatchTool.Ho_Image = new HObject(hv_img);  //这里不仅要进行新的抓取，还要赋值给变量
                Obj_shapeMatchTool.FindModel(out finalResult, out ExeResult);     //执行使，只直接进行执行查找模板函数
                if (ExeResult)
                {
                    UpdateRoi(ref Obj_shapeMatchTool, finalResult.Angle, finalResult.Row, finalResult.Col);     //通过矩阵变换来更新计算新的ROI
                    //搜索范围
                    MeasureROI roi搜索范围 = new MeasureROI(Info.ProjectID.ToString(), "模板匹配", Info.ModuleName, enMeasureROIType.搜索范围.ToString(), "red", new HObject(Obj_shapeMatchTool.searchRegion.hobject));    //整理ROI信息
                    hv_img.UpdateRoiList(roi搜索范围);
                    ////模板范围
                    //MeasureROI roi模板范围 = new MeasureROI(Info.ProjectID.ToString(), "模板匹配", Info.ModuleName, enMeasureROIType.模板范围.ToString(), "green", new HObject(Obj_shapeMatchTool.templateRegion.hobject));    //整理ROI信息
                    //hv_img.UpdateRoiList(roi模板范围);
                    //检测结果
                    MeasureROI roi检测结果 = new MeasureROI(Info.ProjectID.ToString(), "模板匹配", Info.ModuleName, enMeasureROIType.检测结果.ToString(), "green", new HObject(Obj_shapeMatchTool.contour));    //整理ROI信息
                    hv_img.UpdateRoiList(roi检测结果);
                    OutputROIs.Clear();     //一定要先清空
                    OutputROIs.Add(roi搜索范围);
                    //OutputROIs.Add(roi模板范围);  去掉模板区域
                    OutputROIs.Add(roi检测结果);
                    return 1;
                }
                else
                {
                    finalResult.Row = 0;
                    finalResult.Col = 0;
                    finalResult.Angle = 0;
                    finalResult.Socre = 0;
                    return -1;  //赋值操作要放在return前面，不然直接return就退出了
                }
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// ②变量输出
        /// </summary>
        public override void UpdateOutput()
        {
            if (hv_img != null && ExeResult)
            {
                AddOutputVariable("X", "Double", finalResult.Row, finalResult.Row.ToString(), "X");             //输出到变量表
                AddOutputVariable("Y", "Double", finalResult.Col, finalResult.Col.ToString(), "Y");             //输出到变量表
                AddOutputVariable("Deg", "Double", finalResult.Angle, finalResult.Angle.ToString(), "Deg");     //输出到变量表
                AddOutputVariable("分数", "Double", finalResult.Socre, finalResult.Socre.ToString(), "分数");   //输出到变量表
            }
            else
            {
                //AddOutputVariable("GrayImage", "Image", Ho_Image, "null", "采集到的图像");
                AddOutputVariable("X", "Double", 0, "null", "X");             //输出到变量表
                AddOutputVariable("Y", "Double", 0, "null", "Y");             //输出到变量表
                AddOutputVariable("Deg", "Double", 0, "null", "Deg");     //输出到变量表
                AddOutputVariable("分数", "Double", 0, "null", "分数");   //输出到变量表
            }
        }

        /// <summary>
        /// ③更新显示
        /// </summary>
        public override void UpdateDisplay()
        {
            if (hv_img != null && ExeResult && hv_img_Str != "")
            {
                int canvas = ConfirmCanvasByImageVariable(hv_img_Str);
                //AddDisplayImage(CanvasIndex, Ho_Image);     //显示图像，CanvasIndex为之前选择的画布
                AddDisplayRois(canvas, OutputROIs);      //和输出图像变量一样，输出RoiList的变量输出
            }
        }

        /// <summary>
        /// 给moduleform用的读图函数
        /// </summary>
        public void ReadImg()
        {
            if (hv_img_Str != "")
            {
                hv_img = new HImageExt((HImage)GetVariableDataByVarName(hv_img_Str));     //用文件索引来加载文件,并转换格式（IObject）   //20211022系统的传输图像变量改回为HImage，如果插件内部需要ROI内容，可自己将(himage)object实例为HImageExt
            }
        }


        /// <summary>
        /// 显示模板2
        /// </summary>
        /// <param name="matchingTool"></param>
        /// <param name="angle"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        internal void UpdateRoi(ref MatchingTool matchingTool, double angle, double row, double col)
        {
            try
            {
                HOperatorSet.GetShapeModelContours(out matchingTool.contour, matchingTool.modelID, new HTuple(1)); //shapeMatchTool.contour 最后输出的轮廓对象   //通过modelID来生成目标轮廓contour
                HHomMat2D homMat2D = new HHomMat2D();  //二维变换的齐次变换矩阵  
                homMat2D.VectorAngleToRigid(0, 0, 0, (HTuple)row, (HTuple)col, (HTuple)angle);    //从点和角度计算刚性仿射变换
                HOperatorSet.AffineTransContourXld(matchingTool.contour, out matchingTool.contour, homMat2D);   //有了放射矩阵homMat2D，计算后的shapeMatchTool.contour应该已经经过了修正    //使用旋转平移矩阵仿射变换来得到最新的目标轮廓contour 
                //HXLDCont modelRegionXLD = new HXLDCont();
                //HobjectToXld(matchingTool.searchRegion.hobject, ref modelRegionXLD);
                //modelRegionXLD.AffineTransContourXld(homMat2D);     //模型检测范围的搜索区域
                //matchingTool.searchRegion = new MeasureROI(Info.ProjectID.ToString(), "模板匹配", Info.ModuleName, enMeasureROIType.模板范围.ToString(), "green", new HObject(modelRegionXLD));    //整理ROI信息
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Hobject转Xld
        /// </summary>
        /// <param name="ho_Region"></param>
        /// <param name="Contour"></param>
        private void HobjectToXld(HObject ho_Region, ref HXLDCont Contour)
        {
            HObject ho_RegionBorder, ho_Contours;
            HTuple hv_Row, hv_Col;
            HOperatorSet.Boundary(ho_Region, out ho_RegionBorder, "inner");
            HOperatorSet.GenContourRegionXld(ho_RegionBorder, out ho_Contours, "border");
            HOperatorSet.GetContourXld(ho_Contours, out hv_Row, out hv_Col);
            Contour.GenContourPolygonXld(hv_Row, hv_Col);
        }
    }
}
