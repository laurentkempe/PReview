using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using PReview.Git;

namespace PReview
{
    [SolutionTreeFilterProvider(PullRequestFilterPackageGuids.guidPullRequestFilterPackageCmdSetString, PullRequestFilterPackageGuids.PullRequestFilterId)]
    [Export]
    public class PullRequestFilterProvider : HierarchyTreeFilterProvider
    {
        private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;
        private readonly SVsServiceProvider _svcProvider;
        public Dictionary<string, UnifiedDiff> UnifiedDiffs = new Dictionary<string, UnifiedDiff>();

        [ImportingConstructor]
        public PullRequestFilterProvider(SVsServiceProvider serviceProvider, IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
        {
            _svcProvider = serviceProvider;
            _hierarchyCollectionProvider = hierarchyCollectionProvider;
        }

        protected override HierarchyTreeFilter CreateFilter()
        {
            return new PullRequestFilter(_svcProvider, _hierarchyCollectionProvider, this);
        }

        private sealed class PullRequestFilter : HierarchyTreeFilter
        {
            private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;
            private readonly PullRequestFilterProvider _pullRequestFilterProvider;
            private readonly DiffParser _diffParser;

            public PullRequestFilter(IServiceProvider serviceProvider, IVsHierarchyItemCollectionProvider hierarchyCollectionProvider, PullRequestFilterProvider pullRequestFilterProvider)
            {
                _hierarchyCollectionProvider = hierarchyCollectionProvider;
                _pullRequestFilterProvider = pullRequestFilterProvider;

                var dte = (DTE)serviceProvider.GetService(typeof(DTE));
                var solutionDir = Path.GetDirectoryName(dte.Solution.FullName);

                _diffParser = new DiffParser(solutionDir);
            }

            // Gets the items to be included from this filter provider.
            // rootItems is a collection that contains the root of your solution
            // Returns a collection of items to be included as part of the filter
            protected override async Task<IReadOnlyObservableSet> GetIncludedItemsAsync(IEnumerable<IVsHierarchyItem> rootItems)
            {
                var root = HierarchyUtilities.FindCommonAncestor(rootItems);
                var sourceItems = await _hierarchyCollectionProvider.GetDescendantsAsync(root.HierarchyIdentity.NestedHierarchy, CancellationToken);

                //_pullRequestFilterProvider.UnifiedDiffs.Clear();

                _pullRequestFilterProvider.UnifiedDiffs = await _diffParser.ParseAsync();

                return await _hierarchyCollectionProvider.GetFilteredHierarchyItemsAsync(sourceItems, ShouldIncludeInFilter, CancellationToken);
            }

            // Returns true if filters hierarchy item name for given filter; otherwise, false</returns>
            private bool ShouldIncludeInFilter(IVsHierarchyItem hierarchyItem)
            {
                if (hierarchyItem?.CanonicalName == null) return false;

                UnifiedDiff diff;
                return _pullRequestFilterProvider.UnifiedDiffs.TryGetValue(hierarchyItem.CanonicalName, out diff);
            }
        }
    }
}