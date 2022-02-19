using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HalconDotNet;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
//HImageExt类自己独立，无需其他引用
//HImageExt类用于保存图像到本地
//HImageExt类被HWindowFixExt.cs调用    if (hv_image is HImageExt)  //调用HImageExt类，用于保存图像  { ((HImageExt)hv_image).WriteImageExt(sfd.FileName);} 第297行

namespace Heart.Outward
{
    [Serializable]
    public class HImageExt : HImage, ISerializable
    {

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public HImageExt() : base()
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileName"></param>
        public HImageExt(string fileName) : base(fileName)
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="copy"></param>
        public HImageExt(IntPtr key, bool copy) : base(key, copy)
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="obj"></param>
        public HImageExt(HObject obj) : base(obj)
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pixelPointer"></param>
        public HImageExt(string type, int width, int height, IntPtr pixelPointer) : base(type, width, height, pixelPointer)
        {
        }
        /// <summary>
        /// 反序列化构造函数
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public HImageExt(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new System.ArgumentNullException("info");
            }

            //magical 20170821
            try
            {
                this.measureROIlist = (List<MeasureROI>)info.GetValue("measureROIlist", typeof(List<MeasureROI>));
            }
            catch (Exception ex)
            {
                //Helper.LogHandler.Instance.VTLogError(ex.ToString());
            }
        }
        #endregion
        #region 静态函数，通过已有HImage 获取HImageExt
        /// <summary>
        /// 静态函数，通过已有HImage 获取HImageExt
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static HImageExt Instance(HObject image)
        {
            return new HImageExt(image);
        }
        #endregion
        /// <summary>
        /// 从he文件中获取ImageExt对象
        /// </summary>
        /// <param name="fileName">he文件所在路径</param>
        /// <returns></returns>
        public static HImageExt ReadImageExt(string fileName)
        {
            HImageExt ImgExt = null;
            try
            {
                string ext = System.IO.Path.GetExtension(fileName).ToLower();
                if (ext == ".he")
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Open))//作为语句：用于定义一个范围，在此范围的末尾将释放对象。 请参阅 using 语句。 by:longteng
                    {
                        fs.Seek(0, SeekOrigin.Begin);
                        BinaryFormatter binaryFmt = new BinaryFormatter();
                        ImgExt = (HImageExt)binaryFmt.Deserialize(fs);
                        //fs.Close();  //by:longteng
                    }
                }
                else
                {
                    HImage temp = new HImage(fileName);
                    ImgExt = HImageExt.Instance(temp);
                }
                return ImgExt;
            }
            catch (System.Exception ex)
            {
                return ImgExt;
            }
        }

        /// <summary>
        /// 保存HimageExt 到本地
        /// </summary>
        /// <param name="fileName">文件路径</param>
        public void WriteImageExt(string fileName)
        {
            string ext = Path.GetExtension(fileName);

            if (ext == ".he")
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    BinaryFormatter binaryFmt = new BinaryFormatter();
                    fs.Seek(0, SeekOrigin.Begin);
                    binaryFmt.Serialize(fs, this);
                    //fs.Close(); //by:longteng
                }
            }
            else if (ext == "") //当没有传入有后缀的fileName,默认保存png magical 20170822
            {
                string type = this.GetImageType().ToString();
                if (type.Contains("real"))
                {
                    this.WriteImage("tiff", 0, fileName);
                }
                else
                {
                    this.WriteImage("png best", 0, fileName);
                }
            }
            else
            {
                this.WriteImage(ext.Substring(1), 0, fileName);
            }

        }


        public List<MeasureROI> measureROIlist = new List<MeasureROI>();    //调用MeasureROI类

        public void UpdateRoiList(MeasureROI ROI)
        {
            try
            {
                int index = measureROIlist.FindIndex(e => e.CellID == ROI.CellID && e.roiType == ROI.roiType);
                if (index > -1)
                    measureROIlist[index] = ROI;
                else
                    measureROIlist.Add(ROI);
            }
            catch (Exception ex)
            {

            }
        }

        [OnSerializing()]
        internal void OnSerializingMethod(StreamingContext context)
        {
            foreach (MeasureROI ROI in measureROIlist)
            {
                if (ROI.hobject == null || !ROI.hobject.IsInitialized())//修复为null 错误 magical 20171103
                {
                    ROI.hobject = null;
                }
            }
        }
        [OnDeserialized()]
        internal void OnDeSerializedMethod(StreamingContext context)
        {
            //函数已清空
        }

        /// <summary>
        /// 序列化相关，必须添加
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new System.ArgumentNullException("info");
            }

            //info.AddValue("X", X);
            //info.AddValue("Y", Y);
            //info.AddValue("Z", Z);
            //info.AddValue("PhiX", PhiX);
            //info.AddValue("PhiY", PhiY);
            //info.AddValue("ScaleX", ScaleX);
            //info.AddValue("ScaleY", ScaleY);
            //magical 20170821
            info.AddValue("measureROIlist", measureROIlist);

            //Himage 内部函数 反编译得到的
            HSerializedItem item = this.SerializeImage();
            byte[] buffer = (byte[])item;
            item.Dispose();
            info.AddValue("data", buffer, typeof(byte[]));
        }
    }
}
