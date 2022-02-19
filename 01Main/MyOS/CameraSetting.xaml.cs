using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AcqDevice;    //采集设备--相机库
using HalconDotNet;
using Heart.Inward;

namespace MyOS
{
    /// <summary>
    /// CameraSetting.xaml 的交互逻辑
    /// </summary>
    public partial class CameraSetting : Window
    {

        #region 窗体初始化

        private List<CamInfo> _CamInfoList = new List<CamInfo>();

        public CameraSetting()
        {
            InitializeComponent();
            InitDeviceModel();  //初始化设备型号列表下拉框
        }

        /// <summary>
        /// 窗体对象实例
        /// </summary>
        private static CameraSetting _instance;
        internal static CameraSetting Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CameraSetting();
                return CameraSetting._instance;
            }
        }

        /// <summary>
        /// 窗体初始化加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// 窗体每次显示都触发刷新ui信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(Instance.Visibility == Visibility.Visible)
            {
                DeviceList.ItemsSource = Solution.g_AcqDeviceList;   //dgv_DeviceList 下面的设备列表
                DeviceList.Items.Refresh();     //刷新ui界面
            }
        }

        #endregion


        #region 插件UI事件

        /// <summary>
        /// 设备选择--设备型号--下拉框切换选择相机品牌
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _DeviceModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Str_Device = _DeviceModel.SelectedValue.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
            if (Str_Device == "")   return;
            _DeviceList.ItemsSource = null;     //如果确实有切换相机类型，就清空设备信号列表
            _CamInfoList = null;
            switch ((DeviceType)Enum.Parse(typeof(DeviceType), Str_Device))    //DeviceType 枚举的采集设备列表
            {
                case AcqDevice.DeviceType.USB相机:
                    AcqDevice.AcqUsbCamera.SearchCameras(out _CamInfoList);
                    break;
                case AcqDevice.DeviceType.巴斯勒相机:
                    //AcqDevice.AcqBasler.SearchCameras(out _CamInfoList);
                    break;
                case AcqDevice.DeviceType.海康相机:
                    //AcqDevice.AcqHKVision.SearchCameras(out _CamInfoList);
                    break;
            }
            if (_CamInfoList == null)   return;
            List<string> strList = new List<string>();
            foreach (CamInfo item in _CamInfoList)
            {
                strList.Add(item.m_UniqueName);     //m_UniqueName是相机的名称
            }
            _DeviceList.ItemsSource = strList;  //ui的连接使用新的List<string> strList
            if (_DeviceList.ItemsSource != null) _DeviceList.SelectedIndex = 0;     //如果当前相机类型识别出来有相机设备，就选择第一项
        }

        /// <summary>
        /// 设备选择--添加到列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _addCmeraToList_Click(object sender, RoutedEventArgs e)
        {
            AcqAreaDeviceBase m_AcqDevice;
            if (_DeviceList.SelectedIndex < 0)  //_DeviceList设备列表，这里有选到，就是有相机
            {
                MessageBox.Show("请选择设备！");
                return;
            }
            int index = Solution.g_AcqDeviceList.FindIndex(c => c.m_SerialNo == _CamInfoList[_DeviceList.SelectedIndex].m_SerialNO);
            if (index >= 0)
            {
                MessageBox.Show("该设备已经添加列表");
                return;
            }
            switch ((DeviceType)Enum.Parse(typeof(DeviceType), _DeviceModel.Text))
            {
                case AcqDevice.DeviceType.USB相机:
                    if (_DeviceList.SelectedIndex >= 0)
                    {
                        m_AcqDevice = new AcqUsbCamera(AcqDevice.DeviceType.USB相机);
                        m_AcqDevice.m_SerialNo = _CamInfoList[_DeviceList.SelectedIndex].m_SerialNO;
                        m_AcqDevice.m_UniqueName = _CamInfoList[_DeviceList.SelectedIndex].m_UniqueName;
                        Solution.g_AcqDeviceList.Add(m_AcqDevice);
                    }
                    break;
                ////case AcqDevice.DeviceType.巴斯勒相机:
                ////    m_AcqDevice = new AcqBasler(AcqDevice.DeviceType.Basler);
                ////    m_AcqDevice.m_UniqueLabel = _CamInfoList[cmb_DeviceName.SelectedIndex].m_UniqueName;
                ////    m_AcqDevice.m_DevDirExt = _CamInfoList[cmb_DeviceName.SelectedIndex].m_MaskName;
                ////    m_AcqDevice.m_SerialNo = _CamInfoList[cmb_DeviceName.SelectedIndex].m_SerialNO;
                ////    HMeasureSYS.g_AcqDeviceList.Add(m_AcqDevice);
                ////    break;
                ////case AcqDevice.DeviceType.海康相机:
                ////    m_AcqDevice = new AcqHKVision(AcqDevice.DeviceType.HKVision);
                ////    m_AcqDevice.m_UniqueLabel = _CamInfoList[cmb_DeviceName.SelectedIndex].m_UniqueName;
                ////    m_AcqDevice.m_DevDirExt = _CamInfoList[cmb_DeviceName.SelectedIndex].m_MaskName;
                ////    m_AcqDevice.m_SerialNo = _CamInfoList[cmb_DeviceName.SelectedIndex].m_SerialNO;
                ////    ((AcqHKVision)m_AcqDevice).extInfo = _CamInfoList[cmb_DeviceName.SelectedIndex].mExtInfo;
                ////    HMeasureSYS.g_AcqDeviceList.Add(m_AcqDevice);
                ////    break;
            }
            DeviceList.ItemsSource = Solution.g_AcqDeviceList;   //dgv_DeviceList 下面的设备列表
            DeviceList.Items.Refresh();     //刷新ui界面
            DeviceList.SelectedIndex = DeviceList.Items.Count - 1;  //这里是希望新加进来的相机项可以被选中
        }

        /// <summary>
        /// 设备列表--设备栏内容点击切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AcqAreaDeviceBase m_AcqDevice = DeviceList.SelectedItem as AcqAreaDeviceBase;       //因为ui和数据绑定，所以通过ui的选择item，应该可以拿到对应的AcqDeviceBase info类元素信息
            if (DeviceList.SelectedItem == null)
            {
                return;
            }

            _Connect.IsEnabled = !m_AcqDevice.m_bConnected;
            _disConnect.IsEnabled = m_AcqDevice.m_bConnected;
            _TriggerMode.IsEnabled = _ExposureTime.IsEnabled = _Gain.IsEnabled = !m_AcqDevice.m_bConnected;     //如果设备连接上了，相关的设置参数也要屏蔽掉显示able
            _cameraName.Text = m_AcqDevice.m_DeviceID;                                                          //采集设备名
            _ExposureTime.Text = m_AcqDevice._ExposeTime.ToString();                                            //曝光时间
            _TriggerMode.Text = Enum.Parse(typeof(TRIGGER_MODE), m_AcqDevice._TrigerMode.ToString()).ToString();  //触发模式
            _Gain.Text = m_AcqDevice._Gain.ToString();                                                          //增益
            m_AcqDevice = null;
        }

        /// <summary>
        /// 设备列表--设备栏右键菜单--删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteItem_Click(object sender, RoutedEventArgs e)
        {
            AcqAreaDeviceBase m_AcqDevice = DeviceList.SelectedItem as AcqAreaDeviceBase;     //因为ui和数据绑定，所以通过ui的选择item，应该可以拿到对应的AcqDeviceBase info类元素信息
            if (m_AcqDevice.m_bConnected) { MessageBox.Show("请先关闭相机"); return; }
            _cameraName.Text = "";  //清空当前相机的名称
            if (Solution.g_AcqDeviceList.Count > 0 && DeviceList.SelectedIndex != -1)
            {
                Solution.g_AcqDeviceList.Remove(m_AcqDevice);   //尝试直接在g_AcqDeviceList总采集设备列表内移除info项
            }
            if (DeviceList.Items.Count == 0)    //如果删除后了，当前采集设备里面没有内容，就屏蔽关掉连接按钮
            {
                _Connect.IsEnabled = false;
            }
            DeviceList.ItemsSource = Solution.g_AcqDeviceList;
            DeviceList.Items.Refresh();    //刷新ui界面
            m_AcqDevice = null;
        }

        /// <summary>
        /// 设备列表--相机--连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _Connect_Click(object sender, RoutedEventArgs e)
        {
            AcqDeviceBase m_AcqDevice = DeviceList.SelectedItem as AcqDeviceBase;       //因为ui和数据绑定，所以通过ui的选择item，应该可以拿到对应的AcqDeviceBase info类元素信息
            if (DeviceList.SelectedItem != null)
            {
                m_AcqDevice.ConnectDev();   //建立设备连接
                DeviceList.ItemsSource = Solution.g_AcqDeviceList;
                DeviceList.Items.Refresh();
                _Connect.IsEnabled = !m_AcqDevice.m_bConnected;
                _disConnect.IsEnabled = m_AcqDevice.m_bConnected;
                _TriggerMode.IsEnabled = _ExposureTime.IsEnabled = _Gain.IsEnabled = !m_AcqDevice.m_bConnected;     //如果设备连接上了，相关的设置参数也要屏蔽掉显示able
            }          
        }

        /// <summary>
        /// 设备列表--相机--断开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _disConnect_Click(object sender, RoutedEventArgs e)
        {
            AcqDeviceBase m_AcqDevice = DeviceList.SelectedItem as AcqDeviceBase;
            if (DeviceList.SelectedItem != null)
            {
                m_AcqDevice.DisConnectDev();   //建立设备连接
                DeviceList.ItemsSource = Solution.g_AcqDeviceList;
                DeviceList.Items.Refresh();
                _Connect.IsEnabled = !m_AcqDevice.m_bConnected;
                _disConnect.IsEnabled = m_AcqDevice.m_bConnected;
                _TriggerMode.IsEnabled = _ExposureTime.IsEnabled = _Gain.IsEnabled = !m_AcqDevice.m_bConnected;     //如果设备断开连接了，相关的设置参数也要屏蔽掉显示able
            }
        }

        /// <summary>
        /// 设备列表--相机--采集图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _capture_Click(object sender, RoutedEventArgs e)
        {
            AcqDeviceBase m_AcqDevice = DeviceList.SelectedItem as AcqDeviceBase;
            if (DeviceList.SelectedItem != null)
            {
                if (m_AcqDevice.CaptureImage(true))
                {
                    _hWinDisplay.Image = m_AcqDevice.m_Image;
                }
            }
        }

        /// <summary>
        /// 按钮--确定--关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _Confirm_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();    //唯一的实例一直都存在，所以Hide隐藏即可
        }

        #endregion


        #region 窗体退出与关闭

        /// <summary>
        /// 窗体关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //通讯设置是单一的实例，所以窗体关闭时只能隐藏窗体。不能this.close()
            e.Cancel = true;
            Hide();
        }

        /// <summary>
        /// 初始化设备型号列表下拉框
        /// </summary>
        private void InitDeviceModel()
        {
            List<string> listName = new List<string>();
            foreach (string s in Enum.GetNames(typeof(DeviceType)))     //DeviceType 枚举的相机设备品牌
            {
                listName.Add(s);
            }
            _DeviceModel.ItemsSource = listName;   //cmb_DeviceType相机下拉选择框
        }

        #endregion


    }
}
