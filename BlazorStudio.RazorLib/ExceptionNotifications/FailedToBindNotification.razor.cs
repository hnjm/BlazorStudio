﻿using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Store.NotificationCase;
using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace BlazorStudio.RazorLib.ExceptionNotifications;

public partial class FailedToBindNotification : ComponentBase
{
    [CascadingParameter]
    public NotificationRecord NotificationRecord { get; set; } = null!;

    [Parameter, EditorRequired]
    public IAbsoluteFilePath ProjectAbsoluteFilePath { get; set; } = null!;
}