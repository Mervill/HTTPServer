using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace HTTPServer
{
    /// <summary>
    /// Single-function wrapper for a generic WebRequest
    /// </summary>
    public static class SimpleGet
    {
        public static string Get(string remoteAddr, bool supressErrors = false)
        {
            string remoteResponse = string.Empty;
            try
            {
                //DateTime began = DateTime.UtcNow;

                // Request
                WebRequest request = WebRequest.Create(remoteAddr);
                request.Method = "GET";

                // Response
                WebResponse response = request.GetResponse();
                Stream respStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(respStream);
                remoteResponse = reader.ReadToEnd();
                reader.Close();
                respStream.Close();
                response.Close();

                //var timeDiff = DateTime.Now - began;
            }
            catch (WebException ex)
            {
                if (!supressErrors)
                    throw ex;
                    //throw new System.Exception("A required external service is not available!", ex);
            }
            return remoteResponse;
        }

        public static async Task<string> GetAsync(string remoteAddr, bool supressErrors = false)
        {
            string remoteResponse = string.Empty;
            try
            {
                DateTime began = DateTime.UtcNow;

                // Request
                WebRequest request = WebRequest.Create(remoteAddr);
                request.Method = "GET";

                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream streamResponse = response.GetResponseStream())
                using (StreamReader streamReader = new StreamReader(streamResponse))
                    remoteResponse = await streamReader.ReadToEndAsync();

                var timeDiff = DateTime.Now - began;
            }
            catch (WebException ex)
            {
                if (!supressErrors)
                {
                    if (!supressErrors)
                        throw ex;
                    //throw new System.Exception("A required external service is not available!", ex);
                }
            }
            return remoteResponse;
        }

        static string Truncate(string source, int length)
        {
            if (source.Length > length)
                source = source.Substring(0, length) + "[...]";
            return source;
        }
    }
}
