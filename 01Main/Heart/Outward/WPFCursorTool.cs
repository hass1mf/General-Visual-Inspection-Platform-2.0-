using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
//WPFCursorTool类自己独立，无需其他引用
//WPFCursorTool类用于实现一些光标事件
//WPFCursorTool类被ToolBoxExt.xaml.cs调用  m_DragCursor = WPFCursorTool.CreateCursor(200, 30, 13, ImageTool.ImageSourceToBitmap(toolBoxExtNode.IconImage), 24, toolBoxExtNode.Name); 第52行

namespace Heart.Outward
{
    public class WPFCursorTool
    {
        /// <summary>
        /// 自定光标
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="ico">图标</param>
        /// <param name="imageSize">图标大小</param>
        /// <param name="text">文本</param>
        /// <returns></returns>
        public static Cursor CreateCursor(int width, int height, float fontSize, Bitmap ico, int imageSize, string text)
        {

            Bitmap m_Buff = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(m_Buff))
            {
                graphics.FillRectangle(new SolidBrush(Color.Transparent), 0, 0, width, height);

                using (System.Drawing.Font font = new System.Drawing.Font("宋体", fontSize, System.Drawing.FontStyle.Regular))
                {
                    using (SolidBrush brush = new SolidBrush(Color.Blue))
                    {
                        graphics.DrawString(text, font, brush, imageSize + 10, (height - fontSize) / 2);
                    }
                }

                graphics.DrawImage(ico, 0, (height - imageSize) / 2, imageSize, imageSize);
            }

            return CreateCursor(m_Buff, 0, 0);
        }


        private static Cursor CreateCursor(Bitmap bm, uint xHotSpot = 0, uint yHotSpot = 0)
        {
            Cursor ret = null;

            if (bm == null)
            {
                return ret;
            }

            try
            {
                ret = InternalCreateCursor(bm, xHotSpot, yHotSpot);
            }
            catch (Exception)
            {
                ret = null;
            }

            return ret;
        }

        private static Cursor InternalCreateCursor(Bitmap bitmap, uint xHotSpot, uint yHotSpot)
        {
            var iconInfo = new NativeMethods.IconInfo();
            NativeMethods.GetIconInfo(bitmap.GetHicon(), ref iconInfo);

            iconInfo.xHotspot = xHotSpot;//焦点x轴坐标
            iconInfo.yHotspot = yHotSpot;//焦点y轴坐标
            iconInfo.fIcon = false;//设置鼠标

            SafeIconHandle cursorHandle = NativeMethods.CreateIconIndirect(ref iconInfo);
            return CursorInteropHelper.Create(cursorHandle);
        }

    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeIconHandle()
            : base(true)
        {
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        {
            return NativeMethods.DestroyIcon(handle);
        }
    }

    public static class NativeMethods
    {
        public struct IconInfo
        {
            public bool fIcon;
            public uint xHotspot;
            public uint yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        public static extern SafeIconHandle CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);
    }
}
