using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using Heart.Inward;
using Heart.Outward;

namespace Plugin.TemplateMatching
{
    [Serializable]
    public class MatchingTool
    {
        #region 初始变量定义

        /// <summary>
        /// 是否显示匹配到的模板
        /// </summary>
        internal bool showTemplate = true;
        /// <summary>
        /// 是否显示中心十字架
        /// </summary>
        internal bool showCross = true;
        /// <summary>
        /// 是否显示特征
        /// </summary>
        internal bool showFeature = true;
        /// <summary>
        /// 显示结果序号
        /// </summary>
        internal bool showIndex = true;
        internal bool showSearchRegion = true;
        /// <summary>
        /// 模板句柄                                                   
        /// </summary>
        internal HTuple modelID = -1;
        /// <summary>
        /// 行列间隔像素数
        /// </summary>
        internal int spanPixelNum = 100;
        /// <summary>
        /// 模板区域
        /// </summary>
        internal MeasureROI templateRegion;
        internal HObject totalRegion;
        /// <summary>
        /// 搜索区域图像
        /// </summary>
        internal HObject reducedImage;
        /// <summary>
        /// 最小匹配分数
        /// </summary>
        internal double minScore = 0.5;
        /// <summary>
        /// 匹配个数
        /// </summary>
        internal int matchNum = 1;
        /// <summary>
        /// 起始角度
        /// </summary>
        internal int startAngle = -180;
        /// <summary>
        /// 角度范围
        /// </summary>
        internal int angleRange = 180;
        /// <summary>
        /// 角度步长
        /// </summary>
        internal int angleStep = 1;
        /// <summary>
        /// 对比度
        /// </summary>
        internal int contrast = 30;
        /// <summary>
        /// 极性
        /// </summary>
        internal string polarity = "use_polarity";

        /// <summary>
        /// 工具锁
        /// </summary>
        private object obj = new object();
        /// <summary>
        /// 模板匹配结果
        /// </summary>
        internal HObject brush_region;
        /// <summary>
        /// 模板匹配结果
        /// </summary>
        internal HObject final_region;

        internal HObject brush_region111;
        internal HObject final_region111;
        /// <summary>
        /// 搜索区域类型
        /// </summary>
        internal RegionType searchRegionType = RegionType.AllImage;
        /// <summary>
        /// 搜索区域
        /// </summary>
        internal List<MeasureROI> L_regions = new List<MeasureROI>();
        /// <summary>
        /// 搜索区域点数据，如Rectangle1的左上点行列坐标和右下点行列坐标，需要存储下来，在重绘区域时用
        /// </summary>
        internal List<double> searchRegionPointData = new List<double>();
        /// <summary>
        /// 搜索区域
        /// </summary>
        internal MeasureROI searchRegion;
        //internal HObject SearchRegion
        //{
        //    get
        //    {
        //        //if (L_regions != null && L_regions.Count > 0)
        //        //    _searchRegion = L_regions[0].getRegion();
        //        //return _searchRegion;
        //        return _searchRegion;
        //    }
        //    set { _searchRegion = value; }
        //}
        /// <summary>
        /// 做模板时的标准图像
        /// </summary>
        internal HObject Ho_Image;
        internal double minScale = 0.8;
        internal double maxScale = 1.2;
        internal MatchMode matchMode = MatchMode.BasedShape;
        /// <summary>
        /// 匹配最后轮廓特征
        /// </summary>
        public HObject contour;
        /// <summary>
        /// 系统需要显示的全部roi信息
        /// </summary>
        internal List<MeasureROI> ShowRegionList = new List<MeasureROI>();
        /// <summary>
        /// 区域类型
        /// </summary>
        internal enum RegionType
        {
            AllImage,
            Rectangle1,
            Rectangle2,
            Circle,
            Ellipse,
            MultPoint,
            Ring,
            Any,
            整幅图像,
            矩形,
            仿射矩形,
            圆,
            多点,
            椭圆,
            圆环,
            任意,
            InputRegion,        //此区域类型指来自于输入的区域
        }

        /// <summary>
        /// 模板类型
        /// </summary>
        internal enum MatchMode
        {
            BasedShape,
            BasedGray,
        }

        /// <summary>
        /// 模板匹配结果
        /// </summary>
        [Serializable]
        internal struct MatchResult
        {
            internal double Row;
            internal double Col;
            internal double Angle;
            internal double Socre;
        }
        #endregion

