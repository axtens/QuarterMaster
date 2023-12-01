using QuarterMaster.Debugging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;


namespace QuarterMaster.Communications.Rest
{
    public class Fluent
    {
        private string _url { get; set; }
        private string _verb { get; set; }
        private readonly List<string> _path;
        private readonly List<Tuple<string, string>> _query;
        private readonly List<string> _body;
        private readonly List<Tuple<string, string, object>> _formparts;
        private readonly List<Tuple<string, string>> _head;
        private HttpWebRequest _req;
        private bool _ok { get; set; }
        private string _result { get; set; }
        private string _headers { get; set; }
        //private IWebProxy _proxy { get; set; }
        private int _statuscode { get; set; }
        private string _statusdescription { get; set; }
        public string URL { get; set; }
        public string BODY { get; set; }
        public string REQHEAD { get; set; }
        private bool _formbody { get; set; }

        public Fluent()
        {
            _path = new List<string>();
            _body = new List<string>();
            _head = new List<Tuple<string, string>>();
            _query = new List<Tuple<string, string>>();
            _formparts = new List<Tuple<string, string, object>>();
            _verb = "GET";
        }

        public Fluent Reset()
        {
            _path.Clear();
            _body.Clear();
            _head.Clear();
            _query.Clear();
            _formparts.Clear();
            _formbody = false;
            _verb = "GET";
            _statuscode = int.MinValue;
            _statusdescription = string.Empty;
            _ok = false;
            _result = string.Empty;
            _headers = string.Empty;
            URL = string.Empty;
            BODY = string.Empty;
            REQHEAD = string.Empty;
            try { _req.Abort(); } catch (Exception) { }
            return this;
        }

        public Fluent Url(string arg)
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }
            _url = arg.EndsWith("/", StringComparison.InvariantCulture) ? arg.Substring(0, arg.Length - 1) : arg;
            return this;
        }

        public Fluent Verb(string arg)
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }
            _verb = arg;
            return this;
        }

        public Fluent Path(object arg)
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }
            var parts = arg.ToString().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                _path.Add(part);
            }
            return this;
        }

        public Fluent Query(string key, string value, bool escaped = false)
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }
            _query.Add(new Tuple<string, string>(key, escaped ? HttpUtility.UrlEncode(value) : value));
            return this;
        }

        public Fluent Body(string key, string value, bool escaped = false)
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }
            _body.Add(key + "=" + (escaped ? HttpUtility.UrlEncode(value) : value));
            return this;
        }

        public Fluent Body(string value, bool escaped = false)
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }
            _body.Add(escaped ? HttpUtility.UrlEncode(value) : value);
            return this;
        }

        public Fluent FormBody(string boundary, string key, string value, bool escaped = false)
        {
            _formbody = true;
            var moduleName = MethodBase.GetCurrentMethod().Name;
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }
            _formparts.Add(new Tuple<string, string, object>(boundary, key, (escaped ? HttpUtility.UrlEncode(value) : value)));
            return this;
        }

        public Fluent Head(string key, string value)
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }
            _head.Add(new Tuple<string, string>(key, value));
            return this;
        }

        private void ClearSessionVariables()
        {
            _ok = false;
            _result = string.Empty;
            _headers = string.Empty;
            _statuscode = 0;
            _statusdescription = string.Empty;
        }

        public Fluent Send()
        {
            ClearSessionVariables();
            var moduleName = MethodBase.GetCurrentMethod().Name;
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }

            var fullUrl = new StringBuilder();
            fullUrl.Append(_url);
            fullUrl.Append("/");
            fullUrl.Append(string.Join("/", _path.ToArray()));
            if (_query.Count > 0)
            {
                fullUrl.Append("?");
                var queries = new List<string>();
                foreach (var kvp in _query)
                {
                    queries.Add(string.Format("{0}={1}", kvp.Item1, kvp.Item2));
                }
                fullUrl.Append(string.Join("&", queries.ToArray<string>()));
            }

            URL = fullUrl.ToString();

            _req = WebRequest.Create(fullUrl.ToString()) as HttpWebRequest;
            _req.Method = _verb.ToUpper();

            var reqheaders = new StringBuilder();

            if (_head.Count > 0)
            {
                foreach (var kvp in _head)
                {
                    switch (kvp.Item1)
                    {
                        case "Content-Type":
                            _req.ContentType = kvp.Item2;
                            break;
                        case "Accept":
                            _req.Accept = kvp.Item2;
                            break;
                        default:
                            _req.Headers.Set(kvp.Item1, kvp.Item2);
                            break;
                    }
                    reqheaders.Append(kvp.Item1).Append(": ").AppendLine(kvp.Item2);
                }
            }

            REQHEAD = reqheaders.ToString();

            string fullBodyText = string.Empty;

            if (string.Equals(_verb, "POST", StringComparison.OrdinalIgnoreCase) || string.Equals(_verb, "PUT", StringComparison.OrdinalIgnoreCase))
            {
                var fullBody = new StringBuilder();
                if (_formbody)
                {
                    foreach (var formpart in _formparts)
                    {
                        fullBody.Append("--").AppendLine(formpart.Item1)
                            .Append("Content-Disposition: form-data; name=\"").Append(formpart.Item2).AppendLine("\"")
                            .AppendLine()
                            .AppendLine(formpart.Item3.ToString());
                    }
                    fullBodyText = fullBody.ToString();
                }
                else
                {
                    if (_body.Count > 0)
                    {
                        foreach (var bodyPart in _body)
                        {
                            fullBody.Append(bodyPart);
                        }
                        fullBodyText = fullBody.ToString();
                    }
                }

                byte[] formData = Encoding.UTF8.GetBytes(fullBodyText);
                _req.ContentLength = formData.Length;
                using (Stream post = _req.GetRequestStream())
                {
                    post.Write(formData, 0, formData.Length);
                }

                BODY = fullBody.ToString();
            }

            var response = new List<string>();
            HttpWebResponse resp = null;
            try
            {
                resp = _req.GetResponse() as HttpWebResponse;
                _statuscode = (int)resp.StatusCode;
                _statusdescription = resp.StatusDescription;
                _ok = true;
            }
            catch (WebException we)
            {
                _statusdescription = we.Message;
                const string pattern = @"\((\d+?)\)";
                Match m = Regex.Match(we.Message, pattern, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    if (m.Groups.Count > 1)
                    {
                        _statuscode = int.Parse(m.Groups[1].Value);
                    }
                    else
                    {
                        _statuscode = we.HResult;
                    }
                }
                else
                {
                    _statuscode = we.HResult;
                }
                _ok = false;
            }

            if (_ok)
            {
                StreamReader reader = new StreamReader(resp.GetResponseStream());
                _result = reader.ReadToEnd();

                var headers = resp.Headers;

                //_headers = SimpleJson.SerializeObject(resp.Headers);

                foreach (var hd in headers)
                {
                    response.Add("{\"name\":\"" + hd.ToString() + "\",\"value\":" + SimpleJson.SerializeObject(headers.Get(hd.ToString())) + "}");
                }
                _headers = "[" + string.Join(",", response.ToArray()) + "]";
            }

            return this;
        }

        public bool IsOK()
        {
            return _ok;
        }

        public string Response()
        {
            return _result;
        }

        public string ResponseHeaders()
        {
            return _headers;
        }

        public string StatusMessage()
        {
            return _statusdescription;
        }

        public int Status()
        {
            return _statuscode;
        }
    }
}
