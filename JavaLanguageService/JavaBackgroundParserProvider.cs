﻿namespace JavaLanguageService
{
    using System;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Utilities;
    using Tvl.VisualStudio.Language.Parsing;
    using Tvl.VisualStudio.Shell.OutputWindow;

    [Export(typeof(IBackgroundParserProvider))]
    [ContentType(Constants.JavaContentType)]
    public sealed class JavaBackgroundParserProvider : IBackgroundParserProvider
    {
        [Import]
        private IOutputWindowService OutputWindowService
        {
            get;
            set;
        }

        public IBackgroundParser CreateParser(ITextBuffer textBuffer)
        {
            Func<JavaBackgroundParser> creator = () => new JavaBackgroundParser(textBuffer, OutputWindowService);
            return textBuffer.Properties.GetOrCreateSingletonProperty<JavaBackgroundParser>(creator);
        }
    }
}
