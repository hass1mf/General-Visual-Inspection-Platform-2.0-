using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HalconDotNet;
using System.Runtime.Serialization;
//MeasureROI类自己独立，无需其他引用
//MeasureROI类用于ROI
//MeasureROI类被HImgaeExt.cs调用  public List<MeasureROI> measureROIlist = new List<MeasureROI>(); 第139行

namespace Heart.Outward
{
    /// <summary>
    /// 用于展示效果的HObject
    /// </summary>
    [Serializable]
    public class MeasureROI
    {
        public MeasureROI()
        { }

        /// <summary>
        /// 测量roi
        /// </summary>
        /// <param name="_CellID">单元id</param>
        /// <param name="_CellCatagory">单元类型</param>
        /// <param name="_CellDesCribe">单元描述</param>
        /// <param name="_drawColor">画笔颜色</param>
        /// <param name="_hobject">测量roi 必须为HObject类型</param>
        public MeasureROI(string _CellID, string _CellCatagory, string _CellDesCribe, string _roiType, string _drawColor, HObject _hobject)
        {
            CellID = _CellID;
            CellCatagory = _CellCatagory;
            CellDesCribe = _CellDesCribe;
            roiType = _roiType;
            drawColor = _drawColor;
            hobject = _hobject;
        }
        /// <summary>
        /// 单元id
        /// </summary>
        public string CellID { get; set; }
        /// <summary>
        /// 单元类型
        /// </summary>
        public string CellCatagory { get; set; }
        /// <summary>
        /// 单元描述
        /// </summary>
        public string CellDesCribe { get; set; }
        /// <summary>
        /// 轮廓分类
        /// </summary>
        public string roiType { get; set; }
        /// <summary>
        /// 画笔颜色
        /// </summary>
        public string drawColor { get; set; }
        /// <summary>
        /// 测量roi
        /// </summary>
        public HObject hobject { get; set; }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            if (hobject != null && !hobject.IsInitialized())//修复为null 错误 magical 20171103
            {
                hobject = null;
            }
        }

    }

    [Serializable]
    public class MeasureROIText : MeasureROI
    {

        /// <summary>
        /// 文字
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 字体
        /// </summary>
        public string font = "mono";

        /// <summary>
        /// 显示的位置
        /// </summary>
        public double row { get; set; }
        public double col { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        public int size { get; set; }

        /// <summary>
        /// 测量roi
        /// </summary>
        /// <param name="_CellID">单元id</param>
        /// <param name="_CellCatagory">单元类型</param>
        /// <param name="_CellDesCribe">单元描述</param>
        /// <param name="_drawColor">画笔颜色</param>
        /// <param name="_hobject">测量roi 必须为HObject类型</param>
        public MeasureROIText(string _CellID, string _CellCatagory, string _CellDesCribe, string _roiType, string _drawColor,
                string _text, string _font, double _row, double _col, int _size)
        {
            hobject = null;

            CellID = _CellID;
            CellCatagory = _CellCatagory;
            CellDesCribe = _CellDesCribe;
            roiType = _roiType;
            drawColor = _drawColor;
            text = _text;
            font = _font;
            row = _row;
            col = _col;
            size = _size;
        }

    }

    /// <summary>
    /// 轮廓分类
    /// </summary>
    public enum enMeasureROIType
    {
        模板范围,
        检测点,
        检测结果,
        搜索范围,
        屏蔽范围,
        搜索方向,
        参考坐标系,
        文字显示
    }
    /// <summary>
    /// ROI信息定义，创建ROI类时都要用到此类
    /// </summary>
    [Serializable]
    public abstract class ROI
    {
        private string _Color = "#00FF00";
        public string sColor
        {
            get { return _Color; }
            set { _Color = value; }
        }
        public abstract HRegion genRegion();
        public abstract HXLDCont genXLD();
        public abstract HTuple getTuple();
    }

    #region 绘制区域时相关信息的定义

    /// <summary>
    /// 矩形信息
    /// </summary>
    [Serializable]
    public class Rectangle_INFO : ROI
    {
        public double StartY, StartX, EndY, EndX;
        public Rectangle_INFO()
        {

        }
        public Rectangle_INFO(double m_Row_Start, double m_Column_Start, double m_Row_End, double m_Column_End)
        {
            this.StartY = m_Row_Start;
            this.StartX = m_Column_Start;
            this.EndY = m_Row_End;
            this.EndX = m_Column_End;
        }
        public override HRegion genRegion()
        {
            HRegion h = new HRegion();
            h.GenRectangle1(StartY, StartX, EndY, EndX);
            return h;
        }
        public override HXLDCont genXLD()
        {
            HXLDCont xld = new HXLDCont();
            HTuple row = new HTuple(StartY, EndY, EndY, StartY, StartY);
            HTuple col = new HTuple(StartX, StartX, EndX, EndX, StartX);
            xld.GenContourPolygonXld(row, col);
            return xld;
        }

        public override HTuple getTuple()
        {
            double[] rect1 = new double[] { StartY, StartX, EndY, EndX };
            return new HTuple(rect1);
        }
    }

    /// <summary>
    /// 旋转矩形信息
    /// </summary>
    [Serializable]
    public class Rectangle2_INFO : ROI, ICloneable
    {
        public double CenterY { get; set; }
        public double CenterX { get; set; }
        public double Phi { get; set; }
        public double Length1 { get; set; }
        public double Length2 { get; set; }

        public Rectangle2_INFO()
        {
        }
        public Rectangle2_INFO(double m_Row_center, double m_Column_center, double m_Phi, double m_Length1, double m_Length2)
        {
            this.CenterY = m_Row_center;
            this.CenterX = m_Column_center;
            this.Phi = m_Phi;
            this.Length1 = m_Length1;
            this.Length2 = m_Length2;
        }
        public override HRegion genRegion()
        {
            HRegion h = new HRegion();
            h.GenRectangle2(CenterY, CenterX, Phi, Length1, Length2);
            return h;
        }
        public override HXLDCont genXLD()
        {
            HXLDCont xld = new HXLDCont();
            xld.GenRectangle2ContourXld(CenterY, CenterX, Phi, Length1, Length2);
            return xld;
        }
        public override HTuple getTuple()
        {
            double[] rect2 = new double[] { CenterY, CenterX, Phi, Length1, Length2 };
            return new HTuple(rect2);
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    /// <summary>
    /// 圆信息
    /// </summary>
    [Serializable]
    public class Circle_INFO : ROI, ICloneable
    {
        public double CenterY, CenterX, Radius;
        public double StartPhi = 0.0, EndPhi = Math.PI * 2;
        public string PointOrder = "positive";
        public Circle_INFO()
        {
        }
        public Circle_INFO(double m_Row_center, double m_Column_center, double m_Radius)
        {
            this.CenterY = m_Row_center;
            this.CenterX = m_Column_center;
            this.Radius = m_Radius;
        }
        public Circle_INFO(double m_Row_center, double m_Column_center, double m_Radius, double m_StartPhi, double m_EndPhi, string m_PointOrder)
        {
            this.CenterY = m_Row_center;
            this.CenterX = m_Column_center;
            this.Radius = m_Radius;
            this.StartPhi = m_StartPhi;
            this.EndPhi = m_EndPhi;
        }
        public override HRegion genRegion()
        {
            HRegion h = new HRegion();
            h.GenCircle(CenterY, CenterX, Radius);
            return h;
        }
        public override HXLDCont genXLD()
        {
            HXLDCont xld = new HXLDCont();
            xld.GenCircleContourXld(CenterY, CenterX, Radius, StartPhi, EndPhi, PointOrder, 1.0);
            return xld;
        }

        public override HTuple getTuple()
        {
            double[] circle = new double[] { CenterY, CenterX, Radius };
            return new HTuple(circle);
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    /// <summary>
    /// 椭圆信息
    /// </summary>
    [Serializable]
    public class Ellipse_INFO : ROI, ICloneable
    {
        public double CenterY, CenterX, Phi, Radius1, Radius2;
        double StartPhi = 0.0, EndPhi = Math.PI * 2;
        public string PointOrder = "positive";
        public Ellipse_INFO()
        {
        }
        public Ellipse_INFO(double m_Row_center, double m_Column_center, double m_Phi, double m_Radius1, double m_Radius2)
        {
            this.CenterY = m_Row_center;
            this.CenterX = m_Column_center;
            this.Phi = m_Phi;
            this.Radius1 = m_Radius1;
            this.Radius2 = m_Radius2;
        }
        public override HRegion genRegion()
        {
            HRegion h = new HRegion();
            h.GenEllipse(CenterY, CenterX, Phi, Radius1, Radius2);
            return h;
        }

        public override HXLDCont genXLD()
        {
            HXLDCont xld = new HXLDCont();
            xld.GenEllipseContourXld(CenterY, CenterX, Phi, Radius1, Radius2, StartPhi, EndPhi, PointOrder, 1.0);
            return xld;
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public override HTuple getTuple()
        {
            double[] ellipse = new double[] { CenterY, CenterX, Phi, Radius1, Radius2 };
            return new HTuple(ellipse);
        }
    }

    /// <summary>
    /// 添加自定义形状
    /// </summary>
    [Serializable]
    public class UserDefinedShape_INFO : ROI
    {
        HRegion mHRegion;
        public UserDefinedShape_INFO()
        {
        }
        public UserDefinedShape_INFO(HRegion hregion)
        {
            mHRegion = hregion;
        }
        public override HRegion genRegion()
        {
            return mHRegion;
        }
        public override HXLDCont genXLD()
        {
            if (mHRegion != null && mHRegion.IsInitialized())
            {
                return mHRegion.GenContourRegionXld("border_holes");
            }
            else
            {
                return new HXLDCont();
            }
        }
        public override HTuple getTuple()
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
