#region usings

using System;
using System.Net;

#endregion

namespace AsyncWebClient
{
    public class ResponseInfo<T>
    {
        internal ResponseInfo(HttpStatusCode statusCode, long bytesCompleted, long totalBytes, T context)
        {
            StatusCode = statusCode;
            BytesCompleted = bytesCompleted;
            TotalBytes = totalBytes;
            Context = context;
        }

        public T Context { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public long BytesCompleted { get; private set; }
        public long TotalBytes { get; private set; }
        public Exception Error { get; set; }
    }
}