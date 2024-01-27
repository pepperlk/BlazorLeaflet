using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BlazorLeaflet
{
    public class LeafletJsInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        private IJSRuntime _jsRuntime;

        public LeafletJsInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/BlazorLeaflet/leaflet.js").AsTask());
            _jsRuntime = jsRuntime;
        }

        public async ValueTask Create(Map map)
        {

            map._jsRuntime = _jsRuntime;

            var css = new Uri("https://unpkg.com/leaflet@1.9.4/dist/"
                           + "leaflet.css", UriKind.Absolute).ToString();
            var js = new Uri("https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/leaflet.js", UriKind.Absolute).ToString();


            var module = await moduleTask.Value;

            map.module = module;

            await module.InvokeVoidAsync("addScriptIfNotExists", js);
            await module.InvokeVoidAsync("addCssInNotExists", css);


            await module.InvokeVoidAsync("create", map, DotNetObjectReference.Create(map));
        }

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}

namespace System
{
    public static class LeafletExtensions
    {

        public static IServiceCollection AddLeaflet(this IServiceCollection services)
        {
            services.AddScoped<BlazorLeaflet.LeafletJsInterop>();


            return services;
        }
    }
}