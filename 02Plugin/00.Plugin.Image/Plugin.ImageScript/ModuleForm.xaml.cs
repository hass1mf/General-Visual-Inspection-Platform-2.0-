using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HalconDotNet;
using Heart.Inward;
using Heart.Outward;
using ScintillaNET;

namespace Plugin.ImageScript
{
    /// <summary>
    /// ModuleForm.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleForm : ModuleFormBase
    {      
        private ModuleObj m_ModuleObj;
        
        public ModuleForm()
        {
            InitializeComponent();               
            
        }

        public override void LoadModule()
        {
            m_ModuleObj = (ModuleObj)ModuleObjBase;
            Title = m_ModuleObj.Info.ModuleName;

            InitScintilla(_scintillaEditor);

            if (m_ModuleObj.m_EProcedureList != null)
            {
                _editProcedureComboBox.ItemsSource = m_ModuleObj.m_EditProcedureNameList;
                _editProcedureComboBox.DisplayMemberPath = "Name";
                _editProcedureComboBox.SelectedValuePath = "Index";

                if (m_ModuleObj.m_EditProcedureNameList.Count > 0)
                {
                    int index = m_ModuleObj.m_EditProcedureNameList.FindIndex(p => p.Name == m_ModuleObj.m_EditProcedureName);
                    if (index >= 0) {
                        _editProcedureComboBox.SelectedIndex = index;
                        m_ModuleObj.m_EditProcedureName = m_ModuleObj.m_EditProcedureNameList[_editProcedureComboBox.SelectedIndex].Name;
                        _scintillaEditor.Text = m_ModuleObj.m_EProcedureList[_editProcedureComboBox.SelectedIndex].Body;
                    }                
                }

                _executeProcedureComboBox.ItemsSource = m_ModuleObj.m_ExecuteProcedureNameList;
                _executeProcedureComboBox.DisplayMemberPath = "Name";
                _executeProcedureComboBox.SelectedValuePath = "Index";

                if (_executeProcedureComboBox.Items.Count > 0)
                {
                    int index = m_ModuleObj.m_ExecuteProcedureNameList.FindIndex(p => p.Name == m_ModuleObj.m_ExecuteProcedureName);
                    if (index >= 0)
                    {
                        _executeProcedureComboBox.SelectedIndex = index;
                        m_ModuleObj.m_ExecuteProcedureName = m_ModuleObj.m_ExecuteProcedureNameList[_executeProcedureComboBox.SelectedIndex].Name;
                    }
                }

                InitSyntaxColoring(_scintillaEditor);

                _inputDataGrid.ItemsSource = m_ModuleObj.m_InputVariableCollection;
                _outputDataGrid.ItemsSource = m_ModuleObj.m_OutputVariableCollection;

                _inputDataGrid.Items.Refresh();
                _outputDataGrid.Items.Refresh();
            }
            
        }

        public override void RunModuleBefore()
        {

        }

        public override void RunModuleAfter()
        {
            _outputDataGrid.Items.Refresh();
        }

        public override void SaveModuleBefore()
        {
            
        }

        private void _importButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择halcon文件";
            openFileDialog.Filter = "halcon文件|*.hdev";
            openFileDialog.FileName = string.Empty;
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                m_ModuleObj.m_EProcedureList = EProcedure.LoadXmlByFile(openFileDialog.FileName);

                InitProcedure();
            }

        }

        private void _exportButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_ModuleObj.m_EProcedureList != null && m_ModuleObj.m_EProcedureList.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();

                sfd.Filter = "halcon文件|*.hdev";

                //设置默认文件类型显示顺序 
                sfd.FilterIndex = 1;

                sfd.InitialDirectory = @"C:\Users\Administrator\Desktop";
                //保存对话框是否记忆上次打开的目录 
                sfd.RestoreDirectory = true;

                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string localFilePath = sfd.FileName.ToString(); 

                    EProcedure.SaveToFile(localFilePath, m_ModuleObj.m_EProcedureList);
                }
            }

        }

        private void _executeProcedureComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_editProcedureComboBox.SelectedIndex == -1) return;

            if (m_ModuleObj.m_ExecuteProcedureName != m_ModuleObj.m_ExecuteProcedureNameList[_executeProcedureComboBox.SelectedIndex].Name)
            {
                m_ModuleObj.m_ExecuteProcedureName = m_ModuleObj.m_ExecuteProcedureNameList[_executeProcedureComboBox.SelectedIndex].Name;
                m_ModuleObj.UpdateInputVariableCollection();
                m_ModuleObj.UpdateOutputVariableCollection();
                _inputDataGrid.Items.Refresh();
                _outputDataGrid.Items.Refresh();
            }
            
        }

        private void _editProcedureComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_editProcedureComboBox.SelectedIndex == -1) return;

            if (m_ModuleObj.m_EditProcedureName != m_ModuleObj.m_EditProcedureNameList[_editProcedureComboBox.SelectedIndex].Name)
            {
                m_ModuleObj.m_EditProcedureName = m_ModuleObj.m_EditProcedureNameList[_editProcedureComboBox.SelectedIndex].Name;
                _scintillaEditor.Text = m_ModuleObj.m_EProcedureList[_editProcedureComboBox.SelectedIndex].Body;
            }
            
        }

        private void _inputDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point pt = e.GetPosition(_inputDataGrid);
            IInputElement obj = _inputDataGrid.InputHitTest(pt);
            DependencyObject target = obj as DependencyObject;

            while (target != null)
            {
                if (target is System.Windows.Controls.DataGridCell)
                {
                    System.Windows.Controls.DataGridCell dCell = target as System.Windows.Controls.DataGridCell;
                    DataGridColumn dColumn = dCell.Column;
                    string headerStr = dColumn.Header as string;

                    if (headerStr.Trim() == "变量链接")
                    {
                        while ((target != null) && !(target is DataGridRow))
                        {
                            target = VisualTreeHelper.GetParent(target);
                        }
                        DataGridRow dRow = target as DataGridRow;

                        int row = dRow.GetIndex();
                        int col = dColumn.DisplayIndex;

                        VariableTable variableTable = new VariableTable();
                        variableTable.SetModuleInfo(m_ModuleObj.Info.ProjectID, m_ModuleObj.Info.ModuleName,"");
                       
                        if (variableTable.ShowDialog() == true)
                        {                       
                            m_ModuleObj.m_InputVariableCollection[row].vLink = variableTable.SelectedVariableText;                          
                            _inputDataGrid.Items.Refresh();
                        }

                    }
                    break;
                }
                target = VisualTreeHelper.GetParent(target);
            }
            
        }

        private void _outputDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point pt = e.GetPosition(_outputDataGrid);
            IInputElement obj = _outputDataGrid.InputHitTest(pt);
            DependencyObject target = obj as DependencyObject;

            while (target != null)
            {
                if (target is System.Windows.Controls.DataGridCell)
                {
                    System.Windows.Controls.DataGridCell dCell = target as System.Windows.Controls.DataGridCell;
                    DataGridColumn dColumn = dCell.Column;
                    string headerStr = dColumn.Header as string;
                    if (headerStr.Trim() == "备注")
                    {
                        dCell.IsEditing = true;
                    }
                    else if (headerStr.Trim() == "类型")
                    {
                        while ((target != null) && !(target is DataGridRow))
                        {
                            target = VisualTreeHelper.GetParent(target);
                        }
                        DataGridRow dRow = target as DataGridRow;

                        int row = dRow.GetIndex();
                        int col = dColumn.DisplayIndex;

                        DialogComboBox dialogComboBox = new DialogComboBox();
                        dialogComboBox.SetTitle("变量类型");
                        dialogComboBox.SetComboBoxDataList(m_ModuleObj.m_OutputTypeList);

                        if (dialogComboBox.ShowDialog() == true)
                        {
                            m_ModuleObj.m_OutputVariableCollection[row].vType = dialogComboBox.GetSelectedText();
                            _outputDataGrid.Items.Refresh();
                        }

                    }
                    break;
                }
                target = VisualTreeHelper.GetParent(target);
            }
            
        }

        private void _outputDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string newValue = (e.EditingElement as System.Windows.Controls.TextBox).Text;

            int row = e.Row.GetIndex();
            int col = e.Column.DisplayIndex;

            if (col == 1)
            {
                m_ModuleObj.m_OutputVariableCollection[row].vRemark = newValue;
                _outputDataGrid.Items.Refresh();
            }

        }

        private void InitProcedure()
        {
            InitEditProcedure();

            InitExecuteProcedure();

            InitColorProcedure();

            InitSyntaxColoring(_scintillaEditor);

            m_ModuleObj.UpdateInputVariableCollection();
            m_ModuleObj.UpdateOutputVariableCollection();

            _inputDataGrid.ItemsSource = m_ModuleObj.m_InputVariableCollection;
            _outputDataGrid.ItemsSource = m_ModuleObj.m_OutputVariableCollection;

            _inputDataGrid.Items.Refresh();
            _outputDataGrid.Items.Refresh();
        }

        private void InitColorProcedure()
        {
            m_ModuleObj.m_ColorProcedureNameList.Clear();
            foreach (EProcedure eProcedure in m_ModuleObj.m_EProcedureList)
            {
                m_ModuleObj.m_ColorProcedureNameList.Add(eProcedure.Name);
            }
        }

        private void InitExecuteProcedure()
        {
            if (m_ModuleObj.m_EProcedureList == null) return;

            m_ModuleObj.m_ExecuteProcedureNameList.Clear();

            int idx = -1;
            foreach (EProcedure item in m_ModuleObj.m_EProcedureList)
            {
                //主函数不添加到列表中 
                if (item.Name != "main")
                {
                    idx++;
                    m_ModuleObj.m_ExecuteProcedureNameList.Add(new ProcedureItem { Name = item.Name, Index = idx });
                }
            }

            _executeProcedureComboBox.ItemsSource = m_ModuleObj.m_ExecuteProcedureNameList;
            _executeProcedureComboBox.DisplayMemberPath = "Name";
            _executeProcedureComboBox.SelectedValuePath = "Index";

            if (_executeProcedureComboBox.Items.Count > 0)
            {
                _executeProcedureComboBox.SelectedIndex = 0;
                m_ModuleObj.m_ExecuteProcedureName = m_ModuleObj.m_ExecuteProcedureNameList[_executeProcedureComboBox.SelectedIndex].Name;
            }

        }

        private void InitEditProcedure()
        {
            if (m_ModuleObj.m_EProcedureList == null) return;

            m_ModuleObj.m_EditProcedureNameList.Clear();

            int idx = -1;
            foreach (EProcedure eProcedure in m_ModuleObj.m_EProcedureList)
            {
                idx++;
                m_ModuleObj.m_EditProcedureNameList.Add(new ProcedureItem { Name = eProcedure.GetProcedureMethod(), Index = idx });
            }

            _editProcedureComboBox.ItemsSource = m_ModuleObj.m_EditProcedureNameList;
            _editProcedureComboBox.DisplayMemberPath = "Name";
            _editProcedureComboBox.SelectedValuePath = "Index";

            if (m_ModuleObj.m_EditProcedureNameList.Count > 0)
            {
                _editProcedureComboBox.SelectedIndex = 0;
                m_ModuleObj.m_EditProcedureName = m_ModuleObj.m_EditProcedureNameList[_editProcedureComboBox.SelectedIndex].Name;
                _scintillaEditor.Text = m_ModuleObj.m_EProcedureList[_editProcedureComboBox.SelectedIndex].Body;
            }
        }

        private void InitScintilla(Scintilla scintilla)
        {

            // 字体包裹模式
            scintilla.WrapMode = WrapMode.None;

            //自定义关键字代码提示功能
            scintilla.AutoCIgnoreCase = true;//代码提示的时候,不区分大小写

            //染色
            InitSyntaxColoring(scintilla);

            // 这两种操作会导致乱码
            scintilla.ClearCmdKey(Keys.Control | Keys.S);
            scintilla.ClearCmdKey(Keys.Control | Keys.F);

        }

        //设置语法高亮规则
        private void InitSyntaxColoring(Scintilla scintilla)
        {
            // 设置默认格式
            scintilla.StyleResetDefault();
            scintilla.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            scintilla.Styles[ScintillaNET.Style.Default].Size = 10;
            //背景色
            scintilla.Styles[ScintillaNET.Style.Default].ForeColor = System.Drawing.Color.Black;
            scintilla.StyleClearAll();

            scintilla.ScrollWidth = 100;//设置水平滚动条为100 这样水平就不会默认显示滚动条

            //普通代码的颜色
            scintilla.Styles[ScintillaNET.Style.Sql.Default].ForeColor = ColorTranslator.FromHtml("#644614");
            scintilla.Styles[ScintillaNET.Style.Sql.Comment].ForeColor = ColorTranslator.FromHtml("#644614");
            scintilla.Styles[ScintillaNET.Style.Sql.Number].ForeColor = ColorTranslator.FromHtml("#FF6532");
            scintilla.Styles[ScintillaNET.Style.Sql.Character].ForeColor = ColorTranslator.FromHtml("#A31515");
            // scintilla.Styles[ScintillaNET.Style.Sql.Preprocessor].ForeColor = IntToColor(0x8AAFEE);
            //操作符
            scintilla.Styles[ScintillaNET.Style.Sql.Operator].ForeColor = ColorTranslator.FromHtml("#644614");

            scintilla.Styles[ScintillaNET.Style.Sql.User1].ForeColor = ColorTranslator.FromHtml("#0000FF");//关键字
            scintilla.SetKeywords(4, Keyword.s_HalconString.ToLower());

            scintilla.Styles[ScintillaNET.Style.Sql.User2].ForeColor = ColorTranslator.FromHtml("#000096");//halcon算子

            scintilla.SetKeywords(5, Keyword.s_HalconProcedure.ToLower());  //这里的索引需要去查看 ScintillaNET.Style.Sql.Word2 对应的注释

            scintilla.Styles[ScintillaNET.Style.Sql.User3].ForeColor = ColorTranslator.FromHtml("#640096");//本地函数
            scintilla.SetKeywords(6, string.Join(" ", m_ModuleObj.m_ColorProcedureNameList).ToLower());

            scintilla.Lexer = Lexer.Sql;

            //行号字体颜色
            scintilla.Styles[ScintillaNET.Style.LineNumber].ForeColor = ColorTranslator.FromHtml("#8DA3C1");

            //行号相关设置
            var nums = scintilla.Margins[1];
            nums.Width = 30;
            nums.Type = MarginType.Number;
            nums.Sensitive = true;
            nums.Mask = 0;

            //注释
            int NUM = 8; // Indicators 0-7 could be in use by a lexerso we'll use indicator 8 to highlight words.
            scintilla.IndicatorCurrent = NUM;//
            scintilla.Indicators[NUM].Style = IndicatorStyle.TextFore;
            scintilla.Indicators[NUM].ForeColor = ColorTranslator.FromHtml("#008000");
            scintilla.Indicators[NUM].OutlineAlpha = 100;
            scintilla.Indicators[NUM].Alpha = 100;
        }

        private void _scintillaEditor_TextChanged(object sender, EventArgs e)
        {
            Scintilla scintilla = sender as Scintilla;

            scintilla.IndicatorClearRange(0, scintilla.TextLength);

            foreach (ScintillaNET.Line line in scintilla.Lines)
            {
                if (line.Text.Trim().StartsWith("*"))//开始的是* 则是注释符号
                {
                    string text = line.Text;

                    // Update indicator appearance
                    // Search the document
                    scintilla.TargetStart = 0;
                    scintilla.TargetEnd = scintilla.TextLength;
                    scintilla.SearchFlags = SearchFlags.None;
                    while (scintilla.SearchInTarget(text) != -1)
                    {
                        // Mark the search results with the current indicator
                        scintilla.IndicatorFillRange(scintilla.TargetStart, scintilla.TargetEnd - scintilla.TargetStart);
                        // Search the remainder of the document
                        scintilla.TargetStart = scintilla.TargetEnd;
                        scintilla.TargetEnd = scintilla.TextLength;
                    }
                }
            }

            if (m_ModuleObj.m_EProcedureList != null)
            {
                m_ModuleObj.m_EProcedureList[_editProcedureComboBox.SelectedIndex].Body = scintilla.Text;
            }
            else
            {
                System.Windows.MessageBox.Show("请先导入正确的图像脚本");
            }

        }
       
    }

    class Keyword
    {
        public static string s_HalconString = "abs assign bnot catch comment cumul elseif endwhile export_def floor global H_MSG_VOID int log max2 min2 ord rand regexp_select return sgn sort_index strchr strstr tanh uniq acos atan bor ceil continue deg endfor environment fabs fmod H_MSG_FAIL if inverse log10 mean not ords real regexp_test round sin split strlen subset throw until and atan2 break chr cos deviation endif exit false for H_MSG_FALSE ifelse is_number lsh median number pow regexp_match remove rsh sinh sqrt strrchr sum true while asin band bxor chrt cosh else endtry exp find gen_tuple_const H_MSG_TRUE insert ldexp max min or rad regexp_replace repeat select_rank sort stop strrstr tan try xor ";

        public static string s_HalconProcedure = " close_measure  deserialize_measure  fuzzy_measure_pairing  fuzzy_measure_pairs  fuzzy_measure_pos  gen_measure_arc  gen_measure_rectangle2  measure_pairs  measure_pos  measure_projection  measure_thresh  read_measure  reset_fuzzy_measure  serialize_measure  set_fuzzy_measure  set_fuzzy_measure_norm_pair  translate_measure  write_measure  add_metrology_object_circle_measure  add_metrology_object_ellipse_measure  add_metrology_object_generic  add_metrology_object_line_measure  add_metrology_object_rectangle2_measure  align_metrology_model  apply_metrology_model  clear_metrology_model  clear_metrology_object  copy_metrology_model  create_metrology_model  deserialize_metrology_model  get_metrology_model_param  get_metrology_object_fuzzy_param  get_metrology_object_indices  get_metrology_object_measures  get_metrology_object_model_contour  get_metrology_object_num_instances  get_metrology_object_param  get_metrology_object_result  get_metrology_object_result_contour  read_metrology_model  reset_metrology_object_fuzzy_param  reset_metrology_object_param  serialize_metrology_model  set_metrology_model_image_size  set_metrology_model_param  set_metrology_object_fuzzy_param  set_metrology_object_param  write_metrology_model  add_deformable_surface_model_reference_point  add_deformable_surface_model_sample  clear_deformable_surface_matching_result  clear_deformable_surface_model  create_deformable_surface_model  deserialize_deformable_surface_model  find_deformable_surface_model  get_deformable_surface_matching_result  get_deformable_surface_model_param  read_deformable_surface_model  refine_deformable_surface_model  serialize_deformable_surface_model  write_deformable_surface_model  clear_shape_model_3d  create_cam_pose_look_at_point  create_shape_model_3d  deserialize_shape_model_3d  find_shape_model_3d  get_shape_model_3d_contours  get_shape_model_3d_params  project_shape_model_3d  read_shape_model_3d  serialize_shape_model_3d  trans_pose_shape_model_3d  write_shape_model_3d  clear_surface_matching_result  clear_surface_model  create_surface_model  deserialize_surface_model  find_surface_model  get_surface_matching_result  get_surface_model_param  read_surface_model  refine_surface_model_pose  serialize_surface_model  write_surface_model  clear_object_model_3d  copy_object_model_3d  deserialize_object_model_3d  gen_box_object_model_3d  gen_cylinder_object_model_3d  gen_empty_object_model_3d  gen_object_model_3d_from_points  gen_plane_object_model_3d  gen_sphere_object_model_3d  gen_sphere_object_model_3d_center  read_object_model_3d  serialize_object_model_3d  set_object_model_3d_attrib  set_object_model_3d_attrib_mod  union_object_model_3d  write_object_model_3d  area_object_model_3d  distance_object_model_3d  get_object_model_3d_params  max_diameter_object_model_3d  moments_object_model_3d  select_object_model_3d  smallest_bounding_box_object_model_3d  smallest_sphere_object_model_3d  volume_object_model_3d_relative_to_plane  fit_primitives_object_model_3d  reduce_object_model_3d_by_view  segment_object_model_3d  select_points_object_model_3d  affine_trans_object_model_3d  connection_object_model_3d  convex_hull_object_model_3d  intersect_plane_object_model_3d  object_model_3d_to_xyz  prepare_object_model_3d  project_object_model_3d  projective_trans_object_model_3d  register_object_model_3d_global  register_object_model_3d_pair  render_object_model_3d  rigid_trans_object_model_3d  sample_object_model_3d  simplify_object_model_3d  smooth_object_model_3d  surface_normals_object_model_3d  triangulate_object_model_3d  xyz_to_object_model_3d  binocular_disparity  binocular_disparity_mg  binocular_disparity_ms  binocular_distance  binocular_distance_mg  binocular_distance_ms  disparity_image_to_xyz  disparity_to_distance  disparity_to_point_3d  distance_to_disparity  essential_to_fundamental_matrix  gen_binocular_proj_rectification  gen_binocular_rectification_map  intersect_lines_of_sight  match_essential_matrix_ransac  match_fundamental_matrix_distortion_ransac  match_fundamental_matrix_ransac  match_rel_pose_ransac  reconst3d_from_fundamental_matrix  rel_pose_to_fundamental_matrix  vector_to_essential_matrix  vector_to_fundamental_matrix  vector_to_fundamental_matrix_distortion  vector_to_rel_pose  depth_from_focus  select_grayvalues_from_channels  clear_stereo_model  create_stereo_model  get_stereo_model_image_pairs  get_stereo_model_object  get_stereo_model_param  reconstruct_points_stereo  reconstruct_surface_stereo  set_stereo_model_image_pairs  set_stereo_model_param  estimate_al_am  estimate_sl_al_lr  estimate_sl_al_zc  estimate_tilt_lr  estimate_tilt_zc  photometric_stereo  reconstruct_height_field_from_gradient  sfs_mod_lr  sfs_orig_lr  sfs_pentland  shade_height_field  apply_sheet_of_light_calibration  calibrate_sheet_of_light  clear_sheet_of_light_model  create_sheet_of_light_calib_object  create_sheet_of_light_model  deserialize_sheet_of_light_model  get_sheet_of_light_param  get_sheet_of_light_result  get_sheet_of_light_result_object_model_3d  measure_profile_sheet_of_light  query_sheet_of_light_params  read_sheet_of_light_model  reset_sheet_of_light_model  serialize_sheet_of_light_model  set_profile_sheet_of_light  set_sheet_of_light_param  write_sheet_of_light_model  binocular_calibration  caltab_points  create_caltab  disp_caltab  find_calib_object  find_caltab  find_marks_and_pose  gen_caltab  sim_caltab  cam_mat_to_cam_par  cam_par_to_cam_mat  deserialize_cam_par  read_cam_par  serialize_cam_par  write_cam_par  calibrate_hand_eye  get_calib_data_observ_pose  hand_eye_calibration  set_calib_data_observ_pose  get_line_of_sight  camera_calibration  calibrate_cameras  clear_calib_data  clear_camera_setup_model  create_calib_data  create_camera_setup_model  deserialize_calib_data  deserialize_camera_setup_model  get_calib_data  get_calib_data_observ_contours  get_calib_data_observ_points  get_camera_setup_param  query_calib_data_observ_indices  read_calib_data  read_camera_setup_model  remove_calib_data  remove_calib_data_observ  serialize_calib_data  serialize_camera_setup_model  set_calib_data  set_calib_data_calib_object  set_calib_data_cam_param  set_calib_data_observ_points  set_camera_setup_cam_param  set_camera_setup_param  write_calib_data  write_camera_setup_model  cam_par_pose_to_hom_mat3d  project_3d_point  project_hom_point_hom_mat3d  project_point_hom_mat3d  change_radial_distortion_cam_par  change_radial_distortion_contours_xld  change_radial_distortion_image  change_radial_distortion_points  contour_to_world_plane_xld  gen_image_to_world_plane_map  gen_radial_distortion_map  image_points_to_world_plane  image_to_world_plane  radial_distortion_self_calibration  radiometric_self_calibration  stationary_camera_self_calibration  add_class_train_data_gmm  add_sample_class_gmm  classify_class_gmm  clear_class_gmm  clear_samples_class_gmm  create_class_gmm  deserialize_class_gmm  evaluate_class_gmm  get_class_train_data_gmm  get_params_class_gmm  get_prep_info_class_gmm  get_sample_class_gmm  get_sample_num_class_gmm  read_class_gmm  read_samples_class_gmm  select_feature_set_gmm  serialize_class_gmm  train_class_gmm  write_class_gmm  write_samples_class_gmm  clear_sampset  close_class_box  create_class_box  descript_class_box  deserialize_class_box  enquire_class_box  enquire_reject_class_box  get_class_box_param  learn_class_box  learn_sampset_box  read_class_box  read_sampset  serialize_class_box  set_class_box_param  test_sampset_box  write_class_box  add_class_train_data_knn  add_sample_class_knn  classify_class_knn  clear_class_knn  create_class_knn  deserialize_class_knn  get_class_train_data_knn  get_params_class_knn  get_sample_class_knn  get_sample_num_class_knn  read_class_knn  select_feature_set_knn  serialize_class_knn  set_params_class_knn  train_class_knn  write_class_knn  clear_class_lut  create_class_lut_gmm  create_class_lut_knn  create_class_lut_mlp  create_class_lut_svm  add_sample_class_train_data  clear_class_train_data  create_class_train_data  deserialize_class_train_data  get_sample_class_train_data  get_sample_num_class_train_data  read_class_train_data  select_sub_feature_class_train_data  serialize_class_train_data  set_feature_lengths_class_train_data  write_class_train_data  add_class_train_data_mlp  add_sample_class_mlp  classify_class_mlp  clear_class_mlp  clear_samples_class_mlp  create_class_mlp  deserialize_class_mlp  evaluate_class_mlp  get_class_train_data_mlp  get_params_class_mlp  get_prep_info_class_mlp  get_regularization_params_class_mlp  get_rejection_params_class_mlp  get_sample_class_mlp  get_sample_num_class_mlp  read_class_mlp  read_samples_class_mlp  select_feature_set_mlp  serialize_class_mlp  set_regularization_params_class_mlp  set_rejection_params_class_mlp  train_class_mlp  write_class_mlp  write_samples_class_mlp  add_class_train_data_svm  add_sample_class_svm  classify_class_svm  clear_class_svm  clear_samples_class_svm  create_class_svm  deserialize_class_svm  evaluate_class_svm  get_class_train_data_svm  get_params_class_svm  get_prep_info_class_svm  get_sample_class_svm  get_sample_num_class_svm  get_support_vector_class_svm  get_support_vector_num_class_svm  read_class_svm  read_samples_class_svm  reduce_class_svm  select_feature_set_svm  serialize_class_svm  train_class_svm  write_class_svm  write_samples_class_svm  convert_tuple_to_vector_1d  convert_vector_to_tuple  executable_expression  export_def  par_join  dev_clear_obj  dev_clear_window  dev_close_inspect_ctrl  dev_close_tool  dev_close_window  dev_display  dev_error_var  dev_get_exception_data  dev_get_preferences  dev_get_system  dev_get_window  dev_inspect_ctrl  dev_map_par  dev_map_prog  dev_map_var  dev_open_dialog  dev_open_file_dialog  dev_open_tool  dev_open_window  dev_set_check  dev_set_color  dev_set_colored  dev_set_draw  dev_set_line_width  dev_set_lut  dev_set_paint  dev_set_part  dev_set_preferences  dev_set_shape  dev_set_tool_geometry  dev_set_window  dev_set_window_extents  dev_show_tool  dev_unmap_par  dev_unmap_prog  dev_unmap_var  dev_update_pc  dev_update_time  dev_update_var  dev_update_window  deserialize_image  read_image  read_sequence  serialize_image  write_image  copy_file  delete_file  file_exists  get_current_dir  list_files  make_dir  read_world_file  remove_dir  set_current_dir  deserialize_object  read_object  serialize_object  write_object  deserialize_region  read_region  serialize_region  write_region  close_file  fnew_line  fread_char  fread_line  fread_string  fwrite_string  open_file  deserialize_tuple  read_tuple  serialize_tuple  write_tuple  deserialize_xld  read_contour_xld_arc_info  read_contour_xld_dxf  read_polygon_xld_arc_info  read_polygon_xld_dxf  serialize_xld  write_contour_xld_arc_info  write_contour_xld_dxf  write_polygon_xld_arc_info  write_polygon_xld_dxf  abs_diff_image  abs_image  acos_image  add_image  asin_image  atan2_image  atan_image  cos_image  div_image  exp_image  gamma_image  invert_image  log_image  max_image  min_image  mult_image  pow_image  scale_image  sin_image  sqrt_image  sub_image  tan_image  bit_and  bit_lshift  bit_mask  bit_not  bit_or  bit_rshift  bit_slice  bit_xor  apply_color_trans_lut  cfa_to_rgb  clear_color_trans_lut  create_color_trans_lut  gen_principal_comp_trans  linear_trans_color  principal_comp  rgb1_to_gray  rgb3_to_gray  trans_from_rgb  trans_to_rgb  close_edges  close_edges_length  derivate_gauss  diff_of_gauss  edges_color  edges_color_sub_pix  edges_image  edges_sub_pix  frei_amp  frei_dir  highpass_image  info_edges  kirsch_amp  kirsch_dir  laplace  laplace_of_gauss  prewitt_amp  prewitt_dir  roberts  robinson_amp  robinson_dir  sobel_amp  sobel_dir  coherence_enhancing_diff  emphasize  equ_histo_image  illuminate  mean_curvature_flow  scale_image_max  shock_filter  convol_fft  convol_gabor  correlation_fft  deserialize_fft_optimization_data  energy_gabor  fft_generic  fft_image  fft_image_inv  gen_bandfilter  gen_bandpass  gen_derivative_filter  gen_filter_mask  gen_gabor  gen_gauss_filter  gen_highpass  gen_lowpass  gen_mean_filter  gen_sin_bandpass  gen_std_bandpass  optimize_fft_speed  optimize_rft_speed  phase_correlation_fft  phase_deg  phase_rad  power_byte  power_ln  power_real  read_fft_optimization_data  rft_generic  serialize_fft_optimization_data  write_fft_optimization_data  affine_trans_image  affine_trans_image_size  convert_map_type  map_image  mirror_image  polar_trans_image_ext  polar_trans_image_inv  projective_trans_image  projective_trans_image_size  rotate_image  zoom_image_factor  zoom_image_size  harmonic_interpolation  inpainting_aniso  inpainting_ced  inpainting_ct  inpainting_mcf  inpainting_texture  bandpass_image  lines_color  lines_facet  lines_gauss  exhaustive_match  exhaustive_match_mg  gen_gauss_pyramid  monotony  convol_image  deviation_n  expand_domain_gray  gray_inside  gray_skeleton  lut_trans  symmetry  topographic_sketch  add_noise_distribution  add_noise_white  gauss_distribution  noise_distribution_mean  sp_distribution  derivate_vector_field  optical_flow_mg  unwarp_image_vector_field  vector_field_length  corner_response  dots_image  points_foerstner  points_harris  points_harris_binomial  points_lepetit  points_sojka  scene_flow_calib  scene_flow_uncalib  anisotropic_diffusion  binomial_filter  eliminate_min_max  eliminate_sp  fill_interlace  gauss_filter  info_smooth  isotropic_diffusion  mean_image  mean_n  mean_sp  median_image  median_rect  median_separate  median_weighted  midrange_image  rank_image  rank_n  rank_rect  sigma_image  smooth_image  trimmed_mean  deviation_image  entropy_image  texture_laws  gen_psf_defocus  gen_psf_motion  simulate_defocus  simulate_motion  wiener_filter  wiener_filter_ni  add_scene_3d_camera  add_scene_3d_instance  add_scene_3d_light  clear_scene_3d  create_scene_3d  display_scene_3d  get_display_scene_3d_info  remove_scene_3d_camera  remove_scene_3d_instance  remove_scene_3d_light  render_scene_3d  set_scene_3d_camera_pose  set_scene_3d_instance_param  set_scene_3d_instance_pose  set_scene_3d_light_param  set_scene_3d_param  set_scene_3d_to_world_pose  drag_region1  drag_region2  drag_region3  draw_circle  draw_circle_mod  draw_ellipse  draw_ellipse_mod  draw_line  draw_line_mod  draw_nurbs  draw_nurbs_interp  draw_nurbs_interp_mod  draw_nurbs_mod  draw_point  draw_point_mod  draw_polygon  draw_rectangle1  draw_rectangle1_mod  draw_rectangle2  draw_rectangle2_mod  draw_region  draw_xld  draw_xld_mod  gnuplot_close  gnuplot_open_file  gnuplot_open_pipe  gnuplot_plot_ctrl  gnuplot_plot_funct_1d  gnuplot_plot_image  disp_lut  get_fixed_lut  get_lut  get_lut_style  query_lut  set_fixed_lut  set_lut  set_lut_style  write_lut  get_mbutton  get_mbutton_sub_pix  get_mposition  get_mposition_sub_pix  get_mshape  query_mshape  set_mshape  attach_background_to_window  attach_drawing_object_to_window  clear_drawing_object  create_drawing_object_circle  create_drawing_object_circle_sector  create_drawing_object_ellipse  create_drawing_object_ellipse_sector  create_drawing_object_line  create_drawing_object_rectangle1  create_drawing_object_rectangle2  create_drawing_object_text  create_drawing_object_xld  detach_background_from_window  detach_drawing_object_from_window  get_drawing_object_iconic  get_drawing_object_params  get_window_background_image  set_drawing_object_callback  set_drawing_object_params  set_drawing_object_xld  disp_arc  disp_arrow  disp_channel  disp_circle  disp_color  disp_cross  disp_distribution  disp_ellipse  disp_image  disp_line  disp_obj  disp_object_model_3d  disp_polygon  disp_rectangle1  disp_rectangle2  disp_region  disp_xld  get_comprise  get_draw  get_fix  get_hsi  get_icon  get_insert  get_line_approx  get_line_style  get_line_width  get_paint  get_part  get_part_style  get_pixel  get_rgb  get_shape  get_window_param  query_all_colors  query_color  query_colored  query_gray  query_insert  query_line_width  query_paint  query_shape  set_color  set_colored  set_comprise  set_draw  set_fix  set_gray  set_hsi  set_icon  set_insert  set_line_approx  set_line_style  set_line_width  set_paint  set_part  set_part_style  set_pixel  set_rgb  set_shape  set_window_param  get_font  get_font_extents  get_string_extents  get_tposition  get_tshape  new_line  query_font  query_tshape  read_char  read_string  set_font  set_tposition  set_tshape  write_string  clear_rectangle  clear_window  close_window  copy_rectangle  dump_window  dump_window_image  get_disp_object_model_3d_info  get_os_window_handle  get_window_attr  get_window_extents  get_window_pointer3  get_window_type  move_rectangle  new_extern_window  open_textwindow  open_window  query_window_type  set_window_attr  set_window_dc  set_window_extents  set_window_type  slide_image  unproject_coordinates  update_window_pose  clear_bar_code_model  create_bar_code_model  decode_bar_code_rectangle2  deserialize_bar_code_model  find_bar_code  get_bar_code_object  get_bar_code_param  get_bar_code_param_specific  get_bar_code_result  query_bar_code_params  read_bar_code_model  serialize_bar_code_model  set_bar_code_param  set_bar_code_param_specific  write_bar_code_model  clear_data_code_2d_model  create_data_code_2d_model  deserialize_data_code_2d_model  find_data_code_2d  get_data_code_2d_objects  get_data_code_2d_param  get_data_code_2d_results  query_data_code_2d_params  read_data_code_2d_model  serialize_data_code_2d_model  set_data_code_2d_param  write_data_code_2d_model  add_sample_identifier_preparation_data  add_sample_identifier_training_data  apply_sample_identifier  clear_sample_identifier  create_sample_identifier  deserialize_sample_identifier  get_sample_identifier_object_info  get_sample_identifier_param  prepare_sample_identifier  read_sample_identifier  remove_sample_identifier_preparation_data  remove_sample_identifier_training_data  serialize_sample_identifier  set_sample_identifier_object_info  set_sample_identifier_param  train_sample_identifier  write_sample_identifier  get_grayval  get_grayval_contour_xld  get_grayval_interpolated  get_image_pointer1  get_image_pointer1_rect  get_image_pointer3  get_image_size  get_image_time  get_image_type  close_framegrabber  get_framegrabber_callback  get_framegrabber_lut  get_framegrabber_param  grab_data  grab_data_async  grab_image  grab_image_async  grab_image_start  info_framegrabber  open_framegrabber  set_framegrabber_callback  set_framegrabber_lut  set_framegrabber_param  access_channel  append_channel  channels_to_image  compose2  compose3  compose4  compose5  compose6  compose7  count_channels  decompose2  decompose3  decompose4  decompose5  decompose6  decompose7  image_to_channels  copy_image  gen_image1  gen_image1_extern  gen_image1_rect  gen_image3  gen_image3_extern  gen_image_const  gen_image_gray_ramp  gen_image_interleaved  gen_image_proto  gen_image_surface_first_order  gen_image_surface_second_order  region_to_bin  region_to_label  region_to_mean  add_channels  change_domain  full_domain  get_domain  rectangle1_domain  reduce_domain  area_center_gray  cooc_feature_image  cooc_feature_matrix  elliptic_axis_gray  entropy_gray  estimate_noise  fit_surface_first_order  fit_surface_second_order  fuzzy_entropy  fuzzy_perimeter  gen_cooc_matrix  gray_features  gray_histo  gray_histo_abs  gray_histo_range  gray_projections  histo_2dim  intensity  min_max_gray  moments_gray_plane  plane_deviation  select_gray  shape_histo_all  shape_histo_point  change_format  crop_domain  crop_domain_rel  crop_part  crop_rectangle1  tile_channels  tile_images  tile_images_offset  overpaint_gray  overpaint_region  paint_gray  paint_region  paint_xld  set_grayval  complex_to_real  convert_image_type  real_to_complex  real_to_vector_field  vector_field_to_real  apply_bead_inspection_model  clear_bead_inspection_model  create_bead_inspection_model  get_bead_inspection_param  set_bead_inspection_param  close_ocv  create_ocv_proj  deserialize_ocv  do_ocv_simple  read_ocv  serialize_ocv  traind_ocv_proj  write_ocv  clear_train_data_variation_model  clear_variation_model  compare_ext_variation_model  compare_variation_model  create_variation_model  deserialize_variation_model  get_thresh_images_variation_model  get_variation_model  prepare_direct_variation_model  prepare_variation_model  read_variation_model  serialize_variation_model  train_variation_model  write_variation_model  decode_1d_bar_code  discrete_1d_bar_code  find_1d_bar_code  find_1d_bar_code_region  find_1d_bar_code_scanline  gen_1d_bar_code_descr  gen_1d_bar_code_descr_gen  get_1d_bar_code  get_1d_bar_code_scanline  decode_2d_bar_code  find_2d_bar_code  gen_2d_bar_code_descr  get_2d_bar_code  get_2d_bar_code_pos  copy_metrology_object  transform_metrology_object  read_object_model_3d_dxf  phot_stereo  anisotrope_diff  gauss_image  polar_trans_image  abs_invar_fourier_coeff  fourier_1dim  fourier_1dim_inv  invar_fourier_coeff  match_fourier_coeff  move_contour_orig  prep_contour_fourier  draw_lut  create_text_model  check_par_hw_potential  load_par_knowledge  store_par_knowledge  bin_threshold  get_socket_timeout  set_socket_timeout  clear_all_bar_code_models  clear_all_barriers  clear_all_calib_data  clear_all_camera_setup_models  clear_all_class_gmm  clear_all_class_knn  clear_all_class_lut  clear_all_class_mlp  clear_all_class_svm  clear_all_class_train_data  clear_all_color_trans_luts  clear_all_component_models  clear_all_conditions  clear_all_data_code_2d_models  clear_all_deformable_models  clear_all_descriptor_models  clear_all_events  clear_all_lexica  clear_all_matrices  clear_all_metrology_models  clear_all_mutexes  clear_all_ncc_models  clear_all_object_model_3d  clear_all_ocr_class_knn  clear_all_ocr_class_mlp  clear_all_ocr_class_svm  clear_all_sample_identifiers  clear_all_scattered_data_interpolators  clear_all_serialized_items  clear_all_shape_model_3d  clear_all_shape_models  clear_all_sheet_of_light_models  clear_all_stereo_models  clear_all_surface_matching_results  clear_all_surface_models  clear_all_templates  clear_all_text_models  clear_all_text_results  clear_all_training_components  clear_all_variation_models  close_all_bg_esti  close_all_class_box  close_all_files  close_all_framegrabbers  close_all_measures  close_all_ocrs  close_all_ocvs  close_all_serials  close_all_sockets  intersection_ll  union_straight_contours_histo_xld  clear_component_model  clear_training_components  cluster_model_components  create_component_model  create_trained_component_model  deserialize_component_model  deserialize_training_components  find_component_model  gen_initial_components  get_component_model_params  get_component_model_tree  get_component_relations  get_found_component_model  get_training_components  inspect_clustered_components  modify_component_relations  read_component_model  read_training_components  serialize_component_model  serialize_training_components  train_model_components  write_component_model  write_training_components  clear_ncc_model  create_ncc_model  deserialize_ncc_model  determine_ncc_model_params  find_ncc_model  get_ncc_model_origin  get_ncc_model_params  read_ncc_model  serialize_ncc_model  set_ncc_model_origin  set_ncc_model_param  write_ncc_model  clear_deformable_model  create_local_deformable_model  create_local_deformable_model_xld  create_planar_calib_deformable_model  create_planar_calib_deformable_model_xld  create_planar_uncalib_deformable_model  create_planar_uncalib_deformable_model_xld  deserialize_deformable_model  determine_deformable_model_params  find_local_deformable_model  find_planar_calib_deformable_model  find_planar_uncalib_deformable_model  get_deformable_model_contours  get_deformable_model_origin  get_deformable_model_params  read_deformable_model  serialize_deformable_model  set_deformable_model_origin  set_deformable_model_param  set_local_deformable_model_metric  set_planar_calib_deformable_model_metric  set_planar_uncalib_deformable_model_metric  write_deformable_model  clear_descriptor_model  create_calib_descriptor_model  create_uncalib_descriptor_model  deserialize_descriptor_model  find_calib_descriptor_model  find_uncalib_descriptor_model  get_descriptor_model_origin  get_descriptor_model_params  get_descriptor_model_points  get_descriptor_model_results  read_descriptor_model  serialize_descriptor_model  set_descriptor_model_origin  write_descriptor_model  adapt_template  best_match  best_match_mg  best_match_pre_mg  best_match_rot  best_match_rot_mg  clear_template  create_template  create_template_rot  deserialize_template  fast_match  fast_match_mg  read_template  serialize_template  set_offset_template  set_reference_template  write_template  clear_shape_model  create_aniso_shape_model  create_aniso_shape_model_xld  create_scaled_shape_model  create_scaled_shape_model_xld  create_shape_model  create_shape_model_xld  deserialize_shape_model  determine_shape_model_params  find_aniso_shape_model  find_aniso_shape_models  find_scaled_shape_model  find_scaled_shape_models  find_shape_model  find_shape_models  get_shape_model_contours  get_shape_model_origin  get_shape_model_params  inspect_shape_model  read_shape_model  serialize_shape_model  set_shape_model_metric  set_shape_model_origin  set_shape_model_param  write_shape_model  get_diagonal_matrix  get_full_matrix  get_sub_matrix  get_value_matrix  set_diagonal_matrix  set_full_matrix  set_sub_matrix  set_value_matrix  abs_matrix  abs_matrix_mod  add_matrix  add_matrix_mod  div_element_matrix  div_element_matrix_mod  invert_matrix  invert_matrix_mod  mult_element_matrix  mult_element_matrix_mod  mult_matrix  mult_matrix_mod  pow_element_matrix  pow_element_matrix_mod  pow_matrix  pow_matrix_mod  pow_scalar_element_matrix  pow_scalar_element_matrix_mod  scale_matrix  scale_matrix_mod  solve_matrix  sqrt_matrix  sqrt_matrix_mod  sub_matrix  sub_matrix_mod  transpose_matrix  transpose_matrix_mod  clear_matrix  copy_matrix  create_matrix  repeat_matrix  decompose_matrix  orthogonal_decompose_matrix  svd_matrix  eigenvalues_general_matrix  eigenvalues_symmetric_matrix  generalized_eigenvalues_general_matrix  generalized_eigenvalues_symmetric_matrix  determinant_matrix  get_size_matrix  max_matrix  mean_matrix  min_matrix  norm_matrix  sum_matrix  deserialize_matrix  read_matrix  serialize_matrix  write_matrix  dual_rank  gen_disc_se  gray_bothat  gray_closing  gray_closing_rect  gray_closing_shape  gray_dilation  gray_dilation_rect  gray_dilation_shape  gray_erosion  gray_erosion_rect  gray_erosion_shape  gray_opening  gray_opening_rect  gray_opening_shape  gray_range_rect  gray_tophat  read_gray_se  bottom_hat  boundary  closing  closing_circle  closing_golay  closing_rectangle1  dilation1  dilation2  dilation_circle  dilation_golay  dilation_rectangle1  dilation_seq  erosion1  erosion2  erosion_circle  erosion_golay  erosion_rectangle1  erosion_seq  fitting  gen_struct_elements  golay_elements  hit_or_miss  hit_or_miss_golay  hit_or_miss_seq  minkowski_add1  minkowski_add2  minkowski_sub1  minkowski_sub2  morph_hat  morph_skeleton  morph_skiz  opening  opening_circle  opening_golay  opening_rectangle1  opening_seg  pruning  thickening  thickening_golay  thickening_seq  thinning  thinning_golay  thinning_seq  top_hat  close_ocr  create_ocr_class_box  deserialize_ocr  do_ocr_multi  do_ocr_single  info_ocr_class_box  ocr_change_char  ocr_get_features  read_ocr  serialize_ocr  testd_ocr_class_box  traind_ocr_class_box  trainf_ocr_class_box  write_ocr  clear_ocr_class_knn  create_ocr_class_knn  deserialize_ocr_class_knn  do_ocr_multi_class_knn  do_ocr_single_class_knn  do_ocr_word_knn  get_features_ocr_class_knn  get_params_ocr_class_knn  read_ocr_class_knn  select_feature_set_trainf_knn  serialize_ocr_class_knn  trainf_ocr_class_knn  write_ocr_class_knn  clear_lexicon  create_lexicon  import_lexicon  inspect_lexicon  lookup_lexicon  suggest_lexicon  clear_ocr_class_mlp  create_ocr_class_mlp  deserialize_ocr_class_mlp  do_ocr_multi_class_mlp  do_ocr_single_class_mlp  do_ocr_word_mlp  get_features_ocr_class_mlp  get_params_ocr_class_mlp  get_prep_info_ocr_class_mlp  get_regularization_params_ocr_class_mlp  get_rejection_params_ocr_class_mlp  read_ocr_class_mlp  select_feature_set_trainf_mlp  select_feature_set_trainf_mlp_protected  serialize_ocr_class_mlp  set_regularization_params_ocr_class_mlp  set_rejection_params_ocr_class_mlp  trainf_ocr_class_mlp  trainf_ocr_class_mlp_protected  write_ocr_class_mlp  clear_text_model  clear_text_result  create_text_model_reader  find_text  get_text_model_param  get_text_object  get_text_result  segment_characters  select_characters  set_text_model_param  text_line_orientation  text_line_slant  clear_ocr_class_svm  create_ocr_class_svm  deserialize_ocr_class_svm  do_ocr_multi_class_svm  do_ocr_single_class_svm  do_ocr_word_svm  get_features_ocr_class_svm  get_params_ocr_class_svm  get_prep_info_ocr_class_svm  get_support_vector_num_ocr_class_svm  get_support_vector_ocr_class_svm  read_ocr_class_svm  reduce_ocr_class_svm  select_feature_set_trainf_svm  select_feature_set_trainf_svm_protected  serialize_ocr_class_svm  trainf_ocr_class_svm  trainf_ocr_class_svm_protected  write_ocr_class_svm  append_ocr_trainf  concat_ocr_trainf  protect_ocr_trainf  read_ocr_trainf  read_ocr_trainf_names  read_ocr_trainf_names_protected  read_ocr_trainf_select  write_ocr_trainf  write_ocr_trainf_image  compare_obj  count_obj  get_channel_info  get_obj_class  test_equal_obj  clear_obj  concat_obj  copy_obj  gen_empty_obj  integer_to_obj  obj_diff  obj_to_integer  select_obj  get_region_chain  get_region_contour  get_region_convex  get_region_points  get_region_polygon  get_region_runs  gen_checker_region  gen_circle  gen_circle_sector  gen_ellipse  gen_ellipse_sector  gen_empty_region  gen_grid_region  gen_random_region  gen_random_regions  gen_rectangle1  gen_rectangle2  gen_region_contour_xld  gen_region_histo  gen_region_hline  gen_region_line  gen_region_points  gen_region_polygon  gen_region_polygon_filled  gen_region_polygon_xld  gen_region_runs  label_to_region  area_center  area_holes  circularity  compactness  connect_and_holes  contlength  convexity  diameter_region  eccentricity  elliptic_axis  euler_number  find_neighbors  get_region_index  get_region_thickness  hamming_distance  hamming_distance_norm  inner_circle  inner_rectangle1  moments_region_2nd  moments_region_2nd_invar  moments_region_2nd_rel_invar  moments_region_3rd  moments_region_3rd_invar  moments_region_central  moments_region_central_invar  orientation_region  rectangularity  region_features  roundness  runlength_distribution  runlength_features  select_region_point  select_region_spatial  select_shape  select_shape_proto  select_shape_std  smallest_circle  smallest_rectangle1  smallest_rectangle2  spatial_relation  affine_trans_region  mirror_region  move_region  polar_trans_region  polar_trans_region_inv  projective_trans_region  transpose_region  zoom_region  complement  difference  intersection  symm_difference  union1  union2  test_equal_region  test_region_point  test_subset_region  background_seg  clip_region  clip_region_rel  closest_point_transform  connection  distance_transform  eliminate_runs  expand_region  fill_up  fill_up_shape  hamming_change_region  interjacent  junctions_skeleton  merge_regions_line_scan  partition_dynamic  partition_rectangle  rank_region  remove_noise_region  shape_trans  skeleton  sort_region  split_skeleton_lines  split_skeleton_region  add_samples_image_class_gmm  add_samples_image_class_knn  add_samples_image_class_mlp  add_samples_image_class_svm  class_2dim_sup  class_2dim_unsup  class_ndim_box  class_ndim_norm  classify_image_class_gmm  classify_image_class_knn  classify_image_class_lut  classify_image_class_mlp  classify_image_class_svm  learn_ndim_box  learn_ndim_norm  detect_edge_segments  hysteresis_threshold  nonmax_suppression_amp  nonmax_suppression_dir  expand_gray  expand_gray_ref  expand_line  regiongrowing  regiongrowing_mean  regiongrowing_n  auto_threshold  binary_threshold  char_threshold  check_difference  dual_threshold  dyn_threshold  fast_threshold  histo_to_thresh  local_threshold  threshold  threshold_sub_pix  var_threshold  zero_crossing  zero_crossing_sub_pix  critical_points_sub_pix  local_max  local_max_sub_pix  local_min  local_min_sub_pix  lowlands  lowlands_center  plateaus  plateaus_center  pouring  saddle_points_sub_pix  watersheds  watersheds_threshold  activate_compute_device  deactivate_all_compute_devices  deactivate_compute_device  get_compute_device_info  get_compute_device_param  init_compute_device  open_compute_device  query_available_compute_devices  release_all_compute_devices  release_compute_device  set_compute_device_param  count_relation  get_modules  reset_obj_db  get_check  get_error_text  get_extended_error_info  get_spy  query_spy  set_check  set_spy  close_io_channel  close_io_device  control_io_channel  control_io_device  control_io_interface  get_io_channel_param  get_io_device_param  open_io_channel  open_io_device  query_io_device  query_io_interface  read_io_channel  set_io_channel_param  set_io_device_param  write_io_channel  get_chapter_info  get_keywords  get_operator_info  get_operator_name  get_param_info  get_param_names  get_param_num  get_param_types  query_operator_info  query_param_info  search_operator  broadcast_condition  clear_barrier  clear_condition  clear_event  clear_message  clear_message_queue  clear_mutex  create_barrier  create_condition  create_event  create_message  create_message_queue  create_mutex  dequeue_message  enqueue_message  get_message_obj  get_message_param  get_message_queue_param  get_message_tuple  get_threading_attrib  lock_mutex  set_message_obj  set_message_param  set_message_queue_param  set_message_tuple  signal_condition  signal_event  timed_wait_condition  try_lock_mutex  try_wait_event  unlock_mutex  wait_barrier  wait_condition  wait_event  count_seconds  get_system_time  system_call  wait_seconds  get_aop_info  optimize_aop  query_aop_info  read_aop_knowledge  set_aop_info  write_aop_knowledge  get_system  set_system  clear_serial  close_serial  get_serial_param  open_serial  read_serial  set_serial_param  write_serial  clear_serialized_item  create_serialized_item_ptr  fread_serialized_item  fwrite_serialized_item  get_serialized_item_ptr  close_socket  get_next_socket_data_type  get_socket_descriptor  get_socket_param  open_socket_accept  open_socket_connect  receive_data  receive_image  receive_region  receive_serialized_item  receive_tuple  receive_xld  send_data  send_image  send_region  send_serialized_item  send_tuple  send_xld  set_socket_param  socket_accept_connect  close_bg_esti  create_bg_esti  get_bg_esti_params  give_bg_esti  run_bg_esti  set_bg_esti_params  update_bg_esti  abs_funct_1d  compose_funct_1d  create_funct_1d_array  create_funct_1d_pairs  derivate_funct_1d  distance_funct_1d  funct_1d_to_pairs  get_pair_funct_1d  get_y_value_funct_1d  integrate_funct_1d  invert_funct_1d  local_min_max_funct_1d  match_funct_1d_trans  negate_funct_1d  num_points_funct_1d  read_funct_1d  sample_funct_1d  scale_y_funct_1d  smooth_funct_1d_gauss  smooth_funct_1d_mean  transform_funct_1d  write_funct_1d  x_range_funct_1d  y_range_funct_1d  zero_crossings_funct_1d  angle_ll  angle_lx  apply_distance_transform_xld  clear_distance_transform_xld  create_distance_transform_xld  deserialize_distance_transform_xld  distance_cc  distance_cc_min  distance_contours_xld  distance_lc  distance_lr  distance_pc  distance_pl  distance_pp  distance_pr  distance_ps  distance_rr_min  distance_rr_min_dil  distance_sc  distance_sl  distance_sr  distance_ss  get_distance_transform_xld_contour  get_distance_transform_xld_param  get_points_ellipse  intersection_circle_contour_xld  intersection_circles  intersection_contours_xld  intersection_line_circle  intersection_line_contour_xld  intersection_lines  intersection_segment_circle  intersection_segment_contour_xld  intersection_segment_line  intersection_segments  projection_pl  read_distance_transform_xld  serialize_distance_transform_xld  set_distance_transform_xld_param  write_distance_transform_xld  connect_grid_points  create_rectification_grid  find_rectification_grid  gen_arbitrary_distortion_map  gen_grid_rectification_map  hough_circle_trans  hough_circles  hough_line_trans  hough_line_trans_dir  hough_lines  hough_lines_dir  select_matching_lines  clear_scattered_data_interpolator  create_scattered_data_interpolator  interpolate_scattered_data  interpolate_scattered_data_image  interpolate_scattered_data_points_to_image  filter_kalman  read_kalman  update_kalman  approx_chain  approx_chain_simple  line_orientation  line_position  partition_lines  select_lines  select_lines_longest  adjust_mosaic_images  bundle_adjust_mosaic  gen_bundle_adjusted_mosaic  gen_cube_map_mosaic  gen_projective_mosaic  gen_spherical_mosaic  proj_match_points_distortion_ransac  proj_match_points_distortion_ransac_guided  proj_match_points_ransac  proj_match_points_ransac_guided  affine_trans_pixel  affine_trans_point_2d  deserialize_hom_mat2d  hom_mat2d_compose  hom_mat2d_determinant  hom_mat2d_identity  hom_mat2d_invert  hom_mat2d_reflect  hom_mat2d_reflect_local  hom_mat2d_rotate  hom_mat2d_rotate_local  hom_mat2d_scale  hom_mat2d_scale_local  hom_mat2d_slant  hom_mat2d_slant_local  hom_mat2d_to_affine_par  hom_mat2d_translate  hom_mat2d_translate_local  hom_mat2d_transpose  hom_mat3d_project  hom_vector_to_proj_hom_mat2d  point_line_to_hom_mat2d  projective_trans_pixel  projective_trans_point_2d  serialize_hom_mat2d  vector_angle_to_rigid  vector_field_to_hom_mat2d  vector_to_aniso  vector_to_hom_mat2d  vector_to_proj_hom_mat2d  vector_to_proj_hom_mat2d_distortion  vector_to_rigid  vector_to_similarity  affine_trans_point_3d  deserialize_hom_mat3d  hom_mat3d_compose  hom_mat3d_determinant  hom_mat3d_identity  hom_mat3d_invert  hom_mat3d_rotate  hom_mat3d_rotate_local  hom_mat3d_scale  hom_mat3d_scale_local  hom_mat3d_to_pose  hom_mat3d_translate  hom_mat3d_translate_local  hom_mat3d_transpose  pose_to_hom_mat3d  projective_trans_hom_point_3d  projective_trans_point_3d  serialize_hom_mat3d  vector_to_hom_mat3d  convert_point_3d_cart_to_spher  convert_point_3d_spher_to_cart  convert_pose_type  create_pose  deserialize_pose  get_circle_pose  get_pose_type  get_rectangle_pose  pose_average  pose_compose  pose_invert  pose_to_quat  proj_hom_mat2d_to_pose  quat_to_pose  read_pose  serialize_pose  set_origin_pose  vector_to_pose  write_pose  axis_angle_to_quat  deserialize_quat  quat_compose  quat_conjugate  quat_interpolate  quat_normalize  quat_rotate_point_3d  quat_to_hom_mat3d  serialize_quat  tuple_abs  tuple_acos  tuple_add  tuple_asin  tuple_atan  tuple_atan2  tuple_ceil  tuple_cos  tuple_cosh  tuple_cumul  tuple_deg  tuple_div  tuple_exp  tuple_fabs  tuple_floor  tuple_fmod  tuple_ldexp  tuple_log  tuple_log10  tuple_max2  tuple_min2  tuple_mod  tuple_mult  tuple_neg  tuple_pow  tuple_rad  tuple_sgn  tuple_sin  tuple_sinh  tuple_sqrt  tuple_sub  tuple_tan  tuple_tanh  tuple_band  tuple_bnot  tuple_bor  tuple_bxor  tuple_lsh  tuple_rsh  tuple_equal  tuple_equal_elem  tuple_greater  tuple_greater_elem  tuple_greater_equal  tuple_greater_equal_elem  tuple_less  tuple_less_elem  tuple_less_equal  tuple_less_equal_elem  tuple_not_equal  tuple_not_equal_elem  tuple_chr  tuple_chrt  tuple_int  tuple_is_number  tuple_number  tuple_ord  tuple_ords  tuple_real  tuple_round  tuple_string  tuple_concat  tuple_gen_const  tuple_gen_sequence  tuple_rand  tuple_inverse  tuple_sort  tuple_sort_index  tuple_deviation  tuple_histo_range  tuple_length  tuple_max  tuple_mean  tuple_median  tuple_min  tuple_sum  tuple_and  tuple_not  tuple_or  tuple_xor  tuple_insert  tuple_remove  tuple_replace  tuple_find  tuple_find_first  tuple_find_last  tuple_first_n  tuple_last_n  tuple_select  tuple_select_mask  tuple_select_range  tuple_select_rank  tuple_str_bit_select  tuple_uniq  tuple_difference  tuple_intersection  tuple_symmdiff  tuple_union  tuple_environment  tuple_regexp_match  tuple_regexp_replace  tuple_regexp_select  tuple_regexp_test  tuple_split  tuple_str_first_n  tuple_str_last_n  tuple_strchr  tuple_strlen  tuple_strrchr  tuple_strrstr  tuple_strstr  tuple_substr  tuple_is_int  tuple_is_int_elem  tuple_is_mixed  tuple_is_real  tuple_is_real_elem  tuple_is_string  tuple_is_string_elem  tuple_type  tuple_type_elem  get_contour_xld  get_lines_xld  get_parallels_xld  get_polygon_xld  gen_circle_contour_xld  gen_contour_nurbs_xld  gen_contour_polygon_rounded_xld  gen_contour_polygon_xld  gen_contour_region_xld  gen_contours_skeleton_xld  gen_cross_contour_xld  gen_ellipse_contour_xld  gen_nurbs_interp  gen_parallels_xld  gen_polygons_xld  gen_rectangle2_contour_xld  mod_parallels_xld  area_center_points_xld  area_center_xld  circularity_xld  compactness_xld  contour_point_num_xld  convexity_xld  diameter_xld  dist_ellipse_contour_points_xld  dist_ellipse_contour_xld  dist_rectangle2_contour_points_xld  eccentricity_points_xld  eccentricity_xld  elliptic_axis_points_xld  elliptic_axis_xld  fit_circle_contour_xld  fit_ellipse_contour_xld  fit_line_contour_xld  fit_rectangle2_contour_xld  get_contour_angle_xld  get_contour_attrib_xld  get_contour_global_attrib_xld  get_regress_params_xld  info_parallels_xld  length_xld  local_max_contours_xld  max_parallels_xld  moments_any_points_xld  moments_any_xld  moments_points_xld  moments_xld  orientation_points_xld  orientation_xld  query_contour_attribs_xld  query_contour_global_attribs_xld  select_contours_xld  select_shape_xld  select_xld_point  smallest_circle_xld  smallest_rectangle1_xld  smallest_rectangle2_xld  test_closed_xld  test_self_intersection_xld  test_xld_point  affine_trans_contour_xld  affine_trans_polygon_xld  gen_parallel_contour_xld  polar_trans_contour_xld  polar_trans_contour_xld_inv  projective_trans_contour_xld  difference_closed_contours_xld  difference_closed_polygons_xld  intersection_closed_contours_xld  intersection_closed_polygons_xld  symm_difference_closed_contours_xld  symm_difference_closed_polygons_xld  union2_closed_contours_xld  union2_closed_polygons_xld  add_noise_white_contour_xld  clip_contours_xld  clip_end_points_contours_xld  close_contours_xld  combine_roads_xld  crop_contours_xld  merge_cont_line_scan_xld  regress_contours_xld  segment_contour_attrib_xld  segment_contours_xld  shape_trans_xld  smooth_contours_xld  sort_contours_xld  split_contours_xld  union_adjacent_contours_xld  union_cocircular_contours_xld  union_collinear_contours_ext_xld  union_collinear_contours_xld  union_cotangential_contours_xld  union_straight_contours_xld  ";

    }

    
}
