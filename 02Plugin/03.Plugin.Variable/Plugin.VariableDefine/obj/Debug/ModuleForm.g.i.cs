#pragma checksum "..\..\ModuleForm.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "064596BB16A02FCFEE098EB1BC5CDA47C57311A6738FCB746D6FD0E0E5548933"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using Heart.Inward;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Converters;
using MaterialDesignThemes.Wpf.Transitions;
using Plugin.VariableDefine;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Plugin.VariableDefine {
    
    
    /// <summary>
    /// ModuleForm
    /// </summary>
    public partial class ModuleForm : Heart.Inward.ModuleFormBase, System.Windows.Markup.IComponentConnector {
        
        
        #line 33 "..\..\ModuleForm.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid _dataGrid;
        
        #line default
        #line hidden
        
        
        #line 59 "..\..\ModuleForm.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button _buttonAddInt;
        
        #line default
        #line hidden
        
        
        #line 60 "..\..\ModuleForm.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button _buttonAddDouble;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\ModuleForm.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button _buttonAddString;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\ModuleForm.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button _buttonAddBool;
        
        #line default
        #line hidden
        
        
        #line 63 "..\..\ModuleForm.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button _buttonDelete;
        
        #line default
        #line hidden
        
        
        #line 66 "..\..\ModuleForm.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Heart.Inward.ModuleFormBaseControl _baseControl;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Plugin.VariableDefine;component/moduleform.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\ModuleForm.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this._dataGrid = ((System.Windows.Controls.DataGrid)(target));
            
            #line 33 "..\..\ModuleForm.xaml"
            this._dataGrid.MouseDoubleClick += new System.Windows.Input.MouseButtonEventHandler(this._dataGrid_MouseDoubleClick);
            
            #line default
            #line hidden
            
            #line 33 "..\..\ModuleForm.xaml"
            this._dataGrid.CellEditEnding += new System.EventHandler<System.Windows.Controls.DataGridCellEditEndingEventArgs>(this._dataGrid_CellEditEnding);
            
            #line default
            #line hidden
            
            #line 33 "..\..\ModuleForm.xaml"
            this._dataGrid.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this._dataGrid_MouseDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this._buttonAddInt = ((System.Windows.Controls.Button)(target));
            
            #line 59 "..\..\ModuleForm.xaml"
            this._buttonAddInt.Click += new System.Windows.RoutedEventHandler(this._buttonAddInt_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this._buttonAddDouble = ((System.Windows.Controls.Button)(target));
            
            #line 60 "..\..\ModuleForm.xaml"
            this._buttonAddDouble.Click += new System.Windows.RoutedEventHandler(this._buttonAddDouble_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this._buttonAddString = ((System.Windows.Controls.Button)(target));
            
            #line 61 "..\..\ModuleForm.xaml"
            this._buttonAddString.Click += new System.Windows.RoutedEventHandler(this._buttonAddString_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this._buttonAddBool = ((System.Windows.Controls.Button)(target));
            
            #line 62 "..\..\ModuleForm.xaml"
            this._buttonAddBool.Click += new System.Windows.RoutedEventHandler(this._buttonAddBool_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this._buttonDelete = ((System.Windows.Controls.Button)(target));
            
            #line 63 "..\..\ModuleForm.xaml"
            this._buttonDelete.Click += new System.Windows.RoutedEventHandler(this._buttonDelete_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this._baseControl = ((Heart.Inward.ModuleFormBaseControl)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

