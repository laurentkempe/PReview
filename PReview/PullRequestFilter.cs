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
    public class PullRequestFilterProvider : HierarchyTreeFilterProvider
    {
        private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;
        private readonly SVsServiceProvider _svcProvider;

        [ImportingConstructor]
        public PullRequestFilterProvider(SVsServiceProvider serviceProvider, IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
        {
            _svcProvider = serviceProvider;
            _hierarchyCollectionProvider = hierarchyCollectionProvider;
        }

        protected override HierarchyTreeFilter CreateFilter()
        {
            return new PullRequestFilter(_svcProvider, _hierarchyCollectionProvider);
        }

        private sealed class PullRequestFilter : HierarchyTreeFilter
        {
            private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;
            private readonly DiffParser _diffParser;
            private Dictionary<string, UnifiedDiff> _parseResult;

            public PullRequestFilter(IServiceProvider serviceProvider, IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
            {
                _hierarchyCollectionProvider = hierarchyCollectionProvider;

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

                _parseResult = await _diffParser.ParseAsync();

                return await _hierarchyCollectionProvider.GetFilteredHierarchyItemsAsync(sourceItems, ShouldIncludeInFilter, CancellationToken);
            }

            // Returns true if filters hierarchy item name for given filter; otherwise, false</returns>
            private bool ShouldIncludeInFilter(IVsHierarchyItem hierarchyItem)
            {
                if (hierarchyItem?.CanonicalName == null) return false;

                UnifiedDiff diff;
                return _parseResult.TryGetValue(hierarchyItem.CanonicalName, out diff);
            }
        }
    }
}