﻿using System.Collections.Immutable;
using BlazorALaCarte.DialogNotification.Notification;
using BlazorALaCarte.DialogNotification.Store.NotificationCase;
using BlazorStudio.ClassLib.CommonComponents;
using BlazorStudio.ClassLib.FileSystem.Classes;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Store.InputFileCase;
using BlazorStudio.ClassLib.TreeViewImplementations;
using BlazorTextEditor.RazorLib;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorStudio.RazorLib.InputFile;

public partial class InputFileTopNavBar : ComponentBase
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private ICommonComponentRenderers CommonComponentRenderers { get; set; } = null!;

    [CascadingParameter(Name="SetInputFileContentTreeViewRoot")]
    public Action<IAbsoluteFilePath> SetInputFileContentTreeViewRoot { get; set; } = null!;
    [CascadingParameter]
    public InputFileState InputFileState { get; set; } = null!;
    
    public ElementReference? SearchElementReference { get; private set; }
    private string _searchQuery = string.Empty;
    private bool _showInputTextEditForAddress;

    public string SearchQuery
    {
        get => _searchQuery;
        set => Dispatcher
            .Dispatch(
                new InputFileState.SetSearchQueryAction(
                    value));
    }

    private void HandleBackButtonOnClick()
    {
        Dispatcher.Dispatch(new InputFileState.MoveBackwardsInHistoryAction());

        ChangeContentRootToOpenedTreeView(InputFileState);
    }
    
    private void HandleForwardButtonOnClick()
    {
        Dispatcher.Dispatch(new InputFileState.MoveForwardsInHistoryAction());
        
        ChangeContentRootToOpenedTreeView(InputFileState);
    }

    private void HandleUpwardButtonOnClick()
    {
        Dispatcher.Dispatch(new InputFileState.OpenParentDirectoryAction(
            CommonComponentRenderers));
        
        ChangeContentRootToOpenedTreeView(InputFileState);
    }

    private void HandleRefreshButtonOnClick()
    {
        Dispatcher.Dispatch(new InputFileState.RefreshCurrentSelectionAction());
        
        ChangeContentRootToOpenedTreeView(InputFileState);
    }

    private void FocusSearchElementReferenceOnClick()
    {
        SearchElementReference?.FocusAsync();
    }

    private void ChangeContentRootToOpenedTreeView(
        InputFileState inputFileState)
    {
        var openedTreeView = InputFileState.GetOpenedTreeView();
        
        if (openedTreeView?.Item is not null)
            SetInputFileContentTreeViewRoot.Invoke(openedTreeView.Item);
    }
    
    private void InputFileEditAddressOnFocusOutCallback(string address)
    {
        address = FilePathHelper.StripEndingDirectorySeparatorIfExists(
            address);

        try
        {
            if (!Directory.Exists(address))
            {
                if (System.IO.File.Exists(address))
                {
                    throw new ApplicationException(
                        $"Address provided was a file. Provide a directory instead. {address}");
                }
                
                throw new ApplicationException(
                    $"Address provided does not exist. {address}");
            }
            
            var absoluteFilePath = new AbsoluteFilePath(address, true);
            
            _showInputTextEditForAddress = false;
            
            SetInputFileContentTreeViewRoot.Invoke(absoluteFilePath);
        }
        catch (Exception exception)
        {
            var errorNotification = new NotificationRecord(
                NotificationKey.NewNotificationKey(),
                $"ERROR: {nameof(InputFileTopNavBar)}",
                CommonComponentRenderers.ErrorNotificationRendererType,
                new Dictionary<string, object?>
                {
                    {
                        nameof(IErrorNotificationRendererType.Message),
                        exception.ToString()
                    }
                },
                TimeSpan.FromSeconds(12));
            
            Dispatcher.Dispatch(
                new NotificationRecordsCollection.RegisterAction(
                    errorNotification));
        }
    }
    
    private void HideInputFileEditAddress()
    {
        _showInputTextEditForAddress = false;
        InvokeAsync(StateHasChanged);
    }
}