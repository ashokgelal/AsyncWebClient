#region usings

using System;
using System.Net;

#endregion

namespace AsyncWebClient
{
    /// <summary>
    ///     Class that wraps information about a request for either downloading or
    ///     uploading a file.
    /// </summary>
    public class RequestInfo<T>
    {
        #region Properties

        /// <summary>
        ///     Remote resource address.
        /// </summary>
        public Uri ResourceAddress { get; set; }

        /// <summary>
        ///     Header information associated with this request.
        /// </summary>
        public WebHeaderCollection HeaderCollection { get; set; }

        /// <summary>
        ///     A callback for sending back progress as a <see cref="ResponseInfo{T}" /> object.
        /// </summary>
        public Action<ResponseInfo<T>> ProgressChangedAction { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <param name="remoteResourceAddress">
        ///     Remote rosource adddress from where to download or upload a file.
        /// </param>
        /// <param name="progressChangedAction">
        ///     Optional progress changed action that will be invoked asynchronously
        ///     when some progress have been made.
        /// </param>
        public RequestInfo(string remoteResourceAddress, Action<ResponseInfo<T>> progressChangedAction = null)
        {
            ResourceAddress = new Uri(remoteResourceAddress);
            if (progressChangedAction == null)
            {
                progressChangedAction = res => { };
            }
            ProgressChangedAction = progressChangedAction;
            HeaderCollection = new WebHeaderCollection();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Append the specified header for this request to the <seealso cref="HeaderCollection" />.
        /// </summary>
        /// <param name="key">The header to be added to the collection.</param>
        /// <param name="value">The content of the header.</param>
        public void AddHeader(string key, string value)
        {
            HeaderCollection.Add(key, value);
        }

        /// <summary>
        ///     Append the specified header for this request to the <seealso cref="HeaderCollection" />.
        /// </summary>
        /// <param name="requestHeader">The header to be added to the collection.</param>
        /// <param name="value">The content of the header.</param>
        public void AddHeader(HttpRequestHeader requestHeader, string value)
        {
            HeaderCollection.Add(requestHeader, value);
        }

        #endregion
    }
}