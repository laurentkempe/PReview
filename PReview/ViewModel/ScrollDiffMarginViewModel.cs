using System;
using System.Linq;
using PReview.Core;
using PReview.Git;

namespace PReview.ViewModel
{
    internal class ScrollDiffMarginViewModel : DiffMarginViewModelBase
    {
        private readonly Action<DiffViewModel, HunkRangeInfo> _updateDiffDimensions;

        internal ScrollDiffMarginViewModel(IMarginCore marginCore, UnifiedDiff unifiedDiff, Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions)
            : base(marginCore)
        {
            if (updateDiffDimensions == null)
                throw new ArgumentNullException(nameof(updateDiffDimensions));

            _updateDiffDimensions = updateDiffDimensions;

            foreach (var diffViewModel in unifiedDiff.HunkRanges.Select(CreateDiffViewModel))
            {
                DiffViewModels.Add(diffViewModel);
            }
        }

        protected virtual DiffViewModel CreateDiffViewModel(HunkRangeInfo hunkRangeInfo)
        {
            return new EditorDiffViewModel(hunkRangeInfo, MarginCore, _updateDiffDimensions);
        }

    }
}