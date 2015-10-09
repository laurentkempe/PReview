using System;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using PReview.Core;

namespace PReview.ViewModel
{
    internal abstract class DiffMarginViewModelBase : ViewModelBase
    {
        protected readonly IMarginCore MarginCore;

        protected DiffMarginViewModelBase(IMarginCore marginCore)
        {
            if (marginCore == null)
                throw new ArgumentNullException(nameof(marginCore));

            MarginCore = marginCore;

            DiffViewModels = new ObservableCollection<DiffViewModel>();
        }

        public ObservableCollection<DiffViewModel> DiffViewModels { get; }

        public void RefreshDiffViewModelPositions()
        {
            foreach (var diffViewModel in DiffViewModels)
            {
                diffViewModel.RefreshPosition();
            }
        }
    }
}