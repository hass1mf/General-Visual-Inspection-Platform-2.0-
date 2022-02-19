﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.TypeSystem;

namespace ICSharpCode.CodeCompletion.DataItems
{
    /// <summary>
    /// Item for 'override' completion.
    /// </summary>
    internal class OverrideCompletionData : EntityCompletionData
    {
        readonly int declarationBegin;
        readonly CSharpTypeResolveContext contextAtCaret;

        public OverrideCompletionData(int declarationBegin, IMember m, CSharpTypeResolveContext contextAtCaret)
            : base(m)
        {
            this.declarationBegin = declarationBegin;
            this.contextAtCaret = contextAtCaret;
            var ambience = new CSharpAmbience
            {
                ConversionFlags =
                    ConversionFlags.ShowTypeParameterList | ConversionFlags.ShowParameterList |
                    ConversionFlags.ShowParameterNames
            };
            this.CompletionText = ambience.ConvertSymbol(m);
        }

        #region Complete Override
        public override void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            if (declarationBegin > completionSegment.Offset)
            {
                base.Complete(textArea, completionSegment, insertionRequestEventArgs);
                return;
            }
            var b = new TypeSystemAstBuilder(new CSharpResolver(contextAtCaret))
            {
                ShowTypeParameterConstraints = false,
                GenerateBody = true
            };

            var entityDeclaration = b.ConvertEntity(this.Entity);
            entityDeclaration.Modifiers &= ~(Modifiers.Virtual | Modifiers.Abstract);
            entityDeclaration.Modifiers |= Modifiers.Override;

            if (!this.Entity.IsAbstract)
            {
                // modify body to call the base method
                if (this.Entity.SymbolKind == SymbolKind.Method)
                {
                    var baseCall = new BaseReferenceExpression().Invoke(this.Entity.Name, ParametersToExpressions(this.Entity));
                    var body = entityDeclaration.GetChildByRole(Roles.Body);
                    body.Statements.Clear();
                    if (((IMethod)this.Entity).ReturnType.IsKnownType(KnownTypeCode.Void))
                        body.Statements.Add(new ExpressionStatement(baseCall));
                    else
                        body.Statements.Add(new ReturnStatement(baseCall));
                }
                else if (this.Entity.SymbolKind == SymbolKind.Indexer || this.Entity.SymbolKind == SymbolKind.Property)
                {
                    Expression baseCall;
                    if (this.Entity.SymbolKind == SymbolKind.Indexer)
                        baseCall = new BaseReferenceExpression().Indexer(ParametersToExpressions(this.Entity));
                    else
                        baseCall = new BaseReferenceExpression().Member(this.Entity.Name);
                    var getterBody = entityDeclaration.GetChildByRole(PropertyDeclaration.GetterRole).Body;
                    if (!getterBody.IsNull)
                    {
                        getterBody.Statements.Clear();
                        getterBody.Add(new ReturnStatement(baseCall.Clone()));
                    }
                    var setterBody = entityDeclaration.GetChildByRole(PropertyDeclaration.SetterRole).Body;
                    if (!setterBody.IsNull)
                    {
                        setterBody.Statements.Clear();
                        setterBody.Add(new AssignmentExpression(baseCall.Clone(), new IdentifierExpression("value")));
                    }
                }
            }

            var document = textArea.Document;
            StringWriter w = new StringWriter();
            var formattingOptions = FormattingOptionsFactory.CreateSharpDevelop();
            var segmentDict = SegmentTrackingOutputFormatter.WriteNode(w, entityDeclaration, formattingOptions, textArea.Options);

            string newText = w.ToString().TrimEnd();
            document.Replace(declarationBegin, completionSegment.EndOffset - declarationBegin, newText);
            var throwStatement = entityDeclaration.Descendants.FirstOrDefault(n => n is ThrowStatement);
            if (throwStatement != null)
            {
                var segment = segmentDict[throwStatement];
                textArea.Selection = new RectangleSelection(textArea, new TextViewPosition(textArea.Document.GetLocation(declarationBegin + segment.Offset)), new TextViewPosition(textArea.Document.GetLocation(declarationBegin + segment.Offset + segment.Length)));
            }

            //format the inserted code nicely
            var formatter = new CSharpFormatter(formattingOptions);
            formatter.AddFormattingRegion(new DomRegion(document.GetLocation(declarationBegin), document.GetLocation(declarationBegin + newText.Length)));
            var syntaxTree = new CSharpParser().Parse(document);
            formatter.AnalyzeFormatting(document, syntaxTree).ApplyChanges();
        }

        IEnumerable<Expression> ParametersToExpressions(IEntity entity)
        {
            foreach (var p in ((IParameterizedMember)entity).Parameters)
            {
                if (p.IsRef || p.IsOut)
                    yield return new DirectionExpression(p.IsOut ? FieldDirection.Out : FieldDirection.Ref, new IdentifierExpression(p.Name));
                else
                    yield return new IdentifierExpression(p.Name);
            }
        }
        #endregion
    }//end class OverrideCompletionData
}