        /// <summary>
        /// 创建模板
        /// </summary>
        /// <returns>结果状态返回值：0表示成功 -1表示未知异常 1表示特征过少</returns>
        internal int CreateTemplate()//结果状态返回值：0表示成功 -1表示未知异常 1表示特征过少
        {
            try
            {
                HObject template;
                totalRegion = templateRegion.hobject;   //templateRegion是MeasureROI类，hobject应该是内部真实的
                try
                {
                    if (final_region != null)
                        HOperatorSet.Difference(templateRegion.hobject, final_region, out totalRegion);
                }
                catch { }
                try
                {
                    if (final_region111 != null)
                        HOperatorSet.Union2(totalRegion, final_region111, out totalRegion);
                }
                catch { }
                HOperatorSet.ReduceDomain(Ho_Image, totalRegion, out template);
                try
                {
                    HOperatorSet.CreateScaledShapeModel(template,
                                                    (HTuple)"auto",
                                                    Math.Round(startAngle * Math.PI / 180, 3),  //这里的角度要改为弧度值
                                                    Math.Round((angleRange - startAngle) * Math.PI / 180, 3),
                                                    (HTuple)("auto"),
                                                    minScale,
                                                    maxScale,
                                                    "auto",
                                                    (HTuple)"auto",
                                                    (HTuple)polarity,
                                                    "auto",
                                                    (HTuple)"auto",
                                                    out modelID);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("#8510:"))      //特征过少，Halcon报错编号8510
                    {
                        return 1;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// 显示模板
        /// </summary>
        internal void ShowTemplate()
        {
            try
            {
                if (modelID == -1)
                {
                    //Frm_ShapeMatchTool.Instance.label4.ForeColor = Color.Red;
                    //Frm_ShapeMatchTool.Instance.label4.Text = "状态：未创建模板";
                    return;
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        /// <summary>
        /// 查找模板
        /// </summary>
        /// <param name="_Type">模板类型</param>
        /// <param name="_Model">模式</param>
        /// <param name="_image">图片</param>
        /// <param name="_region">寻找区域</param>
        /// <param name="outCoord">输出坐标</param>
        internal void FindModel(out MatchResult matchresult, out bool matchResult)
        {
            //该函数添加了分数score的输出传参
            //outCoord = new MatchResult();
            try
            {
                //score = 0;
                matchResult = false;    //确保至少有一个MathcReslust的输出
                //HTuple row, col, Phi, scale, score;
                HTuple row, col, Phi, score, scale;
                HImage _image = new HImage(Ho_Image);      //新建一个_image对象
                
                if (_image.IsInitialized())
                {
                    HRegion _region = new HRegion();
                    HobjectToRegion(searchRegion.hobject, ref _region);
                    if (_region.IsInitialized())
                    {
                        //HOperatorSet.ReduceDomain(Ho_Image, _region, out _image);   //这里的_image不能直接等于Ho_Image然后再剪切，这样子会还是带有roi信息在里面
                        _image.ReduceDomain(_region);
                    }
                    //HOperatorSet.FindNccModel(_image,
                    //        (HTuple)modelID,
                    //        ((HTuple)startAngle).TupleRad(),
                    //        ((HTuple)angleRange - startAngle).TupleRad(),
                    //        (HTuple)minScore,
                    //        (HTuple)matchNum,
                    //        (HTuple)0.5,
                    //        (HTuple)"true",
                    //        (HTuple)0,
                    //         out row,
                    //         out col,
                    //         out Phi,
                    //         out score);
                    HOperatorSet.FindScaledShapeModel(_image,
                            (HTuple)modelID,
                            Math.Round(startAngle * Math.PI / 180, 3),  //这里的角度要改为弧度值
                            Math.Round((angleRange - startAngle) * Math.PI / 180, 3),
                             minScale,
                             maxScale,
                            (HTuple)minScore,
                            (HTuple)matchNum,
                            (HTuple)0.5,
                            (HTuple)"least_squares",
                            (HTuple)0,
                            (HTuple)0.9,
                             out row,
                             out col,
                             out Phi,
                             out scale,
                             out score);
                    if (score.Length > 0)
                    {
                        matchresult.Row = Math.Round(row[0].D, 6);
                        matchresult.Col = Math.Round(col[0].D, 6);
                        matchresult.Angle = Math.Round(Phi[0].D, 6);
                        matchresult.Socre = Math.Round(score[0].D, 6);
                        matchResult = true;    //模板匹配执行结果为成功
                    }
                    else
                    {
                        matchresult.Row = 0;
                        matchresult.Col = 0;
                        matchresult.Angle = 0;
                        matchresult.Socre = 0;
                        matchResult = false;    //模板匹配执行结果为失败
                    }
                }
                else
                {
                    matchresult.Row = 0;
                    matchresult.Col = 0;
                    matchresult.Angle = 0;
                    matchresult.Socre = 0;
                    matchResult = false;
                }   //模板匹配执行结果为失败
            }
            catch (System.Exception ex)
            {
                matchresult.Row = 0;
                matchresult.Col = 0;
                matchresult.Angle = 0;
                matchresult.Socre = 0;
                //MessageBox.Show(ex.ToString());
                matchResult = false;    //这里保留到最后也有MathcReslust的输出
            }

        }

        /// <summary>
        /// Hobject转Region
        /// </summary>
        /// <param name="ho_Image"></param>
        /// <param name="Region"></param>
        private void HobjectToRegion(HObject ho_Image, ref HRegion Region)
        {
            HObject ho_Domain;
            HTuple hv_Rows, hv_Columns;
            HOperatorSet.GetDomain(ho_Image, out ho_Domain);
            HOperatorSet.GetRegionPolygon(ho_Domain, 5, out hv_Rows, out hv_Columns);
            Region.GenRegionPolygonFilled(hv_Rows, hv_Columns);
        }
    }
}
