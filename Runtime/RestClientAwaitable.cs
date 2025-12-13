using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AwaitableRestClient
{
    public static class RestClientAwaitable
    {
        public static Awaitable<RestResponse> Get(string url)
            => Send(url, UnityWebRequest.kHttpVerbGET, null, null, CancellationToken.None);

        public static Awaitable<RestResponse> Post(string url, string body)
            => Send(url, UnityWebRequest.kHttpVerbPOST, body, "application/json", CancellationToken.None);

        public static Awaitable<RestResponse> Get(string url, CancellationToken cancellationToken)
            => Send(url, UnityWebRequest.kHttpVerbGET, null, null, cancellationToken);

        public static Awaitable<RestResponse> Post(string url, string body, CancellationToken cancellationToken)
            => Send(url, UnityWebRequest.kHttpVerbPOST, body, "application/json", cancellationToken);
        
        private static async Awaitable<RestResponse> Send(
            string url,
            string method,
            string body,
            string contentType,
            CancellationToken cancellationToken)
        {
            using var request = new UnityWebRequest(url, method)
            {
                downloadHandler = new DownloadHandlerBuffer()
            };

            if (body != null)
            {
                var bytes = Encoding.UTF8.GetBytes(body);
                request.uploadHandler = new UploadHandlerRaw(bytes);

                if (!string.IsNullOrEmpty(contentType))
                    request.SetRequestHeader("Content-Type", contentType);
            }

            using (cancellationToken.Register(() => request.Abort()))
            {
                await request.SendWebRequest();
            }

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(cancellationToken);

            var headers = request.GetResponseHeaders();
            IReadOnlyDictionary<string, string> readOnlyHeaders = headers;

            bool isSuccess = request.result == UnityWebRequest.Result.Success;
            bool isNetworkError =
                request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.DataProcessingError;

            string text = request.downloadHandler != null
                ? request.downloadHandler.text
                : null;
            
            return new RestResponse(
                statusCode: request.responseCode,
                text: text,
                error: request
                    .error, 
                headers: readOnlyHeaders,
                isSuccess: isSuccess,
                isNetworkError: isNetworkError
            );
        }
    }
}

