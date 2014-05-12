#region usings

using System;
using System.Net;
using System.Threading.Tasks;

#endregion

namespace AsyncWebClient
{
    public class AsyncWebClient<T> : WebClient
    {
        private TaskCompletionSource<ResponseInfo<T>> _taskCompletionSource;
        private long _totalBytesToSend;

        public Task<ResponseInfo<T>> UploadFileAsync(RequestInfo<T> info, string filepath, T context = default(T))
        {
            if (_taskCompletionSource != null)
            {
                _taskCompletionSource.TrySetCanceled();
            }
            _taskCompletionSource = new TaskCompletionSource<ResponseInfo<T>>();
            Headers.Clear();
            Headers = info.HeaderCollection;
            UploadProgressChanged += OnUploadProgressChanged;
            UploadFileCompleted += OnUploadFileCompleted;
            UploadFileAsync(info.Address, filepath, "POST", new Tuple<RequestInfo<T>, T>(info, context));
            return _taskCompletionSource.Task;
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
            _taskCompletionSource.TrySetResult(response);
        }

        protected void OnUploadProgressChanged(object sender, UploadProgressChangedEventArgs args)
        {
            _totalBytesToSend = args.TotalBytesToSend;
            var state = (Tuple<RequestInfo<T>, T>) args.UserState;
            var response = new ResponseInfo<T>(HttpStatusCode.OK, args.BytesSent, args.TotalBytesToSend, state.Item2);
            state.Item1.ProgressChangedAction(response);
        }

        private static HttpStatusCode ParseHttpStatusCode(Exception error)
        {
            var httpError = ((WebException) error);
            var rawResponse = ((HttpWebResponse) httpError.Response);
            return rawResponse == null ? HttpStatusCode.InternalServerError : rawResponse.StatusCode;
        }
    }
}