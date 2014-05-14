#region usings

using System;
using System.Net;

#endregion

namespace AsyncWebClient
{
    /// <summary>
    ///     Class that wraps information about download or upload response.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResponseInfo<T>
    {
        #region Constructor

        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="statusCode">Upload/Download status code.</param>
        /// <param name="bytesCompleted">
        ///     Bytes that is completed uploading or downloading. At the end, this value
        ///     will be same as total bytes.
        /// </param>
        /// <param name="totalBytes">
        ///     Total bytes that is supposed to be uploaded or downloaded. For a given
        ///     request, this value should never change. This value consists of both file
        ///     size and header size and so is NOT same as that of the file size.
        /// </param>
        /// <param name="context">The context for the request that initiated this upload or download.</param>
        internal ResponseInfo(HttpStatusCode statusCode, long bytesCompleted, long totalBytes, T context)
        {
            StatusCode = statusCode;
            BytesCompleted = bytesCompleted;
            TotalBytes = totalBytes;
            Context = context;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     The context for the request that initiated this upload or download.
        /// </summary>
        public T Context { get; private set; }

        /// <summary>
        ///     Response status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        ///     Total bytes that have completed so far.
        /// </summary>
        public long BytesCompleted { get; private set; }

        /// <summary>
        ///     Total bytes for to be uploaded or downloaded.
        /// </summary>
        public long TotalBytes { get; private set; }

        /// <summary>
        ///     An exception, if any, when invoking the request associated with an
        ///     upload or download request.  If this property is not null,
        ///     <seealso cref="BytesCompleted" /> and <seealso cref="TotalBytes" />
        ///     values are just garbage values and should be ignored.
        /// </summary>
        public Exception Error { get; set; }

        #endregion
    }
}