using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace PReview
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(ScrollDiffMargin.MarginNameConst + "2013")]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [MarginContainer(PredefinedMarginNames.VerticalScrollBar)]
    [Order(After = "OverviewChangeTrackingMargin")]
    [Order(Before = "OverviewErrorMargin")]
    [Order(Before = "OverviewMarkMargin")]
    [Order(Before = "OverviewSourceImageMargin")]
    internal sealed class ScrollDiffMarginFactory2013 : DiffMarginFactoryBase
    {
        private readonly PullRequestFilterProvider _pullRequestFilterProvider;

        [ImportingConstructor]
        public ScrollDiffMarginFactory2013(PullRequestFilterProvider pullRequestFilterProvider)
        {
            _pullRequestFilterProvider = pullRequestFilterProvider;
        }

        public override IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            // Visual Studio uses assembly binding redirection for the Shell assembly.
            if (typeof(ErrorHandler).Assembly.GetName().Version.Major < 12)
                return null;

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
                    return new ScrollDiffMargin(textViewHost.TextView, diff, marginCore, containerMargin);
                }
            }

            return null;
        }
    }
}
