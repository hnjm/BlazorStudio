﻿using System.Collections.Immutable;
using BlazorStudio.ClassLib.CommonComponents;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.InputFile;
using BlazorStudio.ClassLib.TreeViewImplementations;

namespace BlazorStudio.ClassLib.Store.InputFileCase;

public partial record InputFileState
{
    public record RequestInputFileStateFormAction(
        string Message,
        Func<IAbsoluteFilePath?, Task> OnAfterSubmitFunc,
        Func<IAbsoluteFilePath?, Task<bool>> SelectionIsValidFunc,
        ImmutableArray<InputFilePattern> InputFilePatterns);
    
    public record SetSelectedTreeViewModelAction(
        TreeViewAbsoluteFilePath? SelectedTreeViewModel);
    
    public record SetOpenedTreeViewModelAction(
        TreeViewAbsoluteFilePath TreeViewModel,
        ICommonComponentRenderers CommonComponentRenderers);
    
    public record SetSelectedInputFilePatternAction(
        InputFilePattern InputFilePattern);
    
    public record SetSearchQueryAction(
        string SearchQuery);

    public record MoveBackwardsInHistoryAction;
    public record MoveForwardsInHistoryAction;
    public record OpenParentDirectoryAction(ICommonComponentRenderers CommonComponentRenderers);
    public record RefreshCurrentSelectionAction;
    
    public record StartInputFileStateFormAction(
        RequestInputFileStateFormAction RequestInputFileStateFormAction);
}