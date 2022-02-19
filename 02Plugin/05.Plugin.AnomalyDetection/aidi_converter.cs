using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aqrose.aidi_vision;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using HalconDotNet;

namespace aqrose.aidi_vision_client
{
    [Serializable]
    public class ImageConverter
    {
        //拷贝指针数据的接口
        [DllImport("kernel32.dll")]
        public static extern void CopyMemory(long Destination, long Source, int Length);

        //bgr指针转成bgra，透明度置255
        public byte[] bgr_to_pbgra(byte[] bgr, int height, int width)
        {
            byte[] bgra = new byte[height * width * 4];
            for (var pos = 0; pos < height * width; pos++)
            {
                bgra[pos * 4] = bgr[pos * 3];
                bgra[pos * 4 + 1] = bgr[pos * 3 + 1];
                bgra[pos * 4 + 2] = bgr[pos * 3 + 2];
                bgra[pos * 4 + 3] = 255;
            }
            return bgra;
        }

        //bgra指针转成bgr，直接删除透明度
        public byte[] pbgra_to_bgr(byte[] bgra, int height, int width)
        {
            byte[] bgr = new byte[height * width * 3];
            for (var pos = 0; pos < height * width; pos++)
            {
                bgr[pos * 3] = bgra[pos * 4];
                bgr[pos * 3 + 1] = bgra[pos * 4 + 1];
                bgr[pos * 3 + 2] = bgra[pos * 4 + 2];
            }
            return bgr;
        }

        //bgra指针转成bgr，根据透明度比例计算像素值
        public byte[] bgra_to_bgr(byte[] bgra, int height, int width)
        {
            byte[] bgr = new byte[height * width * 3];
            for (var pos = 0; pos < height * width; pos++)
            {
                byte byte_scale = bgra[pos * 4 + 3];
                if (byte_scale == 255)
                {
                    bgr[pos * 3] = bgra[pos * 4];
                    bgr[pos * 3 + 1] = bgra[pos * 4 + 1];
                    bgr[pos * 3 + 2] = bgra[pos * 4 + 2];
                }
                else
                {
                    float scale = (float)bgra[pos * 4 + 3] / 255.0f;
                    if (scale < 0)
                    {
                        scale = 0;
                    }
                    if (scale > 1)
                    {
                        scale = 1;
                    }
                    float b = (float)bgra[pos * 4] * scale;
                    float g = (float)bgra[pos * 4 + 1] * scale;
                    float r = (float)bgra[pos * 4 + 2] * scale;
                    bgr[pos * 3] = (byte)b;
                    bgr[pos * 3 + 1] = (byte)g;
                    bgr[pos * 3 + 2] = (byte)r;
                }
            }
            return bgr;
        }

        //bitmap转换成byte[]
        public byte[] bitmap_to_byte(ref Bitmap bmp, out int stride, out int channel_number)
        {
            //获取rect，为了锁数据
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            //数据加锁
            var bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            //获取stride
            stride = bmpData.Stride;
            //数据类型
            string bit_number = bmp.PixelFormat.ToString();
            //根据数据类型决定通道数
            if (bit_number.Contains("24") || bit_number.Contains("32"))
            { 
                //24位和32位即为3通道，8位为1通道
                channel_number = 3;
            }
            else
            {
                channel_number = 1;
            }
            var rowBytes = bmpData.Width * System.Drawing.Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
            var imgBytes = bmp.Height * rowBytes;
            byte[] rgbValues = new byte[imgBytes];
            IntPtr ptr = bmpData.Scan0;
            //判断数据会不会偏移
            if (stride == rowBytes)
            {
                Marshal.Copy(ptr, rgbValues, 0, imgBytes);
            }
            else
            {
                for (var i = 0; i < bmp.Height; i++)
                {
                    Marshal.Copy(ptr, rgbValues, i * rowBytes, rowBytes);   // 对齐
                    ptr += stride; // next row
                }
            }
            bmp.UnlockBits(bmpData);
            //如果是32位的，转换成24位
            if (bmp.PixelFormat == PixelFormat.Format32bppPArgb || bmp.PixelFormat == PixelFormat.Format32bppArgb)
            {
                rgbValues = pbgra_to_bgr(rgbValues, bmp.Height, bmp.Width);
            }
            return rgbValues;
        }

