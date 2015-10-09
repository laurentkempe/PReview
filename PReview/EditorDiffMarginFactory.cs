#region using

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

#endregion

namespace PReview
{
    [Export(typeof (IWpfTextViewMarginProvider))]
    [Name(EditorDiffMargin.MarginNameConst)]
    [Order(Before = PredefinedMarginNames.LineNumber)]
    [MarginContainer(PredefinedMarginNames.LeftSelection)]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class EditorDiffMarginFactory : DiffMarginFactoryBase
    {
        private readonly PullRequestFilterProvider _pullRequestFilterProvider;

        [ImportingConstructor]
        public EditorDiffMarginFactory(PullRequestFilterProvider  pullRequestFilterProvider)
        {
            _pullRequestFilterProvider = pullRequestFilterProvider;
        }

        public override IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            var marginCore = TryGetMarginCore(textViewHost);
            if (marginCore == null)
                return null;

            ITextDocument textDocument;
            if (TextDocumentFactoryService.TryGetTextDocument(textViewHost.TextView.TextBuffer, out textDocument))
            {
                var filePath = textDocument.FilePath.ToLower();

                UnifiedDiff diff;
                if (_pullRequestFilterProvider.UnifiedDiffs.TryGetValue(filePath, out diff))
                {
                    return new EditorDiffMargin(textViewHost.TextView, diff, marginCore);
                }
            }

            return null;
        }
    }
}