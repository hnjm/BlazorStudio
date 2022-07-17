﻿using BlazorStudio.ClassLib.Family;
using BlazorStudio.ClassLib.Store.ThemeCase;
using BlazorStudio.ClassLib.Store.TreeViewCase;
using BlazorStudio.ClassLib.UserInterface;
using Fluxor;
using Microsoft.AspNetCore.Components;

namespace BlazorStudio.RazorLib.Shared;

public partial class BodyDisplay : ComponentBase
{
    private Dimensions _topDimensions = new Dimensions
    {
        DimensionsPositionKind = DimensionsPositionKind.Static,
        WidthCalc = new List<DimensionUnit>
        {
            new()
            {
                DimensionUnitKind = DimensionUnitKind.Percentage,
                Value = 100
            }
        },
        HeightCalc = new List<DimensionUnit>
        {
            new()
            {
                DimensionUnitKind = DimensionUnitKind.Percentage,
                Value = 33.33
            },
            new()
            {
                DimensionUnitKind = DimensionUnitKind.Pixels,
                Value = -2.66
            },
        }
    };

    private Dimensions _middleDimensions = new Dimensions
    {
        DimensionsPositionKind = DimensionsPositionKind.Static,
        WidthCalc = new List<DimensionUnit>
        {
            new()
            {
                DimensionUnitKind = DimensionUnitKind.Percentage,
                Value = 100
            }
        },
        HeightCalc = new List<DimensionUnit>
        {
            new()
            {
                DimensionUnitKind = DimensionUnitKind.Percentage,
                Value = 33.33
            },
            new()
            {
                DimensionUnitKind = DimensionUnitKind.Pixels,
                Value = -2.66
            },
        }
    };

    private Dimensions _bottomDimensions = new Dimensions
    {
        DimensionsPositionKind = DimensionsPositionKind.Static,
        WidthCalc = new List<DimensionUnit>
        {
            new()
            {
                DimensionUnitKind = DimensionUnitKind.Percentage,
                Value = 100
            }
        },
        HeightCalc = new List<DimensionUnit>
        {
            new()
            {
                DimensionUnitKind = DimensionUnitKind.Percentage,
                Value = 33.33
            },
            new()
            {
                DimensionUnitKind = DimensionUnitKind.Pixels,
                Value = -2.66
            },
        }
    };

    private async Task ReRender()
    {
        await InvokeAsync(StateHasChanged);
    }
}