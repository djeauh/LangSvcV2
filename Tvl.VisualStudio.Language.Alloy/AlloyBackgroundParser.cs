﻿namespace Tvl.VisualStudio.Language.Alloy
{
    using System;
    using System.Collections.Generic;
    using Antlr.Runtime;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Text;
    using Tvl.VisualStudio.Language.Parsing;
    using Tvl.VisualStudio.Shell.OutputWindow;

    public class AlloyBackgroundParser : BackgroundParser
    {
        public AlloyBackgroundParser(ITextBuffer textBuffer, IOutputWindowService outputWindowService)
            : base(textBuffer)
        {
            this.OutputWindowService = outputWindowService;
        }

        public IOutputWindowService OutputWindowService
        {
            get;
            private set;
        }

        protected override void ReParseImpl()
        {
            var outputWindow = OutputWindowService.TryGetPane(AlloyConstants.AntlrIntellisenseOutputWindow);
            try
            {
                var snapshot = TextBuffer.CurrentSnapshot;
                SnapshotCharStream input = new SnapshotCharStream(snapshot);
                AlloyLexer lexer = new AlloyLexer(input);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                AlloyParser parser = new AlloyParser(tokens);
                List<ParseErrorEventArgs> errors = new List<ParseErrorEventArgs>();
                parser.ParseError += (sender, e) =>
                    {
                        errors.Add(e);

                        string message = e.Message;

                        ITextDocument document;
                        if (TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out document) && document != null)
                        {
                            string fileName = document.FilePath;
                            var line = snapshot.GetLineFromPosition(e.Span.Start);
                            message = string.Format("{0}({1},{2}): {3}", fileName, line.LineNumber + 1, e.Span.Start - line.Start.Position + 1, message);
                        }

                        if (message.Length > 100)
                            message = message.Substring(0, 100) + " ...";

                        if (outputWindow != null)
                            outputWindow.WriteLine(message);
                    };

                var result = parser.compilationUnit();
                OnParseComplete(new AntlrParseResultEventArgs(snapshot, errors, tokens.GetTokens(), result));
            }
            catch (Exception e)
            {
                if (ErrorHandler.IsCriticalException(e))
                    throw;

                try
                {
                    if (outputWindow != null)
                        outputWindow.WriteLine(e.Message);
                }
                catch (Exception ex2)
                {
                    if (ErrorHandler.IsCriticalException(ex2))
                        throw;
                }
            }
        }
    }
}