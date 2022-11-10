using BlazorStudio.ClassLib.Dimensions;
using BlazorStudio.ClassLib.Store.DialogCase;
using BlazorStudio.ClassLib.Store.DragCase;
using BlazorStudio.ClassLib.Store.FontCase;
using BlazorStudio.ClassLib.Store.ThemeCase;
using BlazorStudio.RazorLib.DialogCase;
using BlazorStudio.RazorLib.ResizableCase;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorStudio.RazorLib.Shared;

public partial class MainLayout : LayoutComponentBase, IDisposable
{
    [Inject]
    private IState<DragState> DragStateWrap { get; set; } = null!;
    [Inject]
    private IState<ThemeState> ThemeStateWrap { get; set; } = null!;
    [Inject]
    private IState<FontState> FontStateWrap { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    private string _message = string.Empty;
    
    private string UnselectableClassCss => DragStateWrap.Value.ShouldDisplay
        ? "bstudio_unselectable"
        : string.Empty;
    
    private bool _previousDragStateWrapShouldDisplay;

    private ElementDimensions _bodyElementDimensions = new();
    private ElementDimensions _footerElementDimensions = new();

    protected override void OnInitialized()
    {
        DragStateWrap.StateChanged += DragStateWrapOnStateChanged;
        ThemeStateWrap.StateChanged += ThemeStateWrapOnStateChanged;
        FontStateWrap.StateChanged += FontStateWrapOnStateChanged;
        
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

        var footerHeight = _footerElementDimensions.DimensionAttributes
            .Single(da => da.DimensionAttributeKind == DimensionAttributeKind.Height);
        
        footerHeight.DimensionUnits.AddRange(new []
        {
            new DimensionUnit
            {
                Value = 22,
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
            var value = await JsRuntime.InvokeAsync<string>(
                "blazorStudio.localStorageGetItem",
                "a");
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    private void ThemeStateWrapOnStateChanged(object? sender, EventArgs e)
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

    private void OpenDialogOnClick()
    {
        Dispatcher.Dispatch(new RegisterDialogRecordAction(new DialogRecord(
            DialogKey.NewDialogKey(), 
            "Example",
            typeof(ExampleDialog),
            new Dictionary<string, object?>
            {
                { nameof(ExampleDialog.Message), _message }
            })));
    }
    
    private async Task ReRenderAsync()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        DragStateWrap.StateChanged -= DragStateWrapOnStateChanged;
        ThemeStateWrap.StateChanged -= ThemeStateWrapOnStateChanged;
        FontStateWrap.StateChanged -= FontStateWrapOnStateChanged;
    }
}