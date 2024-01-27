using BlazorLeaflet.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Rectangle = BlazorLeaflet.Models.Rectangle;

namespace BlazorLeaflet
{
    public static class LeafletInterops
    {

        private static ConcurrentDictionary<string, (IDisposable, string, Layer)> LayerReferences { get; }
            = new ConcurrentDictionary<string, (IDisposable, string, Layer)>();

        private static readonly string _BaseObjectContainer = "window.leafletBlazor";

        public static async Task Create(IJSRuntime jsRuntime, Map map)
        {





            // add cdn refrences to leaflet and leaflet css
            var css= new Uri("https://unpkg.com/leaflet@1.9.4/dist/"
                               + "leaflet.css", UriKind.Absolute).ToString();
            var js = new Uri("https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/leaflet.js", UriKind.Absolute).ToString();

            // add cdn refrences to leaflet and leaflet css

             await jsRuntime.InvokeVoidAsync("blazorExtensions.AddStylesheet", css);
            await jsRuntime.InvokeVoidAsync("blazorExtensions.AddScript", js);




            await  jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.create", map, DotNetObjectReference.Create(map));
        }

        private static DotNetObjectReference<T> CreateLayerReference<T>(string mapId, T layer) where T : Layer
        {
            var result = DotNetObjectReference.Create(layer);
            LayerReferences.TryAdd(layer.Id, (result, mapId, layer));
            return result;
        }

        private static void DisposeLayerReference(string layerId)
        {
            if (LayerReferences.TryRemove(layerId, out var value))
                value.Item1.Dispose();
        }

        public static ValueTask AddLayer(IJSRuntime jsRuntime, string mapId, Layer layer)
        {
            return layer switch
            {
                TileLayer tileLayer => jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.addTilelayer", mapId, tileLayer, CreateLayerReference(mapId, tileLayer)),
                MbTilesLayer mbTilesLayer => jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.addMbTilesLayer", mapId, mbTilesLayer, CreateLayerReference(mapId, mbTilesLayer)),
                ShapefileLayer shapefileLayer => jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.addShapefileLayer", mapId, shapefileLayer, CreateLayerReference(mapId, shapefileLayer)),
                Marker marker => jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.addMarker", mapId, marker, CreateLayerReference(mapId, marker)),
                Rectangle rectangle => jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.addRectangle", mapId, rectangle, CreateLayerReference(mapId, rectangle)),
                Circle circle => jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.addCircle", mapId, circle, CreateLayerReference(mapId, circle)),
                Polygon polygon => jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.addPolygon", mapId, polygon, CreateLayerReference(mapId, polygon)),
                Polyline polyline => jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.addPolyline", mapId, polyline, CreateLayerReference(mapId, polyline)),
                ImageLayer image => jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.addImageLayer", mapId, image, CreateLayerReference(mapId, image)),
                GeoJsonDataLayer geo => jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.addGeoJsonLayer", mapId, geo, CreateLayerReference(mapId, geo)),
                _ => throw new NotImplementedException($"The layer {typeof(Layer).Name} has not been implemented."),
            };
        }

        public static async ValueTask RemoveLayer(IJSRuntime jsRuntime, string mapId, string layerId)
        {
            await jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.removeLayer", mapId, layerId);
            DisposeLayerReference(layerId);
        }

        public static ValueTask UpdatePopupContent(IJSRuntime jsRuntime, string mapId, Layer layer) =>
            jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.updatePopupContent", mapId, layer.Id, layer.Popup?.Content);

        public static ValueTask UpdateTooltipContent(IJSRuntime jsRuntime, string mapId, Layer layer) =>
            jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.updateTooltipContent", mapId, layer.Id, layer.Tooltip?.Content);

        public static ValueTask UpdateShape(IJSRuntime jsRuntime, string mapId, Layer layer) =>
            layer switch
            {
                Rectangle rectangle => jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.updateRectangle", mapId, rectangle),
                Circle circle => jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.updateCircle", mapId, circle),
                Polygon polygon => jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.updatePolygon", mapId, polygon),
                Polyline polyline => jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.updatePolyline", mapId, polyline),
                _ => throw new NotImplementedException($"The layer {typeof(Layer).Name} has not been implemented."),
            };

        public static ValueTask FitBounds(IJSRuntime jsRuntime, string mapId, PointF corner1, PointF corner2, PointF? padding, float? maxZoom) =>
            jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.fitBounds", mapId, corner1, corner2, padding, maxZoom);

        public static ValueTask PanTo(IJSRuntime jsRuntime, string mapId, PointF position, bool animate, float duration, float easeLinearity, bool noMoveStart) =>
            jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.panTo", mapId, position, animate, duration, easeLinearity, noMoveStart);

        public static ValueTask<LatLng> GetCenter(IJSRuntime jsRuntime, string mapId) =>
            jsRuntime.InvokeAsync<LatLng>($"{_BaseObjectContainer}.getCenter", mapId);

        public static ValueTask<float> GetZoom(IJSRuntime jsRuntime, string mapId) =>
            jsRuntime.InvokeAsync<float>($"{_BaseObjectContainer}.getZoom", mapId);

        public static ValueTask ZoomIn(IJSRuntime jsRuntime, string mapId, MouseEventArgs e) =>
            jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.zoomIn", mapId, e);

        public static ValueTask ZoomOut(IJSRuntime jsRuntime, string mapId, MouseEventArgs e) =>
            jsRuntime.InvokeVoidAsync($"{_BaseObjectContainer}.zoomOut", mapId, e);
    }
}
