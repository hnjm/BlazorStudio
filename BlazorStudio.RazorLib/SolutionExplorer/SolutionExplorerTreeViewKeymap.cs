﻿using BlazorALaCarte.DialogNotification.Notification;
using BlazorALaCarte.DialogNotification.Store.NotificationCase;
using BlazorALaCarte.Shared.Keyboard;
using BlazorALaCarte.Shared.Menu;
using BlazorALaCarte.TreeView.BaseTypes;
using BlazorALaCarte.TreeView.Commands;
using BlazorALaCarte.TreeView.Keymap;
using BlazorALaCarte.TreeView.Services;
using BlazorStudio.ClassLib.CommonComponents;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Namespaces;
using BlazorStudio.ClassLib.Store.EditorCase;
using BlazorStudio.ClassLib.Store.TerminalCase;
using BlazorStudio.ClassLib.TreeViewImplementations;
using BlazorTextEditor.RazorLib;
using Fluxor;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorStudio.RazorLib.SolutionExplorer;

public class SolutionExplorerTreeViewKeymap : ITreeViewKeymap
{
    private readonly IState<TerminalSessionsState> _terminalSessionsStateWrap;
    private BlazorStudio.ClassLib.Menu.ICommonMenuOptionsFactory _commonMenuOptionsFactory;
    private ICommonComponentRenderers _commonComponentRenderers;
    private IDispatcher _dispatcher;
    private readonly ITreeViewService _treeViewService;
    private readonly ITextEditorService _textEditorService;

    public SolutionExplorerTreeViewKeymap(
        IState<TerminalSessionsState> terminalSessionsStateWrap,
        BlazorStudio.ClassLib.Menu.ICommonMenuOptionsFactory commonMenuOptionsFactory,
        ICommonComponentRenderers commonComponentRenderers,
        IDispatcher dispatcher,
        ITreeViewService treeViewService,
        ITextEditorService textEditorService)
    {
        _terminalSessionsStateWrap = terminalSessionsStateWrap;
        _commonMenuOptionsFactory = commonMenuOptionsFactory;
        _commonComponentRenderers = commonComponentRenderers;
        _dispatcher = dispatcher;
        _treeViewService = treeViewService;
        _textEditorService = textEditorService;
    }
    
    public bool TryMapKey(
        KeyboardEventArgs keyboardEventArgs, 
        out TreeViewCommand? treeViewCommand)
    {
        switch (keyboardEventArgs.Code)
        {
            case KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE:
                treeViewCommand = new TreeViewCommand(InvokeOpenInEditor);
                return true;
        }
        
        if (keyboardEventArgs.CtrlKey)
            return CtrlModifiedKeymap(keyboardEventArgs, out treeViewCommand);

        if (keyboardEventArgs.AltKey)
            return AltModifiedKeymap(keyboardEventArgs, out treeViewCommand);

        treeViewCommand = null;
        return false;
    }

    private bool CtrlModifiedKeymap(
        KeyboardEventArgs keyboardEventArgs,
        out TreeViewCommand? treeViewCommand)
    {
        if (keyboardEventArgs.AltKey)
            return CtrlAltModifiedKeymap(keyboardEventArgs, out treeViewCommand);

        TreeViewCommand? command = null;
        
        switch (keyboardEventArgs.Key)
        {
            case "c":
                command = new TreeViewCommand(InvokeCopyFile);
                break;
            case "x":
                command = new TreeViewCommand(InvokeCutFile);
                break;
            case "v":
                command = new TreeViewCommand(InvokePasteClipboard);
                break;
            // case "a":
            //     command = TreeViewCommandFacts.SelectAll;
            //     break;
        }

        if (command is null)
        {
            switch (keyboardEventArgs.Code)
            {
                // Here to illustrate future usage
                case KeyboardKeyFacts.WhitespaceCodes.ENTER_CODE:
                    break;
            }
        }

        treeViewCommand = command;
        
        if (treeViewCommand is null)
            return false;
        
        return true;
    }

    /// <summary>
    ///     Do not go from <see cref="AltModifiedKeymap" /> to
    ///     <see cref="CtrlAltModifiedKeymap" />
    ///     <br /><br />
    ///     Code in this method should only be here if it
    ///     does not include a Ctrl key being pressed.
    ///     <br /><br />
    ///     As otherwise, we'd have to permute over
    ///     all the possible keyboard modifier
    ///     keys and have a method for each permutation.
    /// </summary>
    private bool AltModifiedKeymap(KeyboardEventArgs keyboardEventArgs,
        out TreeViewCommand? treeViewCommand)
    {
        treeViewCommand = null;
        return false;
    }

    private bool CtrlAltModifiedKeymap(KeyboardEventArgs keyboardEventArgs,
        out TreeViewCommand? treeViewCommand)
    {
        treeViewCommand = null;
        return false;
    }
    
    private Task NotifyCopyCompleted(NamespacePath namespacePath)
    {
        var notificationInformative  = new NotificationRecord(
            NotificationKey.NewNotificationKey(), 
            "Copy Action",
            _commonComponentRenderers.InformativeNotificationRendererType,
            new Dictionary<string, object?>
            {
                {
                    nameof(IInformativeNotificationRendererType.Message), 
                    $"Copied: {namespacePath.AbsoluteFilePath.FilenameWithExtension}"
                },
            },
            TimeSpan.FromSeconds(3));
        
        _dispatcher.Dispatch(
            new NotificationRecordsCollection.RegisterAction(
                notificationInformative));

        return Task.CompletedTask;
    }
    
