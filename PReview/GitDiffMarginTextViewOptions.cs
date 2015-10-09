using Microsoft.VisualStudio.Text.Editor;

namespace PReview
{
    public static class GitDiffMarginTextViewOptions
    {
        public static EditorOptionKey<bool> DiffMarginEnabledId = new EditorOptionKey<bool>("GitDiffMargin/DiffMarginEnabled");

        public static EditorOptionKey<bool> CompareToIndexId = new EditorOptionKey<bool>("GitDiffMargin/CompareToIndex");
    }
}
