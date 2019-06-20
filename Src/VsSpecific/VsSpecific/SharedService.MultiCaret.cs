﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.PlatformUI.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;

namespace Vim.VisualStudio.Specific
{
#if VS_SPECIFIC_2015 || VS_SPECIFIC_2017

    internal partial class SharedService
    {
        private IEnumerable<VirtualSnapshotPoint> GetCaretPoints(ITextView textView)
        {
            return new[] { textView.Caret.Position.VirtualBufferPosition };
        }

        private void SetCaretPoints(ITextView textView, IEnumerable<VirtualSnapshotPoint> caretPoints)
        {
            var caretPoint = caretPoints.First();
            textView.Caret.MoveTo(caretPoint);
        }

        private void SetSelectedSpans(ITextView textView, IEnumerable<VirtualSnapshotSpan> selectedSpans)
        {
            var selectedSpan = selectedSpans.First();
            textView.Selection.Select(selectedSpan.Start, selectedSpan.End);
        }
    }

#else

    internal partial class SharedService
    {
        private IEnumerable<VirtualSnapshotPoint> GetCaretPoints(ITextView textView)
        {
            return GetCaretPointsCommon(textView);
        }

        private void SetCaretPoints(ITextView textView, IEnumerable<VirtualSnapshotPoint> caretPoints)
        {
            SetCaretPointsCommon(textView, caretPoints.ToArray());
        }

        private void SetSelectedSpans(ITextView textView, IEnumerable<VirtualSnapshotSpan> selectedSpans)
        {
            SetSelectedSpansCommon(textView, selectedSpans.ToArray());
        }

        private IEnumerable<VirtualSnapshotPoint> GetCaretPointsCommon(ITextView textView)
        {
            var primaryCaretPoint = textView.Caret.Position.VirtualBufferPosition;
            var secondaryCaretPoints = textView.GetMultiSelectionBroker().AllSelections
                .Select(selection => selection.InsertionPoint)
                .Where(caretPoint => caretPoint != primaryCaretPoint);
            return new[] { primaryCaretPoint }.Concat(secondaryCaretPoints);
        }

        private void SetCaretPointsCommon(ITextView textView, VirtualSnapshotPoint[] caretPoints)
        {
            if (caretPoints.Length == 1)
            {
                textView.Caret.MoveTo(caretPoints[0]);
                return;
            }

            var selections = new Microsoft.VisualStudio.Text.Selection[caretPoints.Length];
            for (var caretIndex = 0; caretIndex < caretPoints.Length; caretIndex++)
            {
                selections[caretIndex] = new Microsoft.VisualStudio.Text.Selection(caretPoints[caretIndex]);
            }
            var broker = textView.GetMultiSelectionBroker();
            broker.SetSelectionRange(selections, selections[0]);
        }

        private void SetSelectedSpansCommon(ITextView textView, VirtualSnapshotSpan[] selectedSpans)
        {
            if (selectedSpans.Length == 1)
            {
                var selectedSpan = selectedSpans[0];
                textView.Selection.Select(selectedSpan.Start, selectedSpan.End);
            }

            var selections = new Microsoft.VisualStudio.Text.Selection[selectedSpans.Length];
            for (var caretIndex = 0; caretIndex < selectedSpans.Length; caretIndex++)
            {
                selections[caretIndex] = new Microsoft.VisualStudio.Text.Selection(selectedSpans[caretIndex]);
            }
            var broker = textView.GetMultiSelectionBroker();
            broker.SetSelectionRange(selections, selections[0]);
        }
    }

#endif
}
