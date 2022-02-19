using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//这里定义了一些相机的基本定义 该类单独存在

namespace AcqDevice
{
    /// <summary>
    /// 相机设备品牌
    /// </summary>
    [Serializable]
    public enum DeviceType
    {
        USB相机,
        巴斯勒相机,
        海康相机,
        //Jai驱动相机,
    }

    /// <summary>
    /// 图像位深
    /// </summary>
    [Serializable]
    public enum PIXEL_DEPTH
    {
        PIXEL_DEPTH_8,          ///8位
        PIXEL_DEPTH_12,         ///12位
        PIXEL_DEPTH_16          ///16位
    }

    /// <summary>
    /// 图像彩色信息
    /// </summary>
    [Serializable]
    public enum PIXEL_TYPE
    {
        PIX_GRAY8,              ///灰度图8位，定义时可以不要位深信息
        PIX_RGB8                ///彩色图8位
    }

    /// <summary>
    /// 线扫模式
    /// </summary>
    [Serializable]
    public enum SENSOR_TYPE
    {
        Area,
        Line
    }

    /// <summary>
    /// 触发模式
    /// </summary>
    [Serializable]
    public enum TRIGGER_MODE
    {
        软件触发 = 0,
        硬件触发 = 1,
        上升沿 = 2,
        下降沿 = 3
    }

    /// <summary>
    /// 曝光模式
    /// </summary>
    [Serializable]
    public enum EXPOSURE_MODE
    {
        内曝光 = 0, //内部设置曝光时间
        外曝光,    //电平信号设置曝光时间
    }

    [Serializable]
    public enum IMG_ADJUST
    {
        None = 0,
        垂直镜像,
        水平镜像,
        顺时针90度,
        逆时针90度,
        旋转180度
    }
}
