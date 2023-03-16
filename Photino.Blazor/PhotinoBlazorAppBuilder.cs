﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Photino.Blazor;

public class PhotinoBlazorAppBuilder
{
    internal PhotinoBlazorAppBuilder()
    {
        RootComponents = new RootComponentList();
        Services = new ServiceCollection();
    }

    public RootComponentList RootComponents { get; }

    public IServiceCollection Services { get; }

    public static PhotinoBlazorAppBuilder CreateDefault(string[] args = default)
    {
        // We don't use the args for anything right now, but we want to accept them
        // here so that it shows up this way in the project templates.
        // var jsRuntime = DefaultWebAssemblyJSRuntime.Instance;
        var builder = new PhotinoBlazorAppBuilder();
        builder.Services
            .AddScoped(sp => new HttpClient(new PhotinoHttpHandler(sp.GetService<PhotinoBlazorApp>()))
                { BaseAddress = new Uri(PhotinoWebViewManager.AppBaseUri) })
            .AddSingleton<PhotinoBlazorApp>()
            .AddBlazorWebView();

        // Right now we don't have conventions or behaviors that are specific to this method
        // however, making this the default for the template allows us to add things like that
        // in the future, while giving `new BlazorDesktopHostBuilder` as an opt-out of opinionated
        // settings.
        return builder;
    }

    public PhotinoBlazorApp Build(Action<IServiceProvider> serviceProviderOptions = null)
    {
        var sp = Services.BuildServiceProvider();
        var app = sp.GetService<PhotinoBlazorApp>();

        serviceProviderOptions?.Invoke(sp);

        app.Initialize(sp, RootComponents);
        return app;
    }
}

public class RootComponentList : IEnumerable<(Type, string)>
{
    private readonly List<(Type componentType, string domElementSelector)> _components = new();

    public IEnumerator<(Type, string)> GetEnumerator()
    {
        return _components.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _components.GetEnumerator();
    }

    public void Add<TComponent>(string selector) where TComponent : Microsoft.AspNetCore.Components.IComponent
    {
        _components.Add((typeof(TComponent), selector));
    }
}