using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace AnnoDesigner.Core.Helper
{
    public static class ConnectivityHelper
    {
        private const string URL = @"https://www.github.com";
        private const string SECOND_URL = @"https://www.google.com";
        private const string REQUEST_METHOD_HEAD = "HEAD";

        public static async Task<bool> IsConnected()
        {
            var result = false;

            var isInternetAvailable = false;

            var request = WebRequest.CreateHttp(URL);
            request.Timeout = TimeSpan.FromSeconds(5).Milliseconds;
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = REQUEST_METHOD_HEAD;

            try
            {
                using (var response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
                {
                    isInternetAvailable = response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (WebException)
            {
                isInternetAvailable = false;
            }

            //service outage? try second url
            if (!isInternetAvailable)
            {
                request = WebRequest.CreateHttp(SECOND_URL);
                request.Timeout = TimeSpan.FromSeconds(5).Milliseconds;
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Method = REQUEST_METHOD_HEAD;

                try
                {
                    using (var response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
                    {
                        isInternetAvailable = response.StatusCode == HttpStatusCode.OK;
                    }
                }
                catch (WebException)
                {
                    isInternetAvailable = false;
                }
            }

            if (isInternetAvailable)
            {
                result = IsNetworkAvailable;
            }

            return result;
        }

        public static bool IsNetworkAvailable
        {
            get { return NetworkInterface.GetIsNetworkAvailable(); }
        }
    }
}
