﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.NRefactory.Editor;

namespace ICSharpCode.CodeCompletion
{
    public class CodeTextEditor : AvalonEdit.TextEditor
    {
        protected CompletionWindow completionWindow;
        protected OverloadInsightWindow insightWindow;

        public CodeTextEditor()
        {
            TextArea.TextEntering += OnTextEntering;
            TextArea.TextEntered += OnTextEntered;
            ShowLineNumbers = true;


            var ctrlSpace = new RoutedCommand();
            ctrlSpace.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.Control));
            var cb = new CommandBinding(ctrlSpace, OnCtrlSpaceCommand);

            this.CommandBindings.Add(cb);
        }

        public CSharpCompletion Completion { get; set; }

        #region Open & Save File
        public string FileName { get; set; }


        public void OpenFile(string fileName)
        {
            if (!System.IO.File.Exists(fileName))
                throw new FileNotFoundException(fileName);

            if (completionWindow != null)
                completionWindow.Close();
            if (insightWindow != null)
                insightWindow.Close();

            FileName = fileName;
            Load(fileName);
            Document.FileName = FileName;

            SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(fileName));
        }

        public void SetCsharpText(string text)
        {

            if (completionWindow != null)
                completionWindow.Close();
            if (insightWindow != null)
                insightWindow.Close();

            Document.FileName = "随便给个名字吧,好多地方代码要判断";
            this.Text = text;
            this.IsModified = false;

           // SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".csx");
           SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
        }


        public void NewFile(string fileName, string text)
        {
            if (completionWindow != null)
                completionWindow.Close();
            if (insightWindow != null)
                insightWindow.Close();

            FileName = fileName;
            Text = text;
            Document.FileName = FileName;

            SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(fileName));
        }

        public bool SaveFile()
        {
            if (String.IsNullOrEmpty(FileName))
                return false;

            Save(FileName);
            return true;
        }
        #endregion


        #region Code Completion
        private void OnTextEntered(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            ShowCompletion(textCompositionEventArgs.Text, false);
        }

        private void OnCtrlSpaceCommand(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            ShowCompletion(null, true);
        }

        private void ShowCompletion(string enteredText, bool controlSpace)
        {
            //if (!controlSpace)
            //    //Debug.WriteLine("Code Completion: TextEntered: " + enteredText);
            //else
            //    //Debug.WriteLine("Code Completion: Ctrl+Space");

            //only process csharp files and if there is a code completion engine available
            //if (String.IsNullOrEmpty(Document.FileName))
            //{
            //    //Debug.WriteLine("No document file name, cannot run code completion");
            //    return;
            //}


            if (Completion == null)
            {
                //Debug.WriteLine("Code completion is null, cannot run code completion");
                return;
            }


            if (completionWindow == null)
            {
                CodeCompletionResult results = null;
                try
                {
                    var offset = 0;
                    var doc = GetCompletionDocument(out offset);
                    results = Completion.GetCompletions(doc, offset, controlSpace);
                }
                catch (Exception exception)
                {
                   Debug.WriteLine("Error in getting completion: " + exception);
                }
                if (results == null)
                    return;

                if (insightWindow == null && results.OverloadProvider != null)
                {
                    insightWindow = new OverloadInsightWindow(TextArea);
                    insightWindow.Provider = results.OverloadProvider;
                    insightWindow.Show();
                    insightWindow.Closed += (o, args) => insightWindow = null;
                    return;
                }

                if (completionWindow == null && results != null && results.CompletionData.Any())
                {
                    // Open code completion after the user has pressed dot:
                    completionWindow = new CompletionWindow(TextArea);
                    completionWindow.CloseWhenCaretAtBeginning = controlSpace;
                    completionWindow.StartOffset -= results.TriggerWordLength;
                    //completionWindow.EndOffset -= results.TriggerWordLength;

                    IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                    foreach (var completion in results.CompletionData.OrderBy(item => item.Text))
                    {
                        data.Add(completion);
                    }
                    if (results.TriggerWordLength > 0)
                    {
                        //completionWindow.CompletionList.IsFiltering = false;
                        completionWindow.CompletionList.SelectItem(results.TriggerWord);
                    }
                    completionWindow.Show();
                    completionWindow.Closed += (o, args) => completionWindow = null;
                }
            }//end if


            //update the insight window
            if (!string.IsNullOrEmpty(enteredText) && insightWindow != null)
            {
                //whenver text is entered update the provider
                var provider = insightWindow.Provider as CSharpOverloadProvider;
                if (provider != null)
                {
                    //since the text has not been added yet we need to tread it as if the char has already been inserted
                    var offset = 0;
                    var doc = GetCompletionDocument(out offset);
                    provider.Update(doc, offset);
                    //if the windows is requested to be closed we do it here
                    if (provider.RequestClose)
                    {
                        insightWindow.Close();
                        insightWindow = null;
                    }
                }
            }
        }//end method

        private void OnTextEntering(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            //Debug.WriteLine("TextEntering: " + textCompositionEventArgs.Text);
            if (textCompositionEventArgs.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(textCompositionEventArgs.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(textCompositionEventArgs);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        /// <summary>
        /// Gets the document used for code completion, can be overridden to provide a custom document
        /// </summary>
        /// <param name="offset"></param>
        /// <returns>The document of this text editor.</returns>
        protected virtual IDocument GetCompletionDocument(out int offset)
        {
            offset = CaretOffset;
            return Document;
        }
        #endregion


    }
}
