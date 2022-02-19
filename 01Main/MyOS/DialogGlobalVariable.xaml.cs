using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Heart.Inward;

namespace MyOS
{
    /// <summary>
    /// DialogGlobalVariable.xaml 的交互逻辑
    /// </summary>
    public partial class DialogGlobalVariable : Window
    {
        public ObservableCollection<VariableInfo> GlobalVariableInfoCollection;
        public DialogGlobalVariable(List<VariableInfo> gvList)
        {
            InitializeComponent();
            GlobalVariableInfoCollection = new ObservableCollection<VariableInfo>(gvList);
            _dataGrid.ItemsSource = GlobalVariableInfoCollection;
        }

        private void _buttonAddInt_Click(object sender, RoutedEventArgs e)
        {
            AddingNewItem("int");
        }

        private void _buttonAddDouble_Click(object sender, RoutedEventArgs e)
        {
            AddingNewItem("double");
        }

        private void _buttonAddString_Click(object sender, RoutedEventArgs e)
        {
            AddingNewItem("string");
        }

        private void _buttonAddBool_Click(object sender, RoutedEventArgs e)
        {
            AddingNewItem("bool");
        }

        private void _buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            int index = _dataGrid.SelectedIndex;

            if (index < 0)
            {
                return;
            }

            GlobalVariableInfoCollection.RemoveAt(index);

            _dataGrid.Items.Refresh();
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

        private void _dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(_dataGrid);
            IInputElement obj = _dataGrid.InputHitTest(pt);
            DependencyObject target = obj as DependencyObject;

            while (target != null)
            {
                if (target is DataGridCell)
                {
                    DataGridCell dCell = target as DataGridCell;
                    DataGridColumn dColumn = dCell.Column;
                    string headerStr = dColumn.Header as string;
                    if ((headerStr.Trim() == "值") || (headerStr.Trim() == "名称") || (headerStr.Trim() == "备注"))
                    {
                        dCell.IsEditing = true;
                    }
                    break;
                }
                target = VisualTreeHelper.GetParent(target);
            }
        }

        private void _dataGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(_dataGrid);
            IInputElement obj = _dataGrid.InputHitTest(pt);
            DependencyObject target = obj as DependencyObject;

            while (target != null)
            {
                if (target is DataGridRow)
                {
                    (target as DataGridRow).IsSelected = true;
                    return;
                }
                target = VisualTreeHelper.GetParent(target);
            }

            _dataGrid.CommitEdit();
            _dataGrid.UnselectAll();
        }

        private void _dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string newValue = (e.EditingElement as TextBox).Text;

            int row = e.Row.GetIndex();
            int col = e.Column.DisplayIndex;

            bool IsUpdate = true;
            if (col == 2)
            {
                switch (GlobalVariableInfoCollection[row].vType)
                {
                    case "int":
                        IsUpdate = Regex.IsMatch(newValue, @"^[+-]?\d*$");
                        break;
                    case "double":
                        IsUpdate = Regex.IsMatch(newValue, @"^[+-]?\d*[.]?\d*$");
                        break;
                    case "bool":
                        if (newValue != "false" && newValue != "true")
                        {
                            IsUpdate = false;
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (col == 1)
            {
                List<string> vNameList = GlobalVariableInfoCollection.Select(c => c.vName).ToList();
                if (vNameList.Contains(newValue))
                {
                    IsUpdate = false;
                }
            }

            if (IsUpdate)
            {
                switch (col)
                {
                    case 1:
                        GlobalVariableInfoCollection[row].vName = newValue;
                        break;
                    case 2:
                        GlobalVariableInfoCollection[row].vValue = newValue;
                        break;
                    case 3:
                        GlobalVariableInfoCollection[row].vRemark = newValue;
                        break;
                    default:
                        break;
                }

            }

            _dataGrid.Items.Refresh();
        }

        public void AddingNewItem(string typeStr)
        {
            List<string> vNameList = GlobalVariableInfoCollection.Select(c => c.vName).ToList();

            string vName = "value";
            for (int i = 0; i <= vNameList.Count; ++i)
            {
                if (vNameList.FindIndex(c => c == vName + i.ToString()) < 0)
                {
                    vName += i.ToString();
                    break;
                }
            }

            string initValue = "";
            switch (typeStr)
            {
                case "int":
                    initValue = "0";
                    break;
                case "double":
                    initValue = "0";
                    break;
                case "string":
                    initValue = "";
                    break;
                case "bool":
                    initValue = "false";
                    break;
            }

            VariableInfo gVariable = new VariableInfo(typeStr, vName, initValue, "");

            GlobalVariableInfoCollection.Add(gVariable);
            _dataGrid.Items.Refresh();
        }
    }

}
