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
using MyOS.Common.communacation;    //通信配置
using System.IO.Ports;              //串口相关
using System.Text.RegularExpressions;

namespace MyOS
{
    /// <summary>
    /// CommunicationManagement.xaml 的交互逻辑
    /// </summary>
    public partial class CommunicationManagement : Window
    {

        #region 窗体初始化

        /// <summary>
        /// 当前窗体类型
        /// </summary>
        private static CurForm curForm = CurForm.None;
        /// <summary>
        /// 在添加设备到列表时，不需要重新加载设备
        /// </summary>
        internal static bool cancel = false;
        /// <summary>
        /// 当前通讯对象的key
        /// </summary>
        private string m_CurKey = "";//当前通讯对象的key
        //16进制接收发送
        private bool IsSendByHex = false;
        private bool IsReceivedByHex = false;

        public CommunicationManagement()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 枚举定义当前通讯类型
        /// </summary>
        public enum CurForm
        {
            None,
            TCPSever,
            TCPClient,
            Serial,
            UPD
        }

        /// <summary>
        /// 窗体对象实例
        /// </summary>
        private static CommunicationManagement _instance;
        internal static CommunicationManagement Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CommunicationManagement();
                return CommunicationManagement._instance;
            }
        }
        private delegate void SetTextCallback(string text);     //委托，用于显示数据
        /// <summary>
        /// 窗体初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TCPServer.Visibility = Visibility.Visible;
            TCPClient.Visibility = Visibility.Collapsed;
            Serial.Visibility = Visibility.Collapsed;
            string[] Ports = SerialPort.GetPortNames();
            _SerialPort.Items.Clear();
            foreach (string i in Ports)
            {
                _SerialPort.Items.Add(i);
            }
            if (_SerialPort.Items.Count > 0)
            {
                _SerialPort.SelectedIndex = 0;
            }
            _CommunicationList.CanUserSortColumns = false;
        }

        #endregion


        #region 通讯设备列表

        /// <summary>
        /// 通讯设备列表--切换点击选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _CommunicationList_SelectionChanged(object sender, SelectionChangedEventArgs e)    //通讯列表选择改变函数
        {
            if (_CommunicationList.SelectedItem == null)
            {
                return;
            }
            //清空所有的接受区和发送区
            _TCPServerSendData.Text = "";
            _TCPServerReceiveData.Text = "";
            _TCPClientSendData.Text = "";
            _TCPClientReceiveData.Text = "";
            _SerialSendData.Text = "";
            _SerialReceiveData.Text = "";
            ECommunacation info = _CommunicationList.SelectedItem as ECommunacation;                            //备注下！！！ 
            _remark.Content = info.Key;
            m_CurKey = info.Key;
            checkbox_IsSendByHex.IsChecked = EComManageer.GetECommunacation(m_CurKey).IsSendByHex;
            checkbox_IsReceivedByHex.IsChecked = EComManageer.GetECommunacation(m_CurKey).IsReceivedByHex;
            switch (Regex.Replace(info.Key, @"\d", ""))
            {
                case "TCP服务端":
                    TCPServer.Visibility = Visibility.Visible;
                    TCPClient.Visibility = Visibility.Collapsed;
                    Serial.Visibility = Visibility.Collapsed;
                    //判断选中的通讯设备是否已经打开
                    if (info.IsConnected) { _TCPServerLocalport.IsEnabled = false; }
                    else { _TCPServerLocalport.IsEnabled = true; }
                    //复原通讯参数
                    _TCPServerLocalport.Text = info.LocalPort.ToString();  //这里不是用了value，直接用text效果不知道如何
                    _remark.Content = info.Key;
                    break;
                case "TCP客户端":
                    TCPClient.Visibility = Visibility.Visible;
                    TCPServer.Visibility = Visibility.Collapsed;
                    Serial.Visibility = Visibility.Collapsed;
                    //判断选中的通讯设备是否已经打开
                    if (info.IsConnected) { _TCPClientTargetPort.IsEnabled = false; _TCPClientTargetIP.IsEnabled = false; }    //判断当前的通讯是否链接，链接了就要屏蔽ui输入
                    else { _TCPClientTargetPort.IsEnabled = true; _TCPClientTargetIP.IsEnabled = true; }
                    //复原通讯参数
                    _TCPClientTargetIP.Text = info.RemoteIP;
                    _TCPClientTargetPort.Text = info.LocalPort.ToString();  //这里不是用了value，直接用text效果不知道如何
                    _remark.Content = info.Key;
                    break;
                case "串口":
                    Serial.Visibility = Visibility.Visible;
                    TCPServer.Visibility = Visibility.Collapsed;
                    TCPClient.Visibility = Visibility.Collapsed;
                    //判断选中的通讯设备是否已经打开
                    if (info.IsConnected) { serialInfo.IsEnabled = false; }  
                    else { serialInfo.IsEnabled = true; }
                    //复原通讯参数
                    _SerialPort.Text = info.PortName;
                    _SerialBaudRate.Text = info.BaudRate;
                    _SerialCheckDigit.Text = info.Parity;
                    _SerialDataDigit.Text = info.DataBits;
                    _SerialStopDigit.Text = info.StopBits;
                    _remark.Content = info.Key;
                    break;
            }
        }

        /// <summary>
        /// 通讯设备列表--点击选项更改状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _CommunicationList_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

        }

        /// <summary>
        /// 通讯设备列表--勾选栏--设备开启
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _connect_click(object sender, RoutedEventArgs e)
        {
            ECommunacation info = _CommunicationList.SelectedItem as ECommunacation;
            if (info == null) { return; }
            ECommunacation eCommunacation = EComManageer.GetECommunacation(info.Key);   //拿出该名称的端口
            m_CurKey = info.Key;
            switch (Regex.Replace(info.Key, @"\d", "")) //正则表达式可以实现把数字替换成""空
            {
                case "TCP服务端":
                    if (EComManageer.GetECommunacation(m_CurKey).IsConnected)    //判断当前通讯是否连接
                    {
                        EComManageer.DisConnect(m_CurKey);
                        _TCPServerLocalport.IsEnabled = true;
                    }
                    else
                    {    
                        try
                        {
                            eCommunacation.RemotePort = int.Parse(_TCPServerLocalport.Value.ToString());              //重新设定回端口
                            //UPD对同一端口连接重复可能会报错
                            if (EComManageer.Connect(info.Key))  //如果能打开通讯才改变图标
                            {
                                _TCPServerLocalport.IsEnabled = false;
                            }
                        }
                        catch { }     
                    }
                    _CommunicationList.Items.Refresh();
                    break;
                case "TCP客户端":
                    if (EComManageer.GetECommunacation(m_CurKey).IsConnected)    //判断当前通讯是否连接
                    {
                        EComManageer.DisConnect(m_CurKey);
                        _TCPClientTargetIP.IsEnabled = true;
                        _TCPClientTargetPort.IsEnabled = true;
                    }
                    else
                    {
                        try
                        {
                            eCommunacation.RemoteIP = _TCPClientTargetIP.Text;
                            eCommunacation.RemotePort = int.Parse(_TCPClientTargetPort.Value.ToString());              //重新设定回端口
                            //UPD对同一端口连接重复可能会报错
                            if (EComManageer.Connect(info.Key))  //如果能打开通讯才改变图标
                            {
                                _TCPClientTargetIP.IsEnabled = false;
                                _TCPClientTargetPort.IsEnabled = false;
                            }
                        }
                        catch { }
                    }
                    _CommunicationList.Items.Refresh();
                    break;
                case "串口":
                    if (EComManageer.GetECommunacation(m_CurKey).IsConnected)    //判断当前通讯是否连接
                    {
                        EComManageer.DisConnect(m_CurKey);
                        serialInfo.IsEnabled = true;
                    }
                    else
                    {
                        try
                        {
                            eCommunacation.PortName = _SerialPort.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
                            eCommunacation.BaudRate = _SerialBaudRate.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
                            eCommunacation.Parity = _SerialCheckDigit.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
                            eCommunacation.DataBits = _SerialDataDigit.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
                            eCommunacation.StopBits = _SerialStopDigit.SelectedItem.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
                            //UPD对同一端口连接重复可能会报错
                            if (EComManageer.Connect(info.Key))  //如果能打开通讯才改变图标
                            {
                                serialInfo.IsEnabled = false;   //这里实现了对整个serialInfo区域的操作屏蔽
                            }
                        }
                        catch { }
                    }
                    _CommunicationList.Items.Refresh();
                    break;
            }
            info = null;    //清空
        }

        /// <summary>
        /// 通讯设备列表--勾选栏--设备关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _disconnect_click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 通讯设备列表--右键菜单--删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteItem_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 通讯设备列表--添加设备--4选1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _AddDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_AddDevice.SelectedItem == null)    //获取通讯设备的选择项
            {
                return;     //若当前选择为空则退出
            }
            string StrSelect = _AddDevice.SelectedValue.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
            if (StrSelect == "TCP服务端")        
            {
                m_CurKey = EComManageer.CreateECom(CommunicationModel.TcpServer);//创建tcp服务器
                ECommunacation eCommunacation = EComManageer.GetECommunacation(m_CurKey);
                eCommunacation.ReceiveString += ECommunacation_ReceiveString;
                //设置通讯参数,第一次建立。所以直接赋值
                eCommunacation.RemoteIP = "127.0.0.1";//
                eCommunacation.RemotePort = 8000;
                _TCPClientTargetPort.Value = eCommunacation.RemotePort;     //ui界面上的端口号赋值8000
            }
            else if (StrSelect == "TCP客户端")
            {
                m_CurKey = EComManageer.CreateECom(CommunicationModel.TcpClient);//创建tcp客户端
                ECommunacation eCommunacation = EComManageer.GetECommunacation(m_CurKey);
                eCommunacation.ReceiveString += ECommunacation_ReceiveString;

                //设置通讯参数,第一次建立。所以直接赋值
                eCommunacation.RemoteIP = "127.0.0.1";//
                eCommunacation.RemotePort = 8000;
                //然后将默认值传过去窗体
                _TCPClientTargetIP.Text = eCommunacation.RemoteIP;
                _TCPClientTargetPort.Value = eCommunacation.RemotePort;
            }
            else if (StrSelect == "串口通讯")
            {
                m_CurKey = EComManageer.CreateECom(CommunicationModel.COM);//创建tcp客户端
                ECommunacation eCommunacation = EComManageer.GetECommunacation(m_CurKey);
                eCommunacation.ReceiveString += ECommunacation_ReceiveString;

                //设置通讯参数,第一次建立。所以直接赋值
                eCommunacation.PortName = "COM1";
                eCommunacation.BaudRate = "9600";
                eCommunacation.Parity = "None";
                eCommunacation.DataBits = "8";
                eCommunacation.StopBits = "One";
                //然后将默认值传过去窗体
                _SerialPort.SelectedItem = eCommunacation.PortName;
                _SerialBaudRate.SelectedItem = eCommunacation.BaudRate;
                _SerialCheckDigit.SelectedItem = eCommunacation.Parity;
                _SerialDataDigit.SelectedItem = eCommunacation.DataBits;
                _SerialStopDigit.SelectedItem = eCommunacation.StopBits;
            }
            else if (StrSelect == "UDP通讯")
            {

            }
            _remark.Content = m_CurKey;                                     //ui界面上的备注赋值当前的名字m_CurKey
            _CommunicationList.ItemsSource = EComManageer.GetEcomList();    //获取当前的通讯设备列表，ItemsSource绑定了一个是名称 Key，另一个是状态 IsConnected
            _AddDevice.SelectedValue = "";                                  //使得选择框选空项
        }

        /// <summary>
        /// 通讯列表单击事件,这个函数实现随时点击ui都可以触发进入，意义不大
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void item_gotfocus(object sender, RoutedEventArgs e)   //通讯列表单击事件
        {
            //ECommunacation info = _CommunicationList.SelectedItem as ECommunacation;    //在前面新建通讯时，已经将ui的_CommunicationList与ECommunacation类绑定在一起的，所以这里相当于通过ui直接拿出ECommunacation类的对象
            //var item = (DataGridRow)sender;
            //FrameworkElement frameworkElement = _CommunicationList.Columns[0].GetCellContent(item);
            //if (frameworkElement != null && info != null && info.Key != "")
            //{
            //    switch (Regex.Replace(info.Key, @"\d", ""))
            //    {
            //        case "TCP服务端":
            //            _remark.Content = info.Key;
            //            //DeviceInfo.TCPServerDetails[info.Name] = _TCPServerLocalport.Text;
            //            break;
            //        case "TCP客户端":
            //            _remark.Content = info.Key;
            //            //string[] b = { _TargetIP.Text, _TCPClientTargetPort.Text };
            //            //DeviceInfo.TCPCLientDetails[info.Name] = b;
            //            //b = null;
            //            break;
            //        case "串口通讯":
            //            _remark.Content = info.Key;
            //            //string[] a = { _SerialPort.Text, _BaudRate.Text, _CheckDigit.Text, _DataDigit.Text, _StopDigit.Text };
            //            //DeviceInfo.serialportdetails[info.Name] = a;
            //            //a = null;
            //            break;
            //    }
            //}
        }

        #endregion


        #region 窗体退出与关闭

        /// <summary>
        /// 按钮--关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        /// <summary>
        /// 窗体--关闭事件
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
        /// 窗体每次显示都触发刷新ui信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(Instance.Visibility == Visibility.Visible)
            {
                _CommunicationList.ItemsSource = EComManageer.GetEcomList();    //获取当前的通讯设备列表，ItemsSource绑定了一个是名称 Key，另一个是状态 IsConnected
                _CommunicationList.Items.Refresh();
            }
        }

        #endregion


        #region TCP服务器

        /// <summary>
        /// TCP服务器--发送数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPServerSend_Click(object sender, RoutedEventArgs e)
        {
            if (EComManageer.GetECommunacation(m_CurKey).IsConnected)    //判断当前通讯是否连接
            {
                if (_TCPServerSendData.Text != null)
                {
                    EComManageer.SendStr(m_CurKey, _TCPServerSendData.Text.Trim());
                }
            }
        }

        /// <summary>
        /// TCP服务器--清空发送区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPServerSendClear_Click(object sender, RoutedEventArgs e)
        {
            _TCPServerSendData.Text = "";
        }

        /// <summary>
        /// TCP服务器--清空接收区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPServerReceiveClear_Click(object sender, RoutedEventArgs e)
        {
            _TCPServerReceiveData.Text = "";
        }

        /// <summary>
        /// TCP服务器--端口号编辑改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPServer_PortTextchanged(object sender, TextChangedEventArgs e)
        {

        }

        #endregion


        #region TCP客户端

        /// <summary>
        /// TCP客户端--发送数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPClientSend_Click(object sender, RoutedEventArgs e)
        {
            if (EComManageer.GetECommunacation(m_CurKey).IsConnected)    //判断当前通讯是否连接
            {
                if (_TCPClientSendData.Text != null)
                {
                    EComManageer.SendStr(m_CurKey, _TCPClientSendData.Text.Trim());
                }
            }
        }

        /// <summary>
        /// TCP客户端--清空发送区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPClientSendClear_Click(object sender, RoutedEventArgs e)
        {
            _TCPClientSendData.Text = "";
        }

        /// <summary>
        /// TCP客户端--清空接收区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPClientReceiveClear_Click(object sender, RoutedEventArgs e)
        {
            _TCPClientReceiveData.Text = "";
        }

        /// <summary>
        /// TCP客户端--ip号编辑改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPClient_IpTextChanged(object sender, TextChangedEventArgs e)
        {

        }

        /// <summary>
        /// TCP客户端--端口号编辑改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TCPClient_PortTextChanged(object sender, TextChangedEventArgs e)
        {

        }

        #endregion


        #region 串口通讯

        /// <summary>
        /// 串口通讯--发送数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialSend_Click(object sender, RoutedEventArgs e)
        {
            if (EComManageer.GetECommunacation(m_CurKey).IsConnected)    //判断当前通讯是否连接
            {
                if (_SerialSendData.Text != null)
                {
                    EComManageer.SendStr(m_CurKey, _SerialSendData.Text.Trim());
                }
            }
        }

        /// <summary>
        /// 串口通讯--清空发送区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialSendClear_Click(object sender, RoutedEventArgs e)
        {
            _SerialSendData.Text = "";
        }

        /// <summary>
        /// 串口通讯--清空接收区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialReceiveClear_Click(object sender, RoutedEventArgs e)
        {
            _SerialReceiveData.Text = "";
        }

        /// <summary>
        /// 串口通讯--串口号编辑改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Serial_PortTextChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// 串口通讯--波特率编辑改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Serial_BaudRateTextChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// 串口通讯--检验位编辑改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Serial_CheckDigitTextChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// 串口通讯--数据位编辑改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Serial_DataDigitTextChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// 串口通讯--停止位编辑改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Serial_StopDigitTextChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #endregion


        #region 16进制收发checkbox事件

        /// <summary>
        /// 开启16进制发送数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _checkbox_IsSendByHex(object sender, RoutedEventArgs e)
        {
            if(m_CurKey != "") EComManageer.GetECommunacation(m_CurKey).IsSendByHex = true;
        }

        /// <summary>
        /// 开启16进制发送数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _checkbox_NoSendByHex(object sender, RoutedEventArgs e)
        {
            if (m_CurKey != "") EComManageer.GetECommunacation(m_CurKey).IsSendByHex = false;
        }

        /// <summary>
        /// 关闭16进制显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _checkbox_IsReceivedByHex(object sender, RoutedEventArgs e)
        {
            if (m_CurKey != "") EComManageer.GetECommunacation(m_CurKey).IsReceivedByHex = true;
        }

        /// <summary>
        /// 开启16进制显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _checkbox_NoReceivedByHex(object sender, RoutedEventArgs e)
        {
            if (m_CurKey != "") EComManageer.GetECommunacation(m_CurKey).IsReceivedByHex = false;
        }

        #endregion


        #region 相关辅助函数

        /// <summary>
        /// 接收到数据后的回调事件
        /// </summary>
        /// <param name="str"></param>
        public void ECommunacation_ReceiveString(string str)
        {
            //EComManageer.SendStr(m_CurKey, "这是返回数据" + str);
            SetText(str);   //显示接收的数据，
        }

        /// <summary>
        /// 通过设备栏行信息判断当前通讯是否打开
        /// </summary>
        /// <param name="SelectItem"></param>
        /// <returns></returns>
        public bool GetCheckboxIsChecked(object SelectItem)
        {
            ECommunacation info = (ECommunacation)SelectItem;
            return info.IsConnected;
        }

        /// <summary>
        /// 显示接收内容
        /// </summary>
        /// <param name="text"></param>
        private void SetText(string text)
        {
            //这里是数据底层想对UI进行操作，不能直接操作，所以需要比较线程来输入
            //InvokeRequired需要比较调用线程ID和创建线程ID
            // 如果它们不相同则返回true
            if (m_CurKey.StartsWith("TCP服务端"))
            {
                if (!this._TCPServerReceiveData.CheckAccess())
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Dispatcher.Invoke(d, new object[] { text });
                }
                else
                {
                    this._TCPServerReceiveData.Text += text;
                }
            }
            else if (m_CurKey.StartsWith("TCP客户端"))
            {
                if (!this._TCPClientReceiveData.CheckAccess())
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Dispatcher.Invoke(d, new object[] { text });
                }
                else
                {
                    this._TCPClientReceiveData.Text += text;
                }
            }
            else if (m_CurKey.StartsWith("串口"))
            {
                if (!this._SerialReceiveData.CheckAccess())
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Dispatcher.Invoke(d, new object[] { text });
                }
                else
                {
                    this._SerialReceiveData.Text += text;
                }
            }
        }


        #endregion


    }
}
