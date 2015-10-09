using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;

namespace PReview.Core
{
    internal interface IMarginCore
    {
        event EventHandler BrushesChanged;

        FontFamily FontFamily { get; }
        FontStretch FontStretch { get; }
        FontStyle FontStyle { get; }
        FontWeight FontWeight { get; }
        double FontSize { get; }
        Brush Background { get; }
        Brush Foreground { get; }
        Brush AdditionBrush { get; }
        Brush ModificationBrush { get; }
        Brush RemovedBrush { get; }
        double EditorChangeLeft { get; }
        double EditorChangeWidth { get; }
        double ScrollChangeWidth { get; }
        void MoveToChange(int lineNumber);
        ITextDocument GetTextDocument();
    }
}