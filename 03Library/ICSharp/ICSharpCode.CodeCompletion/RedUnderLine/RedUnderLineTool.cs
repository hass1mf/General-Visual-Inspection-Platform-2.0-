using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.AddIn;
using ICSharpCode.SharpDevelop.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICSharpCode.CodeCompletion.Sample.RedUnderLine
{
    /// <summary>
    /// 用户给控件画下划线
    /// </summary>
    public class RedUnderLineTool
    {
        private TextEditor m_TextEditor;
        private ITextMarkerService m_TextMarkerService;

        private bool m_IsHasMark = false;//是否已经有划线
        // 初始化
        public void InitializeTextMarkerService(TextEditor textEditor)
        {
            m_TextEditor = textEditor;
            //
            var textMarkerService = new TextMarkerService(m_TextEditor.Document);
            m_TextEditor.TextArea.TextView.BackgroundRenderers.Add(textMarkerService);
            m_TextEditor.TextArea.TextView.LineTransformers.Add(textMarkerService);
            IServiceContainer services = (IServiceContainer)m_TextEditor.Document.ServiceProvider.GetService(typeof(IServiceContainer));
            if (services != null)
                services.AddService(typeof(ITextMarkerService), textMarkerService);
            m_TextMarkerService = textMarkerService;
        }
        public  void RemoveAll()
        {
            if (m_IsHasMark==true)
            {
                m_IsHasMark = false;
                m_TextMarkerService.RemoveAll(m => true);
            }
      
        }

        //public  void RemoveSelected()
        //{
        //    m_IsHasMark = false;
        //    m_TextMarkerService.RemoveAll(IsSelected);
        //}
         bool IsSelected(ITextMarker marker)
        {
            int selectionEndOffset = m_TextEditor.SelectionStart + m_TextEditor.SelectionLength;
            if (marker.StartOffset >= m_TextEditor.SelectionStart && marker.StartOffset <= selectionEndOffset)
                return true;
            if (marker.EndOffset >= m_TextEditor.SelectionStart && marker.EndOffset <= selectionEndOffset)
                return true;
            return false;
        }

        public  void AddMarkerFromSelection()
        {
            m_IsHasMark = true;
            ITextMarker marker = m_TextMarkerService.Create(m_TextEditor.SelectionStart, m_TextEditor.SelectionLength);
            marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
            marker.MarkerColor = System.Windows.Media.Colors.Red;
        }
    }
}
