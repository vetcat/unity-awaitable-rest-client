using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Vetcat.AwaitableRestClient
{
    public static class RestClientAwaitable
    {
        // --- Публичное API ---

        // Базовые методы без отмены

        public static Awaitable<RestResponse> Get(string url)
            => Send(url, UnityWebRequest.kHttpVerbGET, null, null, CancellationToken.None);

        public static Awaitable<RestResponse> Post(string url, string body)
            => Send(url, UnityWebRequest.kHttpVerbPOST, body, "application/json", CancellationToken.None);

        // Перегрузки с CancellationToken

        public static Awaitable<RestResponse> Get(string url, CancellationToken cancellationToken)
            => Send(url, UnityWebRequest.kHttpVerbGET, null, null, cancellationToken);

        public static Awaitable<RestResponse> Post(string url, string body, CancellationToken cancellationToken)
            => Send(url, UnityWebRequest.kHttpVerbPOST, body, "application/json", cancellationToken);

        // При желании можно добавить перегрузки с кастомным Content-Type / заголовками, но JSON внутрь не пихаем.


        // --- Внутренняя реализация ---

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

            // Привязываем отмену к Abort()
            using (cancellationToken.Register(() => request.Abort()))
            {
                await request.SendWebRequest();
            }

            // Если токен отменён — явно кидаем OperationCanceledException.
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(cancellationToken);

            // Собираем заголовки (могут быть null)
            var headers = request.GetResponseHeaders();
            IReadOnlyDictionary<string, string> readOnlyHeaders = headers;

            // Определяем тип результата
            bool isSuccess = request.result == UnityWebRequest.Result.Success;
            bool isNetworkError =
                request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.DataProcessingError;

            string text = request.downloadHandler != null
                ? request.downloadHandler.text
                : null;

            // НИКАКИХ исключений за 4xx / 5xx.
            // Просто возвращаем структурку, а вызывающий сам решает, что с этим делать.
            return new RestResponse(
                statusCode: request.responseCode,
                text: text,
                error: request
                    .error, // здесь Unity пишет описание ошибки, напр. "HTTP/1.1 404 Not Found" или сетевую ошибку
                headers: readOnlyHeaders,
                isSuccess: isSuccess,
                isNetworkError: isNetworkError
            );
        }
    }
}

