using System.Collections.Generic;

namespace Vetcat.AwaitableRestClient
{
    public readonly struct RestResponse
    {
        /// <summary>
        /// HTTP status code returned by the server.
        /// Zero if no response was received (network error, DNS failure, aborted request, etc.).
        /// </summary>
        public long StatusCode { get; }

        /// <summary>
        /// Raw response body as string. Can be null if no download handler was assigned
        /// or the response body was empty.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Error message reported by UnityWebRequest.error.
        /// Null if Unity considers request successful.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Response headers. Can be null if no headers were received.
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; }

        /// <summary>
        /// True if UnityWebRequest.result == Success
        /// (usually indicates HTTP 2xx status code).
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// True if UnityWebRequest reported a connection or data processing error.
        /// Indicates a low-level networking failure (DNS, socket, SSL, timeout, etc.),
        /// not an HTTP failure (4xx / 5xx).
        /// </summary>
        public bool IsNetworkError { get; }

        /// <summary>
        /// True if the response body exists and is not empty.
        /// </summary>
        public bool HasBody => !string.IsNullOrEmpty(Text);

        public RestResponse(
            long statusCode,
            string text,
            string error,
            IReadOnlyDictionary<string, string> headers,
            bool isSuccess,
            bool isNetworkError)
        {
            StatusCode = statusCode;
            Text = text;
            Error = error;
            Headers = headers;
            IsSuccess = isSuccess;
            IsNetworkError = isNetworkError;
        }
    }
}