#region usings

using System;
using System.Net;

#endregion

namespace AsyncWebClient
{
    public class RequestInfo<T>
    {
        public RequestInfo(string remoteResourceAddress, Action<ResponseInfo<T>> progressChangedAction = null)
        {
            ServerAddress = new Uri(remoteResourceAddress);
            if (progressChangedAction == null)
            {
                progressChangedAction = res => { };
            }
            ProgressChangedAction = progressChangedAction;
            HeaderCollection = new WebHeaderCollection();
        }

        public Uri ServerAddress { get; set; }
        public WebHeaderCollection HeaderCollection { get; set; }
        public Action<ResponseInfo<T>> ProgressChangedAction { get; set; }

        public void AddHeader(string key, string value)
        {
            HeaderCollection.Add(key, value);
        }

        public void AddHeader(HttpRequestHeader requestHeader, string value)
        {
            HeaderCollection.Add(requestHeader, value);
        }
    }
}