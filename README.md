# !!! This is a test repository not ready for production - the task is to study the planning of writing and testing code using AI agents.

# Unity Awaitable REST Client

Lightweight UnityWebRequest wrapper based on Unity's Awaitable API.

✔ No JSON dependencies  
✔ CancellationToken support  
✔ No exceptions for HTTP 4xx/5xx  
✔ Minimal GC allocations  
✔ Full control over serialization at call site

---
### Install the _Unity Awaitable REST Client_ Package

1. Copy the git URL of this repository: `https://github.com/vetcat/unity-awaitable-rest-client.git`
2. In Unity, open the Package Manager from the menu by going to `Window > Package Manager`
3. Click the plus icon in the upper left and choose `Add package from git URL...`
4. Paste the git URL into the text field and click the `Add` button.

## Example Usage

```csharp
var cts = new CancellationTokenSource();

try
{
    RestResponse response = await RestClientAwaitable.Get("https://example.com", cts.Token);

    if (!response.IsSuccess)
    {
        Debug.LogWarning($"HTTP {response.StatusCode}: {response.Error}");
        Debug.Log(response.Text);
        return;
    }

    Debug.Log($"OK: {response.Text}");
}
catch (OperationCanceledException)
{
    Debug.Log("Request canceled");
}
```

### Supported Features

GET / POST (more verbs coming)

Cancellation via CancellationToken

Response metadata: status code, headers, body, low-level error flag

### Roadmap

Timeout helper utilities

Extra HTTP methods (PUT, PATCH, DELETE)

Custom request headers

Automatic disposal safeties in Unity Editor

Example scenes & tests

### Requirements

Unity 2022.3+
C# 8+
Awaitable API enabled
