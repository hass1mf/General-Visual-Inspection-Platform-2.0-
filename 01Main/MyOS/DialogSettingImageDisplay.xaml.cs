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

namespace MyOS
{
    /// <summary>
    /// DialogSettingImageDisplay.xaml 的交互逻辑
    /// </summary>
    public partial class DialogSettingImageDisplay : Window
    {
        public int CanvasNumber { get; set; }
        public DialogSettingImageDisplay()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_radioButton1.IsChecked == true)
            {
                CanvasNumber = 1;
            }
            else if (_radioButton2.IsChecked == true)
            {
                CanvasNumber = 2;
            }
            else if (_radioButton4.IsChecked == true)
            {
                CanvasNumber = 4;
            }
            else if (_radioButton6.IsChecked == true)
            {
                CanvasNumber = 6;
            }
            DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            switch (CanvasNumber)
            {
                case 1:
                    _radioButton1.IsChecked = true;
                    break;
                case 2:
                    _radioButton2.IsChecked = true;
                    break;
                case 4:
                    _radioButton4.IsChecked = true;
                    break;
                case 6:
                    _radioButton6.IsChecked = true;
                    break;
            }
        }
    }
}
