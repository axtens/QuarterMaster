using System;
using System.Collections.Generic;

namespace QuarterMaster.Communications.Rest.COM
{
    public class RESTful
    {
        private const int HTTPREQUEST_PROXYSETTING_DEFAULT = 0;
        private const int HTTPREQUEST_PROXYSETTING_PRECONFIG = 0;
        private const int HTTPREQUEST_PROXYSETTING_DIRECT = 1;
        private const int HTTPREQUEST_PROXYSETTING_PROXY = 2;
        private string _url = "";
        private string _verb = "";
        private string _tail = "";
        private string _body = "";
        private string _contentType = "";
        private string _result = "";
        private string _path = "";
        public readonly int[] _timeouts = { 0, 60000, 30000, 30000 };
        private string _proxy = "";
        private bool _async;
        private readonly Dictionary<string, string> _reqheads = new Dictionary<string, string>();
        private readonly Dictionary<string, object> _status = new Dictionary<string, object>();
        private readonly WinHttp.WinHttpRequest _req = new WinHttp.WinHttpRequest();

        public string URL = string.Empty;
        public string BODY = string.Empty;

        //private HttpWebRequest _req = null;
        private readonly Dictionary<string, object> _params = new Dictionary<string, object>();

        public RESTful(string url="")
        {
            this._url = url;
        }

        public RESTful Url(string url = "")
        {
            this._url = url;
            return this;
        }

        public RESTful Verb(string verb = "GET")
        {
            this._verb = verb;
            return this;
        }

        public RESTful Accept(string accept = "*/*")
        {
            this._reqheads["Accept"] = accept;
            //this._accept = accept;
            return this;
        }

        public string ResponseHeaders()
        {
            return this._req.GetAllResponseHeaders();
        }

        public RESTful RequestHeader(string key, string value)
        {
            this._reqheads[key] = value;
            return this;
        }

        public RESTful Path(string path = "")
        {
            if (!string.IsNullOrEmpty(this._path))
            {
                this._path += (path.StartsWith("/", StringComparison.CurrentCulture) ? path : "/" + path);
            }
            else
            {
                if (!string.IsNullOrEmpty(path))
                {
                    this._path = (path.StartsWith("/", StringComparison.CurrentCulture) ? path : "/" + path);
                }
            }
            return this;
        }

        public RESTful Tail(string tail = "")
        {
            if (!string.IsNullOrEmpty(this._tail))
            {
                this._tail = this._tail + "&" + tail;
            }
            else
            {
                if (!string.IsNullOrEmpty(tail))
                {
                    this._tail = tail;
                }
            }
            return this;
        }

        public RESTful Body(string body = "")
        {
            if (!string.IsNullOrEmpty(this._body))
            {
                this._body = this._body + "&" + body;
            }
            else
            {
                if (!string.IsNullOrEmpty(body))
                {
                    this._body = body;
                }
            }
            return this;
        }

        public RESTful ContentType(string contentType = "")
        {
            this._contentType = contentType;
            return this;
        }

        public RESTful Timeouts(int a = 0, int b = 60000, int c = 30000, int d = 30000)
        {
            this._timeouts[0] = a;
            this._timeouts[1] = b;
            this._timeouts[2] = c;
            this._timeouts[3] = d;
            return this;
        }

        public RESTful Proxy(string proxy)
        {
            this._proxy = proxy;
            return this;
        }

        public RESTful UserAgent(string useragent)
        {
            this._reqheads["User-Agent"] = useragent;
            //this._useragent = useragent;
            return this;
        }

        public RESTful Asynchronous(bool flag = false)
        {
            this._async = flag;
            return this;
        }

        public RESTful Send()
        {
            string url = this._url + (!string.IsNullOrEmpty(this._path) ? this._path : string.Empty) + (!string.IsNullOrEmpty(this._tail) ? "?" + this._tail : string.Empty);
            this._url = string.Empty;
            this._path = string.Empty;
            this._tail = string.Empty;
            this.URL = url;

            this._req.Open(this._verb, url, this._async);

            this._async = false;

            this._req.SetTimeouts(this._timeouts[0], this._timeouts[1], this._timeouts[2], this._timeouts[3]);

            //this._req.SetRequestHeader("User-Agent", this._useragent);

            this.Accept();
            if (!this._reqheads.ContainsKey("User-Agent"))
                this.UserAgent("Mozilla/4.0 (compatible; Mozilla/5.0 (compatible; MSIE 8.0; Windows NT 6.0; Trident/4.0; Acoo Browser 1.98.744; .NET CLR 3.5.30729); Windows NT 5.1; Trident/4.0)");
            foreach (KeyValuePair<string, string> kvp in _reqheads)
            {
                this._req.SetRequestHeader(kvp.Key, kvp.Value);
            }

            if ("" != this._proxy)
            {
                this._req.SetProxy(HTTPREQUEST_PROXYSETTING_PROXY, this._proxy, string.Empty);
            }
            else
            {
                this._req.SetProxy(HTTPREQUEST_PROXYSETTING_DIRECT);
            }

            this._proxy = string.Empty;

            try
            {
                if (this._verb == "POST" || this._verb == "PUT")
                {
                    this._req.Send(this._body);
                }
                else
                {
                    this._req.Send();
                }

                this._status["OK"] = true;
                this._status["DATA"] = this._req.ResponseText;
                this._status["HRESULT"] = 0;
                this._status["STATUS"] = this._req.Status;
                this._status["STATUSTEXT"] = this._req.StatusText;
                this._result = this._req.ResponseText;
                this.BODY = this._body;
            }
            catch (Exception hexc)
            {
                this._status["OK"] = false;
                this._status["DATA"] = hexc.Message;
                this._status["HRESULT"] = hexc.HResult;
            }
            this._verb = string.Empty;
            this._body = string.Empty;
            return this;
        }

        public string Response()
        {
            return this._req.ResponseText;
        }

        public dynamic ResponseStream()
        {
            return this._req.ResponseStream;
        }

        public dynamic ResponseBody()
        {
            return this._req.ResponseBody;
        }

        public bool IsOK()
        {
            return (bool)this._status["OK"];
        }

        public string Data()
        {
            return this._status["DATA"].ToString();
        }

        public int HResult()
        {
            return (int)this._status["HRESULT"];
        }

        public int Status()
        {
            this._status["STATUS"] = this._req.Status;
            return (int)this._status["STATUS"];
        }

        public string StatusText()
        {
            this._status["STATUSTEXT"] = this._req.StatusText;
            return this._status["STATUSTEXT"].ToString();
        }

        override public string ToString()
        {
            return this._result;
        }
    }
}
