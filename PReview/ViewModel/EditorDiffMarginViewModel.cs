#region using

using System;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using PReview.Core;
using PReview.Git;

#endregion

namespace PReview.ViewModel
{
    internal class EditorDiffMarginViewModel : DiffMarginViewModelBase
    {
        private readonly Action<DiffViewModel, HunkRangeInfo> _updateDiffDimensions;
        private RelayCommand<DiffViewModel> _previousChangeCommand;
        private RelayCommand<DiffViewModel> _nextChangeCommand;

        internal EditorDiffMarginViewModel(IMarginCore marginCore, UnifiedDiff unifiedDiff, Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions) :
            base(marginCore)
        {
            if (updateDiffDimensions == null)
                throw new ArgumentNullException(nameof(updateDiffDimensions));

            _updateDiffDimensions = updateDiffDimensions;

            foreach (var diffViewModel in unifiedDiff.HunkRanges.Select(CreateDiffViewModel))
            {
                DiffViewModels.Add(diffViewModel);
            }
        }

        public RelayCommand<DiffViewModel> PreviousChangeCommand
        {
            get { return _previousChangeCommand ?? (_previousChangeCommand = new RelayCommand<DiffViewModel>(PreviousChange, PreviousChangeCanExecute)); }
        }

        public RelayCommand<DiffViewModel> NextChangeCommand
        {
            get { return _nextChangeCommand ?? (_nextChangeCommand = new RelayCommand<DiffViewModel>(NextChange, NextChangeCanExecute)); }
        }

        private bool PreviousChangeCanExecute(DiffViewModel currentEditorDiffViewModel)
        {
            return DiffViewModels.IndexOf(currentEditorDiffViewModel) > 0;
        }

        private bool NextChangeCanExecute(DiffViewModel currentEditorDiffViewModel)
        {
            return DiffViewModels.IndexOf(currentEditorDiffViewModel) < (DiffViewModels.Count - 1);
        }

        private void PreviousChange(DiffViewModel currentEditorDiffViewModel)
        {
            MoveToChange(currentEditorDiffViewModel, -1);
        }

        private void NextChange(DiffViewModel currentEditorDiffViewModel)
        {
            MoveToChange(currentEditorDiffViewModel, +1);
        }

        public void MoveToChange(DiffViewModel currentDiffViewModel, int indexModifier)
        {
            var diffViewModelIndex = DiffViewModels.IndexOf(currentDiffViewModel) + indexModifier;
            var diffViewModel  = DiffViewModels[diffViewModelIndex];

            MarginCore.MoveToChange(diffViewModel.LineNumber);

            ((EditorDiffViewModel)currentDiffViewModel).ShowPopup = false;
        }

        protected virtual DiffViewModel CreateDiffViewModel(HunkRangeInfo hunkRangeInfo)
        {
            return new EditorDiffViewModel(hunkRangeInfo, MarginCore, _updateDiffDimensions);
        }
    }
}