using System.Text.Json;
using Microsoft.Maui.DevFlow.Agent.Core;

namespace LeXtudio.DevFlow.Agent.Core;

public abstract class DevFlowAgentServiceBase : IDisposable
{
    private readonly AgentHttpServer _server;
    private bool _started;

    protected DevFlowAgentServiceBase(AgentOptions? options = null)
    {
        Options = options ?? new AgentOptions();
        _server = new AgentHttpServer(Options.Port);
        RegisterRoutes();
    }

    protected AgentOptions Options { get; }

    public bool IsRunning => _server.IsRunning;
    public int Port => _server.Port;

    public void Start()
    {
        if (_started) return;
        _server.Start();
        _started = true;
    }

    public Task StopAsync() => _server.StopAsync();

    protected abstract string AgentId { get; }
    protected abstract string AgentName { get; }
    protected abstract string FrameworkName { get; }
    protected abstract Task<List<ElementInfo>> BuildTreeAsync();
    protected abstract Task<ElementInfo?> FindElementAsync(string id);
    protected abstract Task<byte[]?> CaptureScreenshotAsync(string? elementId = null, string? selector = null);
    protected virtual Task<object?> GetWebViewContextsAsync() => Task.FromResult<object?>(new { contexts = Array.Empty<object>() });
    protected virtual Task<byte[]?> CaptureWebViewScreenshotAsync(string? contextId = null) => Task.FromResult<byte[]?>(null);
    protected virtual Task<object?> SendWebViewCdpCommandAsync(string? contextId, string method, JsonElement? @params) => Task.FromResult<object?>(null);
    protected abstract Task<bool> TryTapAsync(string elementId);
    protected abstract Task<bool> TryScrollAsync(string elementId, double deltaX, double deltaY);
    protected abstract Task<bool> TryFillAsync(string elementId, string text);
    protected abstract Task<bool> TryClearAsync(string elementId);
    protected abstract Task<object?> TryKeyAsync(string? elementId, string? key, string? text);
    protected abstract Task<string?> GetApplicationNameAsync();
    protected virtual object GetCapabilities() => new
    {
        screenshots = true,
        elementScreenshots = true,
        selectorScreenshots = true,
        tap = true,
        scroll = true,
        structuredErrors = true,
        webview = false,
        webviewCdp = false,
        multiWindow = false
    };

    private void RegisterRoutes()
    {
        _server.MapGet("/api/v1/agent/status", HandleStatusAsync);
        _server.MapGet("/api/v1/ui/tree", HandleTreeAsync);
        _server.MapGet("/api/v1/ui/element", HandleElementAsync);
        _server.MapGet("/api/v1/ui/screenshot", HandleScreenshotAsync);
        _server.MapGet("/api/v1/webview/contexts", HandleWebViewContextsAsync);
        _server.MapGet("/api/v1/webview/screenshot", HandleWebViewScreenshotAsync);
        _server.MapPost("/api/v1/webview/cdp", HandleWebViewCdpAsync);
        _server.MapPost("/api/v1/ui/tap", HandleTapAsync);
        _server.MapPost("/api/v1/ui/actions/fill", HandleFillAsync);
        _server.MapPost("/api/v1/ui/actions/clear", HandleClearAsync);
        _server.MapPost("/api/v1/ui/actions/key", HandleKeyAsync);
        _server.MapPost("/api/v1/ui/actions/scroll", HandleScrollAsync);
    }

    private async Task<HttpResponse> HandleStatusAsync(HttpRequest request)
    {
        var status = new
        {
            name = AgentName,
            id = AgentId,
            framework = FrameworkName,
            version = GetType().Assembly.GetName().Version?.ToString() ?? "0.0.0",
            running = true,
            port = Port,
            application = await GetApplicationNameAsync().ConfigureAwait(false),
            capabilities = GetCapabilities()
        };
        return HttpResponse.Json(status);
    }

    private async Task<HttpResponse> HandleTreeAsync(HttpRequest request)
    {
        var tree = await BuildTreeAsync().ConfigureAwait(false);
        return HttpResponse.Json(new { elements = tree });
    }

    private async Task<HttpResponse> HandleElementAsync(HttpRequest request)
    {
        if (!request.QueryParams.TryGetValue("id", out var id) || string.IsNullOrWhiteSpace(id))
            return HttpResponse.Error("Missing required query parameter 'id'", 400);

        var element = await FindElementAsync(id).ConfigureAwait(false);
        return element is null ? HttpResponse.NotFound($"Element '{id}' not found") : HttpResponse.Json(element);
    }

    private async Task<HttpResponse> HandleScreenshotAsync(HttpRequest request)
    {
        request.QueryParams.TryGetValue("id", out var elementId);
        request.QueryParams.TryGetValue("selector", out var selector);
        var bytes = await CaptureScreenshotAsync(
            string.IsNullOrWhiteSpace(elementId) ? null : elementId,
            string.IsNullOrWhiteSpace(selector) ? null : selector).ConfigureAwait(false);
        return bytes != null ? HttpResponse.Png(bytes) : HttpResponse.Error("Screenshot capture failed", 500);
    }