        //byte[]转换成bitmap
        public Bitmap byte_to_bitmap(byte[] rgbValues, int width, int height, int channel_number)
        {
            //初始化像素数据类型
            PixelFormat pixel_format = new PixelFormat();

            //判断类型
            if(channel_number == 1)
            {
                //单通道
                pixel_format = PixelFormat.Format8bppIndexed;
            }
            else if(channel_number == 3)
            {
                //如果是三通道图，设置为对应格式
                pixel_format = PixelFormat.Format24bppRgb;
            }
            else if (channel_number == 4)
            {
                //四通道
                pixel_format = PixelFormat.Format32bppRgb;
            }
            else
            {
                //其他通道数不支持
                Console.WriteLine("Do not support aidi::Image with {0} channels", channel_number);
            }

            //初始化bitmap
            Bitmap bmp = new Bitmap(width, height, pixel_format);
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            //锁
            BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
            //获取strade
            int strade = bmpData.Stride;
            //获取bitmap的指针
            IntPtr ptr = bmpData.Scan0;

            //判断数据会不会偏移
            if (strade == bmpData.Width * channel_number)
            {
                Marshal.Copy(rgbValues, 0 , ptr, height * strade);
            }
            else
            {
                for (var i = 0; i < bmp.Height; i++)
                {
                    Marshal.Copy(rgbValues, i * bmpData.Width * channel_number, ptr, bmpData.Width * channel_number);   // 对齐
                    ptr += strade; // next row
                }
            }

            //解锁
            bmp.UnlockBits(bmpData);
            //返回
            return bmp;
        }

        //bitmap转成aqimage
        public void bitmap_to_aqimg(ref Bitmap bitmap, ref aqrose.aidi_vision.Image aidi_image)
        {
            //先将bitmap转成byte[]
            int stride,channel_number;
            byte[] image_bytes = bitmap_to_byte(ref bitmap, out stride, out channel_number);
            //byte[]转成AIDI的Image
            aidi_image.from_chars(image_bytes, bitmap.Height, bitmap.Width, channel_number);
        }

        //aqimage转成bitmap
        public Bitmap aqimg_to_bitmap(ref aqrose.aidi_vision.Image aidi_image)
        {
            //先将aqimage转成byte[]
            byte[] image_bytes = new byte[aidi_image.data_byte_size()];
            aidi_image.to_chars(image_bytes, aidi_image.data_byte_size());
            //byte[]转成bitmap
            return byte_to_bitmap(image_bytes, aidi_image.width(), aidi_image.height(), aidi_image.total_channels());
        }

        //Bitmap转HImage
        public HObject bigmap_to_hobject(Bitmap bImage)
        {
            //获取rect，为了锁数据
            var rect = new Rectangle(0, 0, bImage.Width, bImage.Height);
            //数据加锁
            var bmpData = bImage.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bImage.PixelFormat);
            //获取stride
            int stride = bmpData.Stride;
            //数据类型
            string bit_number = bImage.PixelFormat.ToString();
            int channel_number = 0;
            //根据数据类型决定通道数
            if (bit_number.Contains("24") || bit_number.Contains("32"))
            {
                //24位和32位即为3通道，8位为1通道
                channel_number = 3;
            }
            else
            {
                channel_number = 1;
            }
            //每一行有多少个byte
            var rowBytes = bmpData.Width * System.Drawing.Image.GetPixelFormatSize(bImage.PixelFormat) / 8;
            //整张图有多少个byte
            var imgBytes = bImage.Height * rowBytes;
            //新建byte[]存放转换后图像的数据
            byte[] rgbValues = new byte[imgBytes];
            //bitmap的数据指针
            IntPtr ptr = bmpData.Scan0;
            //判断数据会不会偏移
            if (stride == rowBytes)
            {
                //不偏移时直接复制
                Marshal.Copy(ptr, rgbValues, 0, imgBytes);
            }
            else
            {
                //偏移时，把每一行最后多出来的byte给去掉
                for (var i = 0; i < bImage.Height; i++)
                {
                    Marshal.Copy(ptr, rgbValues, i * rowBytes, rowBytes);   // 对齐
                    ptr += stride; // next row
                }
            }
            //图像数据解锁
            bImage.UnlockBits(bmpData);

