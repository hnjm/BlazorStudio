﻿using Blazor.Text.Editor.Analysis.Shared;
using BlazorTextEditor.RazorLib.Lexing;

namespace Blazor.Text.Editor.Analysis.Html.ClassLib;

public class TextEditorHtmlDiagnosticBag : TextEditorDiagnosticBag
{
    public void ReportTagNameMissing(TextEditorTextSpan textEditorTextSpan)
    {
        Report(
            DiagnosticLevel.Error,
            "Missing tag name.",
            textEditorTextSpan);
    }
}