    private async Task<HttpResponse> HandleWebViewContextsAsync(HttpRequest request)
    {
        var contexts = await GetWebViewContextsAsync().ConfigureAwait(false);
        return HttpResponse.Json(contexts ?? new { contexts = Array.Empty<object>() });
    }

    private async Task<HttpResponse> HandleWebViewScreenshotAsync(HttpRequest request)
    {
        request.QueryParams.TryGetValue("context", out var contextId);
        var bytes = await CaptureWebViewScreenshotAsync(string.IsNullOrWhiteSpace(contextId) ? null : contextId).ConfigureAwait(false);
        return bytes != null ? HttpResponse.Png(bytes) : HttpResponse.Error("WebView screenshot capture failed", 500);
    }

    private async Task<HttpResponse> HandleWebViewCdpAsync(HttpRequest request)
    {
        var payload = request.BodyAs<WebViewCdpRequest>();
        if (payload == null || string.IsNullOrWhiteSpace(payload.Method))
            return HttpResponse.Error("Request must include a JSON body with a 'method' field", 400);

        var result = await SendWebViewCdpCommandAsync(payload.Context, payload.Method, payload.Params).ConfigureAwait(false);
        return result != null ? HttpResponse.Json(result) : HttpResponse.Error("WebView CDP command failed", 500);
    }

    private async Task<HttpResponse> HandleTapAsync(HttpRequest request)
    {
        var payload = request.BodyAs<TapRequest>();
        if (payload == null || string.IsNullOrWhiteSpace(payload.Id))
            return HttpResponse.Error("Request must include a JSON body with an 'id' field", 400);

        var result = await TryTapAsync(payload.Id).ConfigureAwait(false);
        return result ? HttpResponse.Ok() : HttpResponse.Error($"Tap target '{payload.Id}' could not be activated", 404);
    }

    private async Task<HttpResponse> HandleScrollAsync(HttpRequest request)
    {
        var payload = request.BodyAs<ScrollRequest>();
        if (payload == null || string.IsNullOrWhiteSpace(payload.Id))
            return HttpResponse.Error("Request must include a JSON body with an 'id' field", 400);

        var result = await TryScrollAsync(payload.Id, payload.DeltaX, payload.DeltaY).ConfigureAwait(false);
        return result ? HttpResponse.Ok() : HttpResponse.Error($"Scroll target '{payload.Id}' could not be scrolled", 404);
    }

    private async Task<HttpResponse> HandleFillAsync(HttpRequest request)
    {
        var payload = request.BodyAs<FillRequest>();
        if (payload == null || string.IsNullOrWhiteSpace(payload.ElementId) || payload.Text == null)
            return HttpResponse.Error("elementId and text are required", 400);

        var result = await TryFillAsync(payload.ElementId, payload.Text).ConfigureAwait(false);
        return result ? HttpResponse.Ok("Text set") : HttpResponse.Error("Element does not accept text input", 404);
    }

    private async Task<HttpResponse> HandleClearAsync(HttpRequest request)
    {
        var payload = request.BodyAs<ActionRequest>();
        if (payload == null || string.IsNullOrWhiteSpace(payload.ElementId))
            return HttpResponse.Error("elementId is required", 400);

        var result = await TryClearAsync(payload.ElementId).ConfigureAwait(false);
        return result ? HttpResponse.Ok("Cleared") : HttpResponse.Error("Element does not accept text input", 404);
    }

    private async Task<HttpResponse> HandleKeyAsync(HttpRequest request)
    {
        var payload = request.BodyAs<KeyRequest>();
        if (payload == null || (string.IsNullOrWhiteSpace(payload.Key) && string.IsNullOrWhiteSpace(payload.Text)))
            return HttpResponse.Error("key or text is required", 400);

        var result = await TryKeyAsync(payload.ElementId, payload.Key, payload.Text).ConfigureAwait(false);
        return result != null ? HttpResponse.Json(result) : HttpResponse.Error("Key action failed", 404);
    }

    private sealed class TapRequest
    {
        public string? Id { get; set; }
    }

    private sealed class ScrollRequest
    {
        public string? Id { get; set; }
        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
    }

    private sealed class FillRequest
    {
        public string? ElementId { get; set; }
        public string? Text { get; set; }
    }

    private sealed class ActionRequest
    {
        public string? ElementId { get; set; }
    }

    private sealed class KeyRequest
    {
        public string? ElementId { get; set; }
        public string? Key { get; set; }
        public string? Text { get; set; }
    }

    private sealed class WebViewCdpRequest
    {
        public string? Context { get; set; }
        public string? Method { get; set; }
        public JsonElement? Params { get; set; }
    }

    public void Dispose()
    {
        _ = _server.StopAsync();
    }
}