            //新建一个指针，用于生成halcon图像
            IntPtr pixelPointer = Marshal.AllocHGlobal(imgBytes); 
            //拷贝数据
            Marshal.Copy(rgbValues, 0, pixelPointer, imgBytes);
            //新建halcon图像
            HObject halcon_image = new HObject();
            //不同通道数使用不同创造方法
            if (channel_number == 1) //判断8位、24位、32位
            {
                //单通道图创建方法
                HOperatorSet.GenImage1(out halcon_image, "byte" ,bImage.Width, bImage.Height, pixelPointer);
            }
            else if (channel_number == 3)
            {
                //三通道图创建方法
                HOperatorSet.GenImageInterleaved(out halcon_image, pixelPointer, "bgr", bImage.Width, bImage.Height, -1, "byte", 0, 0, 0, 0, -1, 0);
            }
            else
            {
                throw new Exception("The PixelFormat of bitmap is higher than 4!");
            }

            return halcon_image;
        }

        //Bitmap转HImage，使用了unsafe代码来提高转换效率，需要在工程属性-构建中选中“允许不安全代码”
        public HObject bitmap_to_hobject_unsafe(Bitmap bitmap)
        {
            //新建halcon图像
            HObject halcon_image = new HObject();
            //图像的高度
            int height = bitmap.Height;
            //图像的宽度
            int width = bitmap.Width;
            //数据类型
            string bit_number = bitmap.PixelFormat.ToString();
            //通道数
            int channel_number = 0;
            //根据数据类型决定通道数
            if (bit_number.Contains("24") || bit_number.Contains("32"))
            {
                //24位和32位即为3通道，8位为1通道
                channel_number = 3;
            }
            else
            {
                channel_number = 1;
            }

            Rectangle imgRect = new Rectangle(0, 0, width, height);
            BitmapData bitData = bitmap.LockBits(imgRect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            //由于Bitmap图像每行的字节数必须保持为4的倍数，因此在行的字节数不满足这个条件时，会对行进行补充，步幅数Stride表示的就是补充过后的每行的字节数，也成为扫描宽度
            int stride = bitData.Stride;
            unsafe
            {
                int row_bytes = width * channel_number;
                int total_bytes = height * width * channel_number;
                byte[] data = new byte[total_bytes];
                byte* bptr = (byte*)bitData.Scan0;
                fixed (byte* pData = data)
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < row_bytes; j++)
                        {
                            //舍去填充的部分
                            data[i * row_bytes + j] = bptr[i * stride + j];
                        }
                    }
                    if (channel_number == 1)
                    {
                        HOperatorSet.GenImage1(out halcon_image, "byte", width, height, new IntPtr(pData));
                    }
                    else 
                    {
                        HOperatorSet.GenImageInterleaved(out halcon_image , new IntPtr(pData), "bgr", width, height, -1, "byte", 0, 0, 0, 0, -1, 0);
                    }
                }
            }
            bitmap.UnlockBits(bitData);
            return halcon_image;
        }

        //Himage转bitmap
        public Bitmap hobject_to_bitmap(HObject halcon_image)
        {
            //获取HImage的通道信息
            HTuple channelNum = new HTuple();
            HOperatorSet.CountChannels(halcon_image, out channelNum);

            //判断8位、24位、32位
            if (channelNum == 1) 
            {
                //单通道图
                //先得到图像的数据指针
                HTuple hpoint, type, width, height;
                HOperatorSet.GetImagePointer1(halcon_image, out hpoint, out type, out width, out height);
                Bitmap res = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
                const int Alpha = 255;
                //调色盘，没有的话会乱码
                ColorPalette pal = res.Palette;
                for (int i = 0; i <= 255; i++)
                {
                    pal.Entries[i] = Color.FromArgb(Alpha, i, i, i);
                }
                res.Palette = pal;
                //新建Bitmap
                BitmapData bitmapData = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                int lenth = width * height;
                byte[] bmpData = new byte[lenth];
                Marshal.Copy(hpoint, bmpData, 0, lenth);
                //去除bitmap中多余的空位
                if (width % 4 == 0)
                {
                    IntPtr iptr = bitmapData.Scan0;  // 获取bmpData的内存起始位置              
                    Marshal.Copy(bmpData, 0, iptr, lenth);//用Marshal的Copy方法，将刚才得到的内存字节数组复制到BitmapData中  
                }
                else
                {
                    byte[] NewbmpData = new byte[bitmapData.Stride * bitmapData.Height];
                    for (int i = 0; i < bitmapData.Height; i++)
                    {
                        for (int j = 0; j < bitmapData.Width; j++)
                        {
                            NewbmpData[i * bitmapData.Stride + j] = bmpData[i * bitmapData.Width + j];
                        }
                    }
                    IntPtr iptr = bitmapData.Scan0;  //获取bmpData的内存起始位置                  
                    Marshal.Copy(NewbmpData, 0, iptr, bitmapData.Height * bitmapData.Stride);//用Marshal的Copy方法，将刚才得到的内存字节数组复制到BitmapData中           
                }
                res.UnlockBits(bitmapData);
                return res;
            }
            else if (channelNum == 3 || channelNum == 4)
            {
                //3通道图
                //先得到图像的数据指针
                HTuple hred, hgreen, hblue, type, width, height;
                HOperatorSet.GetImagePointer3(halcon_image, out hred, out hgreen, out hblue, out type, out width, out height);
                //新建bitmap
                Bitmap res = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                BitmapData bitmapData = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                //分别取出RGB三个通道的数据
                int lenth = width * height;
                byte[] bmpDataRed = new byte[lenth];
                byte[] bmpDataGreen = new byte[lenth];
                byte[] bmpDataBlue = new byte[lenth];
                Marshal.Copy(hred, bmpDataRed, 0, lenth);
                Marshal.Copy(hgreen, bmpDataGreen, 0, lenth);
                Marshal.Copy(hblue, bmpDataBlue, 0, lenth);
                //组合到一起
                byte[] bmpData = new byte[bitmapData.Stride * bitmapData.Height];
                Console.WriteLine(bitmapData.Stride);
                Console.WriteLine(bitmapData.Width);
                for (int i = 0; i < bitmapData.Height; i++)
                {
                    for (int j = 0; j < bitmapData.Width; j++)
                    {
                        bmpData[(i * bitmapData.Stride + j * 3)] = bmpDataBlue[i * bitmapData.Width + j];
                        bmpData[(i * bitmapData.Stride + j * 3) + 1] = bmpDataGreen[i * bitmapData.Width + j];
                        bmpData[(i * bitmapData.Stride + j * 3) + 2] = bmpDataRed[i * bitmapData.Width + j];
                    }
                }
                // 获取bmpData的内存起始位置
                IntPtr iptr = bitmapData.Scan0;
                //用Marshal的Copy方法，将刚才得到的内存字节数组复制到BitmapData中
                Marshal.Copy(bmpData, 0, iptr, lenth * 3);  
                res.UnlockBits(bitmapData);
                return res;
            }
            else
            {
                throw new Exception("The PixelFormat of bitmap is higher than 4!");
            }
        }

        //Byte[]转HImage
        public HObject byte_to_hobject(byte[] image_byte, int height, int width, int channel)
        {

            //新建一个指针，用于生成halcon图像
            IntPtr pixelPointer = Marshal.AllocHGlobal(height * width * channel);
            //拷贝数据
            Marshal.Copy(image_byte, 0, pixelPointer, height * width * channel);
            //新建halcon图像
            HObject halcon_image = new HObject();
            //不同通道数使用不同创造方法
            if (channel == 1) //判断8位、24位、32位
            {
                //单通道图创建方法
                HOperatorSet.GenImage1(out halcon_image, "byte", width, height, pixelPointer);
            }
            else if (channel == 3)
            {
                //三通道图创建方法
                HOperatorSet.GenImageInterleaved(out halcon_image, pixelPointer, "bgr", width, height, -1, "byte", 0, 0, 0, 0, -1, 0);
            }
            else
            {
                throw new Exception("The PixelFormat of bitmap is higher than 4!");
            }

            return halcon_image;
        }

        //HImage转成byte数组
        public byte[] hobject_to_byte(HObject halcon_image , out int out_width , out int out_height , out int out_channel)
        {
            HTuple channelNum = new HTuple();
            HOperatorSet.CountChannels(halcon_image, out channelNum);
            out_channel = channelNum;
            if (channelNum == 1) //判断8位、24位、32位
            {
                HTuple hpoint, type, width, height;
                HOperatorSet.GetImagePointer1(halcon_image, out hpoint, out type, out width, out height);

                out_width = width;
                out_height = height;

                Bitmap res = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

                const int Alpha = 255;
                //调色盘？
                ColorPalette pal = res.Palette;
                for (int i = 0; i <= 255; i++)
                {
                    pal.Entries[i] = Color.FromArgb(Alpha, i, i, i);
                }
                res.Palette = pal;

                BitmapData bitmapData = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
                int lenth = width * height;
                byte[] bmpData = new byte[lenth];
                Marshal.Copy(hpoint, bmpData, 0, lenth);
                res.UnlockBits(bitmapData);
                return bmpData;
            }
            else if (channelNum == 3 || channelNum == 4)
            {
                HTuple hred, hgreen, hblue, type, width, height;
                HOperatorSet.GetImagePointer3(halcon_image, out hred, out hgreen, out hblue, out type, out width, out height);

                out_width = width;
                out_height = height;

                Bitmap res = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                BitmapData bitmapData = res.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                int lenth = width * height;
                byte[] bmpDataRed = new byte[lenth];
                byte[] bmpDataGreen = new byte[lenth];
                byte[] bmpDataBlue = new byte[lenth];
                Marshal.Copy(hred, bmpDataRed, 0, lenth);
                Marshal.Copy(hgreen, bmpDataGreen, 0, lenth);
                Marshal.Copy(hblue, bmpDataBlue, 0, lenth);

                byte[] bmpData = new byte[bitmapData.Stride * bitmapData.Height];
                for (int i = 0; i < bitmapData.Height; i++)
                {
                    for (int j = 0; j < bitmapData.Width; j++)
                    {
                        bmpData[(i * bitmapData.Stride + j * 3)] = bmpDataBlue[i * bitmapData.Width + j];
                        bmpData[(i * bitmapData.Stride + j * 3) + 1] = bmpDataGreen[i * bitmapData.Width + j];
                        bmpData[(i * bitmapData.Stride + j * 3) + 2] = bmpDataRed[i * bitmapData.Width + j];
                    }
                }
                res.UnlockBits(bitmapData);
                return bmpData;
            }
            else
            {
                throw new Exception("The PixelFormat of bitmap is higher than 4!");
            }
        }

        //HObject转成aqimage
        public void hobject_to_aqimg(ref HObject hobject, ref aqrose.aidi_vision.Image aidi_image)
        {
            //先将bitmap转成byte[]
            int width, height,channel_number;
            byte[] image_bytes = hobject_to_byte(hobject,out width,out height,out channel_number);
            //byte[]转成AIDI的Image
            aidi_image.from_chars(image_bytes, height, width, channel_number);
        }

        //aqimage转成bitmap
        public HObject aqimg_to_hobject(ref aqrose.aidi_vision.Image aidi_image)
        {
            //先将aqimage转成byte[]
            byte[] image_bytes = new byte[aidi_image.data_byte_size()];
            aidi_image.to_chars(image_bytes, aidi_image.data_byte_size());
            //byte[]转成bitmap
            return byte_to_hobject(image_bytes, aidi_image.width(), aidi_image.height(), aidi_image.total_channels());
        }
    }
}