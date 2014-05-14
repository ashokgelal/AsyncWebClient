#region usings

using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

#endregion

namespace AsyncWebClient
{
    /// <summary>
    ///     An WebClient that is capable of reporting progress of a file upload or
    ///     download asynchronously.  The use of this class is only appropriate when
    ///     targeting .NET 4.0. .NET 4.5's WebClient is already capable of doing what
    ///     this class does and more.
    /// </summary>
    public class AsyncWebClient<T> : WebClient
    {
        #region Fields

        private bool _disposed;
        private TaskCompletionSource<ResponseInfo<T>> _downloadTaskCompletionSource;
        private long _totalBytesToReceive;
        private long _totalBytesToSend;
        private TaskCompletionSource<ResponseInfo<T>> _uploadTaskCompletionSource;

        #endregion

        #region Async Upload

        /// <summary>
        ///     Upload a given local file asynchronously with the specified request.
        ///     The context object is passed back when reporting progress as well as when
        ///     the upload is complete.
        /// </summary>
        /// <param name="request">
        ///     A <see cref="RequestInfo{T}" /> object that contains information about
        ///     this upload request.
        /// </param>
        /// <param name="localFileAddress">The full path of the file to be upload.</param>
        /// <param name="context">
        ///     A context object that will be passed back when reporting upload progress
        ///     as well as when the upload is completed, with our without error.
        /// </param>
        /// <returns>A Task object that can be awaited.</returns>
        public Task<ResponseInfo<T>> UploadFileAsync(RequestInfo<T> request, string localFileAddress, T context = default(T))
        {
            if (_uploadTaskCompletionSource != null)
            {
                _uploadTaskCompletionSource.TrySetCanceled();
            }
            _uploadTaskCompletionSource = new TaskCompletionSource<ResponseInfo<T>>();
            Headers.Clear();
            Headers = request.HeaderCollection;
            UploadProgressChanged += OnUploadProgressChanged;
            UploadFileCompleted += OnUploadFileCompleted;
            UploadFileAsync(request.ResourceAddress, localFileAddress, "POST", new Tuple<RequestInfo<T>, T>(request, context));
            return _uploadTaskCompletionSource.Task;
        }

        protected void OnUploadFileCompleted(object sender, UploadFileCompletedEventArgs args)
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

        /// <summary>
        ///     Download a remote file asynchronously with the specified request.
        ///     The context object is passed back when reporting progress as well as when
        ///     the download is complete.
        /// </summary>
        /// <param name="request">
        ///     A <see cref="RequestInfo{T}" /> object that contains information about
        ///     this download request.
        /// </param>
        /// <param name="localFileAddress">The name of the file to be placed on the local computer.</param>
        /// <param name="context">
        ///     A context object that will be passed back when reporting download progress
        ///     as well as when the upload is completed, with our without error.
        /// </param>
        /// <returns>A Task object that can be awaited.</returns>
        public Task<ResponseInfo<T>> DownloadFileAsync(RequestInfo<T> request, string localFileAddress, T context = default(T))
        {
            if (_downloadTaskCompletionSource != null)
            {
                _downloadTaskCompletionSource.TrySetCanceled();
            }
            _downloadTaskCompletionSource = new TaskCompletionSource<ResponseInfo<T>>();
            Headers.Clear();
            Headers = request.HeaderCollection;
            DownloadProgressChanged += OnDownloadProgressChanged;
            DownloadFileCompleted += OnDownloadFileCompleted;
            DownloadFileAsync(request.ResourceAddress, localFileAddress, new Tuple<RequestInfo<T>, T>(request, context));
            return _downloadTaskCompletionSource.Task;
        }

        protected void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs args)
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

        protected void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
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

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    UploadProgressChanged -= OnUploadProgressChanged;
                    UploadFileCompleted -= OnUploadFileCompleted;
                    DownloadProgressChanged -= OnDownloadProgressChanged;
                    DownloadFileCompleted -= OnDownloadFileCompleted;
                }
                _disposed = true;
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}