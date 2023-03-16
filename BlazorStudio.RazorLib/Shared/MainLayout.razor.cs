﻿using BlazorCommon.RazorLib.Dialog;
using BlazorCommon.RazorLib.Notification;
using BlazorCommon.RazorLib.Dimensions;
using BlazorCommon.RazorLib.Drag;
using BlazorCommon.RazorLib.Icons;
using BlazorCommon.RazorLib.Options;
using BlazorCommon.RazorLib.Resize;
using BlazorCommon.RazorLib.Store;
using BlazorCommon.RazorLib.Store.ApplicationOptions;
using BlazorCommon.RazorLib.Store.DragCase;
using BlazorCommon.RazorLib.Store.ThemeCase;
using BlazorCommon.RazorLib.Theme;
using BlazorStudio.ClassLib.Dimensions;
using BlazorStudio.ClassLib.FileSystem.Classes;
using BlazorStudio.ClassLib.FileSystem.Classes.FilePath;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Panel;
using BlazorStudio.ClassLib.Store.PanelCase;
using BlazorStudio.ClassLib.Store.SolutionExplorer;
using BlazorTextEditor.RazorLib;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorStudio.RazorLib.Shared;

public partial class MainLayout : LayoutComponentBase, IDisposable
{
    [Inject]
    private IState<DragState> DragStateWrap { get; set; } = null!;
    [Inject]
    private IState<AppOptionsState> AppOptionsStateWrap { get; set; } = null!;
    [Inject]
    private IState<PanelsCollection> PanelsCollectionWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject]
    private ITextEditorService TextEditorService { get; set; } = null!;
    [Inject]
    private IAppOptionsService AppOptionsService { get; set; } = null!;
    [Inject]
    private IEnvironmentProvider EnvironmentProvider { get; set; } = null!;

    private string _message = string.Empty;

    private string UnselectableClassCss => DragStateWrap.Value.ShouldDisplay
        ? "balc_unselectable"
        : string.Empty;

    private bool _previousDragStateWrapShouldDisplay;

    private ElementDimensions _bodyElementDimensions = new();
    private ElementDimensions _footerElementDimensions = new();

    protected override void OnInitialized()
    {
        DragStateWrap.StateChanged += DragStateWrapOnStateChanged;
        AppOptionsStateWrap.StateChanged += AppOptionsStateWrapOnStateChanged;

        var bodyHeight = _bodyElementDimensions.DimensionAttributes
            .Single(da => da.DimensionAttributeKind == DimensionAttributeKind.Height);

        bodyHeight.DimensionUnits.AddRange(new []
        {
            new DimensionUnit
            {
                Value = 78,
                DimensionUnitKind = DimensionUnitKind.Percentage
            },
            new DimensionUnit
            {
                Value = ResizableRow.RESIZE_HANDLE_HEIGHT_IN_PIXELS / 2,
                DimensionUnitKind = DimensionUnitKind.Pixels,
                DimensionOperatorKind = DimensionOperatorKind.Subtract
            },
            new DimensionUnit
            {
                Value = SizeFacts.Bstudio.Header.Height.Value / 2,
                DimensionUnitKind = SizeFacts.Bstudio.Header.Height.DimensionUnitKind,
                DimensionOperatorKind = DimensionOperatorKind.Subtract
            }
        });

        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await TextEditorService.OptionsSetFromLocalStorageAsync();
            
            if (System.IO.File.Exists("/home/hunter/Repos/Demos/TestApp/TestApp.sln"))
            {
                Dispatcher.Dispatch(new SolutionExplorerState.RequestSetSolutionExplorerStateAction(
                    new AbsoluteFilePath(
                        "/home/hunter/Repos/Demos/TestApp/TestApp.sln",
                        false,
                        EnvironmentProvider),
                    EnvironmentProvider));
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void AppOptionsStateWrapOnStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void FontStateWrapOnStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private async void DragStateWrapOnStateChanged(object? sender, EventArgs e)
    {
        if (_previousDragStateWrapShouldDisplay != DragStateWrap.Value.ShouldDisplay)
        {
            _previousDragStateWrapShouldDisplay = DragStateWrap.Value.ShouldDisplay;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ReRenderAsync()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        DragStateWrap.StateChanged -= DragStateWrapOnStateChanged;
        AppOptionsStateWrap.StateChanged -= AppOptionsStateWrapOnStateChanged;
    }
}
	