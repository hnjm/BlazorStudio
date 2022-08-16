﻿using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorStudio.RazorLib.VirtualizeComponentExperiments;

public partial class VirtualizeCoordinateSystemExperimental<TItem> : ComponentBase, IDisposable
{
    [Inject] 
    private IJSRuntime JsRuntime { get; set; } = null!;
    
    [Parameter, EditorRequired] 
    public ICollection<TItem>? Items { get; set; } = null!;
    [Parameter, EditorRequired]
    public RenderFragment<TItem> ChildContent { get; set; } = null!;

    private Guid _virtualizeCoordinateSystemIdentifier = Guid.NewGuid();
    private VirtualizeItemDimensions? _dimensions;
    private ApplicationException _dimensionsWereNullException = new (
        $"The {nameof(_dimensions)} was null");
    private ElementReference? _topBoundaryElementReference;
    private ElementReference? _bottomBoundaryElementReference;
    private ScrollDimensions? _scrollDimensions;
    private ConcurrentStack<ScrollDimensions> _scrollEventConcurrentStack = new();
    private SemaphoreSlim _handleScrollEventSemaphoreSlim = new(1, 1);
    private TimeSpan _throttleDelayTimeSpan = TimeSpan.FromMilliseconds(100);
    private Task _throttleDelayTask = Task.CompletedTask;

    private VirtualizeRenderData<TItem> _virtualizeRenderData = new();

    protected override async Task OnParametersSetAsync()
    {
        if (_scrollDimensions is not null)
        {
            await GetResultSetAsync();
        }
        
        await base.OnParametersSetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JsRuntime.InvokeVoidAsync("plainTextEditor.subscribeToVirtualizeScrollEvent",
                _topBoundaryElementReference,
                DotNetObjectReference.Create(this));
            
            var firstScrollDimensions = await JsRuntime.InvokeAsync<ScrollDimensions>("plainTextEditor.getVirtualizeScrollDimensions",
                _topBoundaryElementReference);

            await OnParentElementScrollEvent(firstScrollDimensions);
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    [JSInvokable]
    public async Task OnParentElementScrollEvent(ScrollDimensions scrollDimensions)
    {
        // TODO: ensure this semaphore logic does not lose the most recent event in any cases.
        _scrollEventConcurrentStack.Push(scrollDimensions);

        ScrollDimensions? mostRecentScrollDimensions = null;
        
        try
        {
            await _handleScrollEventSemaphoreSlim.WaitAsync();

            await _throttleDelayTask;
            
            if (!_scrollEventConcurrentStack.TryPop(out mostRecentScrollDimensions))
            {
                return;
            }

            _scrollEventConcurrentStack.Clear();

            _throttleDelayTask = Task.Delay(_throttleDelayTimeSpan);
        }
        finally
        {
            _handleScrollEventSemaphoreSlim.Release();
        }

        _scrollDimensions = mostRecentScrollDimensions;

        await GetResultSetAsync();
    }
    
    private async Task GetResultSetAsync()
    {
        if (_dimensions is null)
            throw _dimensionsWereNullException;

        if (_scrollDimensions is null)
        {
            return;
        }
        
        var startIndex = _scrollDimensions.ScrollTop / _dimensions.HeightOfItemInPixels;
        var count = _dimensions.HeightOfScrollableContainerInPixels / _dimensions.HeightOfItemInPixels;
        
        var totalHeight = _dimensions.HeightOfItemInPixels * Items.Count;
        
        var topBoundaryHeight = _scrollDimensions.ScrollTop;
        
        var bottomBoundaryHeight = totalHeight - topBoundaryHeight - _dimensions.HeightOfScrollableContainerInPixels;

        var results = Items
            .Skip((int) (startIndex))
            .Take((int) (count))
            .Select((item, i) => 
                new VirtualizeItemWrapper<TItem>(item, 
                    topBoundaryHeight + (i * _dimensions.HeightOfItemInPixels), 
                    100))
            .ToArray();
        
        var bottomVirtualizeBoundary = _virtualizeRenderData.BottomVirtualizeBoundary with
        {
            HeightInPixels = bottomBoundaryHeight,
            OffsetFromTopInPixels = topBoundaryHeight + _dimensions.HeightOfScrollableContainerInPixels
        };
        
        var topVirtualizeBoundary = _virtualizeRenderData.TopVirtualizeBoundary with
        {
            HeightInPixels = topBoundaryHeight,
            OffsetFromTopInPixels = 0
        };

        _virtualizeRenderData = _virtualizeRenderData with
        {
            BottomVirtualizeBoundary = bottomVirtualizeBoundary,
            TopVirtualizeBoundary = topVirtualizeBoundary,
            VirtualizeItemWrappers = results
        }; 
        
        await InvokeAsync(StateHasChanged);
    }

    private void OnAfterMeasurementTaken(VirtualizeItemDimensions virtualizeItemDimensions)
    {
        _dimensions = virtualizeItemDimensions;
    }

    public void Dispose()
    {
    }
}