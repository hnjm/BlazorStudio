using BlazorStudio.ClassLib;
using BlazorStudio.ClassLib.CommonComponents;
using BlazorStudio.RazorLib.Clipboard;
using BlazorStudio.RazorLib.File;
using BlazorStudio.RazorLib.InputFile;
using BlazorStudio.RazorLib.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorStudio.RazorLib;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlazorTextEditorRazorLibServices(this IServiceCollection services)
    {
        var commonRendererTypes = new CommonComponentRenderers(
            typeof(InputFileDisplay),
            typeof(CommonInformativeNotificationDisplay),
            typeof(CommonErrorNotificationDisplay),
            typeof(FileFormDisplay),
            typeof(DeleteFileFormDisplay));
        
        return services
            .AddBlazorStudioClassLibServices(
                _ => new TemporaryInMemoryClipboardProvider(),
                commonRendererTypes);
    }
}