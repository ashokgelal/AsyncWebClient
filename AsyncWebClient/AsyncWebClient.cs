#region usings

using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

#endregion

namespace AsyncWebClient
{
    public class AsyncWebClient<T> : WebClient
    {
        #region Fields

        private TaskCompletionSource<ResponseInfo<T>> _downloadTaskCompletionSource;
        private long _totalBytesToReceive;
        private long _totalBytesToSend;
        private TaskCompletionSource<ResponseInfo<T>> _uploadTaskCompletionSource;

        #endregion

        #region Async Upload

        public Task<ResponseInfo<T>> UploadFileAsync(RequestInfo<T> info, string localFileAddress, T context = default(T))
        {
            if (_uploadTaskCompletionSource != null)
            {
                _uploadTaskCompletionSource.TrySetCanceled();
            }
            _uploadTaskCompletionSource = new TaskCompletionSource<ResponseInfo<T>>();
            Headers.Clear();
            Headers = info.HeaderCollection;
            UploadProgressChanged += OnUploadProgressChanged;
            UploadFileCompleted += OnUploadFileCompleted;
            UploadFileAsync(info.ServerAddress, localFileAddress, "POST", new Tuple<RequestInfo<T>, T>(info, context));
            return _uploadTaskCompletionSource.Task;
        }

        private void OnUploadFileCompleted(object sender, UploadFileCompletedEventArgs args)
        {
            UploadProgressChanged -= OnUploadProgressChanged;
            UploadFileCompleted -= OnUploadFileCompleted;
            var statuCode = (args.Error == null)
                ? HttpStatusCode.OK
                : ParseHttpStatusCode(args.Error);

            var state = (Tuple<RequestInfo<T>, T>) args.UserState;
            var response = new ResponseInfo<T>(statuCode, _totalBytesToSend, _totalBytesToSend, state.Item2) {Error = args.Error};
            _uploadTaskCompletionSource.TrySetResult(response);
        }

        protected void OnUploadProgressChanged(object sender, UploadProgressChangedEventArgs args)
        {
            _totalBytesToSend = args.TotalBytesToSend;
            var state = (Tuple<RequestInfo<T>, T>) args.UserState;
            var response = new ResponseInfo<T>(HttpStatusCode.OK, args.BytesSent, args.TotalBytesToSend, state.Item2);
            state.Item1.ProgressChangedAction(response);
        }

        #endregion

        #region Async Download

        public Task<ResponseInfo<T>> DownloadFileAsync(RequestInfo<T> info, string localFileAddress, T context = default(T))
        {
            if (_downloadTaskCompletionSource != null)
            {
                _downloadTaskCompletionSource.TrySetCanceled();
            }
            _downloadTaskCompletionSource = new TaskCompletionSource<ResponseInfo<T>>();
            Headers.Clear();
            Headers = info.HeaderCollection;
            DownloadProgressChanged += OnDownloadProgressChanged;
            DownloadFileCompleted += OnDownloadFileCompleted;
            DownloadFileAsync(info.ServerAddress, localFileAddress, new Tuple<RequestInfo<T>, T>(info, context));
            return _downloadTaskCompletionSource.Task;
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs args)
        {
            DownloadProgressChanged -= OnDownloadProgressChanged;
            DownloadFileCompleted -= OnDownloadFileCompleted;

            var statuCode = (args.Error == null)
                ? HttpStatusCode.OK
                : ParseHttpStatusCode(args.Error);

            var state = (Tuple<RequestInfo<T>, T>) args.UserState;
            var response = new ResponseInfo<T>(statuCode, _totalBytesToReceive, _totalBytesToReceive, state.Item2) {Error = args.Error};
            _downloadTaskCompletionSource.TrySetResult(response);
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            _totalBytesToReceive = args.TotalBytesToReceive;
            var state = (Tuple<RequestInfo<T>, T>) args.UserState;
            var response = new ResponseInfo<T>(HttpStatusCode.OK, args.BytesReceived, args.TotalBytesToReceive, state.Item2);
            state.Item1.ProgressChangedAction(response);
        }

        #endregion

        #region Helpers

        private static HttpStatusCode ParseHttpStatusCode(Exception error)
        {
            var httpError = ((WebException) error);
            var rawResponse = ((HttpWebResponse) httpError.Response);
            return rawResponse == null ? HttpStatusCode.InternalServerError : rawResponse.StatusCode;
        }

        #endregion
    }
}