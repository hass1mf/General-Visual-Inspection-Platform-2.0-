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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Heart.Outward
{
    /// <summary>
    /// DialogComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class DialogComboBox : Window
    {
        private List<ComboBoxDataItem> m_ComboBoxDataList = new List<ComboBoxDataItem>();
        public DialogComboBox()
        {
            InitializeComponent();
        }

        public void SetTitle(string title)
        {
            Title = title;
        }
        public void SetComboBoxDataList(List<string> dataList)
        {
            int idx = -1;
            foreach(string item in dataList)
            {
                idx++;
                m_ComboBoxDataList.Add(new ComboBoxDataItem { Name = item, Index = idx });            
            }

            _comboBox.ItemsSource = m_ComboBoxDataList;
            _comboBox.DisplayMemberPath = "Name";
            _comboBox.SelectedValuePath = "Index";
            if (_comboBox.Items.Count > 0)
            {
                _comboBox.SelectedIndex = 0;
            }
        }

        public string GetSelectedText()
        {
            if (_comboBox.Items.Count > 0)
            {
                return (_comboBox.SelectedItem as ComboBoxDataItem).Name;
            }
            else
            {
                return "";
            }
                
        }

        private void _buttonSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void _buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

    }

    class ComboBoxDataItem
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }
}
