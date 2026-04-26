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
        internal readonly struct RequestData
        {
            public string Url { get; }
            public string Method { get; }
            public string Body { get; }
            public string ContentType { get; }

            public RequestData(string url, string method, string body, string contentType)
            {
                Url = url;
                Method = method;
                Body = body;
                ContentType = contentType;
            }
        }

        internal static Func<RequestData, CancellationToken, Awaitable<RestResponse>> SendImplementation { get; set; }
            = DefaultSendImplementation;

        public static Awaitable<RestResponse> Get(string url)
            => Send(url, UnityWebRequest.kHttpVerbGET, null, null, CancellationToken.None);

        public static Awaitable<RestResponse> Post(string url, string body)
            => Send(url, UnityWebRequest.kHttpVerbPOST, body, "application/json", CancellationToken.None);

        public static Awaitable<RestResponse> Get(string url, CancellationToken cancellationToken)
            => Send(url, UnityWebRequest.kHttpVerbGET, null, null, cancellationToken);

        public static Awaitable<RestResponse> Post(string url, string body, CancellationToken cancellationToken)
            => Send(url, UnityWebRequest.kHttpVerbPOST, body, "application/json", cancellationToken);
        
        private static Awaitable<RestResponse> Send(
            string url,
            string method,
            string body,
            string contentType,
            CancellationToken cancellationToken)
        {
            return SendImplementation(new RequestData(url, method, body, contentType), cancellationToken);
        }

        private static async Awaitable<RestResponse> DefaultSendImplementation(
            RequestData requestData,
            CancellationToken cancellationToken)
        {
            using var request = new UnityWebRequest(requestData.Url, requestData.Method)
            {
                downloadHandler = new DownloadHandlerBuffer()
            };

            if (requestData.Body != null)
            {
                var bytes = Encoding.UTF8.GetBytes(requestData.Body);
                request.uploadHandler = new UploadHandlerRaw(bytes);

                if (!string.IsNullOrEmpty(requestData.ContentType))
                    request.SetRequestHeader("Content-Type", requestData.ContentType);
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
