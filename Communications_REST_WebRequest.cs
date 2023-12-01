using QuarterMaster.Debugging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

namespace QuarterMaster.Communications.Rest
{
    public static class SimpleREST
    {
        /// http://rest.elkstein.org/2008/02/using-rest-in-c-sharp.html

        public static Dictionary<string, string> DictCreate()
        {
            return new Dictionary<string, string>();
        }

        public static Dictionary<string, string> DictAdd(Dictionary<string, string> dict, string key, string value)
        {
            dict[key] = value;
            return dict;
        }


        public static Tuple<bool, string, string> Http(string url,
            string verb,
            Dictionary<string, string> head,
            Dictionary<string, string> body)
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }

            HttpWebRequest req = WebRequest.Create(url)
                                 as HttpWebRequest;
            req.Method = verb.ToUpper();

            foreach (KeyValuePair<string, string> headPart in head)
            {
                req.Headers.Add(headPart.Key, headPart.Value);
            }

            if (string.Equals(verb, "POST", StringComparison.OrdinalIgnoreCase) || string.Equals(verb, "PUT", StringComparison.OrdinalIgnoreCase))
            {
                List<string> bodyList = new List<string>();
                foreach (KeyValuePair<string, string> bodyPart in body)
                {
                    bodyList.Add(bodyPart.Key + "=" + HttpUtility.UrlEncode(bodyPart.Value.ToString()));
                }
                string bodyString = string.Join("&", bodyList.ToArray<string>());

                // Encode the parameters as form data:
                byte[] formData = Encoding.UTF8.GetBytes(bodyString);
                req.ContentLength = formData.Length;

                // Send the request:
                using (Stream post = req.GetRequestStream())
                {
                    post.Write(formData, 0, formData.Length);
                }
            }

            // Pick up the response:
            string result = null;
            var response = new List<string>();
            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(resp.GetResponseStream());
                result = reader.ReadToEnd();
                var headers = resp.Headers;

                foreach (var hd in headers)
                {
                    response.Add("\"" + hd.ToString() + "\":\"" + headers.Get(hd.ToString()) + "\"");
                }
            }

            return new Tuple<bool, string, string>(true, result, "{" + string.Join(",", response.ToArray()) + "}");
        }

        public static string HttpGet(string url)
        {
            HttpWebRequest req = WebRequest.Create(url)
                                 as HttpWebRequest;
            string result = null;
            try
            {
                using (HttpWebResponse resp = req.GetResponse()
                                              as HttpWebResponse)
                {
                    StreamReader reader =
                        new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                }
            }
            catch (WebException hex)
            {
                result = hex.Message;
            }

            return result;
        }

        public static string HttpPost(string url, Dictionary<string, object> paramDict)
        {
            HttpWebRequest req = WebRequest.Create(new Uri(url))
                                 as HttpWebRequest;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            // Build a string with all the params, properly encoded.
            // We assume that the arrays paramName and paramVal are
            // of equal length:
            List<string> paramList = new List<string>();
            foreach (KeyValuePair<string, object> entry in paramDict)
            {
                paramList.Add(entry.Key + "=" + HttpUtility.UrlEncode(entry.Value.ToString()));
            }

            string paramString = string.Join("&", paramList.ToArray<string>());

            // Encode the parameters as form data:
            byte[] formData =
                Encoding.UTF8.GetBytes(paramString);
            req.ContentLength = formData.Length;

            // Send the request:
            using (Stream post = req.GetRequestStream())
            {
                post.Write(formData, 0, formData.Length);
            }

            // Pick up the response:
            string result = null;
            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            {
                StreamReader reader =
                    new StreamReader(resp.GetResponseStream());
                result = reader.ReadToEnd();
            }

            return result;
        }
    }
}