    private Task NotifyCutCompleted(
        NamespacePath namespacePath,
        TreeViewNamespacePath? parentTreeViewModel)
    {
        SolutionExplorerContextMenu.ParentOfCutFile = parentTreeViewModel;
        
        var notificationInformative  = new NotificationRecord(
            NotificationKey.NewNotificationKey(), 
            "Cut Action",
            _commonComponentRenderers.InformativeNotificationRendererType,
            new Dictionary<string, object?>
            {
                {
                    nameof(IInformativeNotificationRendererType.Message), 
                    $"Cut: {namespacePath.AbsoluteFilePath.FilenameWithExtension}"
                },
            },
            TimeSpan.FromSeconds(3));
        
        _dispatcher.Dispatch(
            new NotificationRecordsCollection.RegisterAction(
                notificationInformative));

        return Task.CompletedTask;
    }

    private Task InvokeCopyFile(ITreeViewCommandParameter treeViewCommandParameter)
    {
        var activeNode = treeViewCommandParameter.TreeViewState.ActiveNode;

        if (activeNode is null ||
            activeNode is not TreeViewNamespacePath treeViewNamespacePath ||
            treeViewNamespacePath.Item is null)
        {
            return Task.CompletedTask;
        }

        var copyFileMenuOption = _commonMenuOptionsFactory.CopyFile(
            treeViewNamespacePath.Item.AbsoluteFilePath,
            () => NotifyCopyCompleted(treeViewNamespacePath.Item));

        copyFileMenuOption.OnClick?.Invoke();
        
        return Task.CompletedTask;
    }  
    
    private Task InvokePasteClipboard(ITreeViewCommandParameter treeViewCommandParameter)
    {
        var activeNode = treeViewCommandParameter.TreeViewState.ActiveNode;

        if (activeNode is null ||
            activeNode is not TreeViewNamespacePath treeViewNamespacePath ||
            treeViewNamespacePath.Item is null)
        {
            return Task.CompletedTask;
        }

        MenuOptionRecord pasteMenuOptionRecord;

        if (treeViewNamespacePath.Item.AbsoluteFilePath.IsDirectory)
        {
            pasteMenuOptionRecord = _commonMenuOptionsFactory.PasteClipboard(
                treeViewNamespacePath.Item.AbsoluteFilePath,
                async () =>
                {
                    var localParentOfCutFile =
                        SolutionExplorerContextMenu.ParentOfCutFile;

                    SolutionExplorerContextMenu.ParentOfCutFile = null;

                    if (localParentOfCutFile is not null)
                        await ReloadTreeViewModel(localParentOfCutFile);

                    await ReloadTreeViewModel(treeViewNamespacePath);
                });
        }
        else
        {
            var parentDirectory = (IAbsoluteFilePath)treeViewNamespacePath
                .Item.AbsoluteFilePath.Directories.Last();

            pasteMenuOptionRecord = _commonMenuOptionsFactory.PasteClipboard(
                parentDirectory,
                async () =>
                {
                    var localParentOfCutFile =
                        SolutionExplorerContextMenu.ParentOfCutFile;

                    SolutionExplorerContextMenu.ParentOfCutFile = null;

                    if (localParentOfCutFile is not null)
                        await ReloadTreeViewModel(localParentOfCutFile);

                    await ReloadTreeViewModel(treeViewNamespacePath);
                });
        }

        pasteMenuOptionRecord.OnClick?.Invoke();
        return Task.CompletedTask;
    }
    
    private Task InvokeCutFile(ITreeViewCommandParameter treeViewCommandParameter)
    {
        var activeNode = treeViewCommandParameter.TreeViewState.ActiveNode;

        if (activeNode is null ||
            activeNode is not TreeViewNamespacePath treeViewNamespacePath ||
            treeViewNamespacePath.Item is null)
        {
            return Task.CompletedTask;
        }

        var parent = treeViewNamespacePath.Parent as TreeViewNamespacePath;

        MenuOptionRecord cutFileOptionRecord = _commonMenuOptionsFactory.CutFile(
            treeViewNamespacePath.Item.AbsoluteFilePath,
            () => NotifyCutCompleted(
                treeViewNamespacePath.Item, 
                parent));

        cutFileOptionRecord.OnClick?.Invoke();
        return Task.CompletedTask;
    }
    
    private async Task InvokeOpenInEditor(ITreeViewCommandParameter treeViewCommandParameter)
    {
        var activeNode = treeViewCommandParameter.TreeViewState.ActiveNode;

        if (activeNode is null ||
            activeNode is not TreeViewNamespacePath treeViewNamespacePath ||
            treeViewNamespacePath.Item is null)
        {
            return;
        }
        
        await EditorState.OpenInEditorAsync(
            treeViewNamespacePath.Item.AbsoluteFilePath,
            _dispatcher,
            _textEditorService,
            _commonComponentRenderers);
    }
    
    private async Task ReloadTreeViewModel(
        TreeViewNoType? treeViewModel)
    {
        if (treeViewModel is null)
            return;

        await treeViewModel.LoadChildrenAsync();
        
        _treeViewService.ReRenderNode(
            SolutionExplorerDisplay.TreeViewSolutionExplorerStateKey, 
            treeViewModel);
        
        _treeViewService.MoveActiveSelectionUp(
            SolutionExplorerDisplay.TreeViewSolutionExplorerStateKey);
    }
}