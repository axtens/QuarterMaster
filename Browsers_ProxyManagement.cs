using Newtonsoft.Json;

using QuarterMaster.Communications.Rest;
using QuarterMaster.Communications.Rest.COM;
using QuarterMaster.Debugging;
using QuarterMaster.Infrastructure;
using QuarterMaster.Logging;
using QuarterMaster.Serialization;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace QuarterMaster.Browsers
{
    public static class ProxyManagement
    {
        internal static ApplicationLogging AL;

        public static bool PortAvailable(ref int port, ref string reason)
        {
            AL?.Module("portAvailable");
            RESTful restful = new RESTful()
                .Url("http://localhost:8080/proxy")
                .Verb("POST")
                .Tail("trustAllServers=true")
                .Body("trustAllServers=true");
            var result = restful.Send();
            bool answer;
            if (result.IsOK())
            {
                try
                {
                    JSON js = new JSON(restful.ToString());
                    js.Deserialize();
                    Dictionary<string, object> dso = (Dictionary<string, object>)js.ToObject();
                    port = (int)(dso["port"]);
                    //port = (int)JSengine.Evaluate("var portObj  = (" + fr.ToString() + "); portObj.port;");
                    answer = true;
                    AL?.Informational().Send($"POST {restful.URL} {restful}");
                }
                catch (Exception exc)
                {
                    answer = false;
                    reason = exc.Message;
                    AL?.Warning().Send($"POST {restful.URL} {reason}");
                }
            }
            else
            {
                answer = false;
                reason = result.Data();
                AL?.Warning().Send($"POST {restful.URL} {reason}");
            }
            AL?.Module();
            return answer;
        }

        public static bool ConfigureProxy(ref int port, ref string reason)
        {
            AL?.Module("configureProxy");
            RESTful restful = new RESTful()
            .Url("http://localhost:8080/proxy/" + port + "/har")
            .Verb("PUT")
            .Tail("captureHeaders=true&captureCookies=true&captureContent=true&captureBinaryContent=true&initialPageRef=Scraper")
            .Body("captureHeaders=true&captureCookies=true&captureContent=true&captureBinaryContent=true&initialPageRef=Scraper");

            var result = restful.Send();
            bool answer;
            if (result.IsOK())
            {
                answer = true;
                AL?.Informational().Send($"PUT {result.URL}");
            }
            else
            {
                answer = false;
                AL?.Informational().Send($"PUT {result.URL} {result.Data()}");
            }

            AL?.Module();
            return answer;
        }

        public static string ConnectToProxy()
        {
            RESTful restful = new RESTful()
                .Url("http://localhost:8080")
                .Path("/proxy")
                .Verb("POST");

            var result = restful.Send();
            string proxy = result.ToString();

            JSON fj = new JSON(proxy);
            //var sod = new Dictionary<string, object>();
            var sod = (Dictionary<string, object>)fj.Deserialize().ToObject();

            restful
                .Verb("PUT")
                .Path("/proxy/" + sod["port"] + "/har")
                .Tail("captureHeaders=true&captureCookies=true&captureContent=true&captureBinaryContent=true")
                .Body("captureHeaders=true&captureCookies=true&captureContent=true&captureBinaryContent=true")
                .Send();

            return sod["port"].ToString();
        }

        public static bool KillAndRestartProxy()
        {
            AL?.Module("killAndRestartProxy");
            bool done;// = false;
            int retryCount = 0;
            do
            {
                done = Processes.RestartService("BrowserMobProxy", 10000);
                retryCount++;
            } while (!done && retryCount < 3);

            if (!done)
            {
                AL?.Error().Send("Could not restart BrowserMobProxy");
                AL?.Module();
                return false;// Environment.Exit(1);
            }
            else
            {
                AL?.Module();
                return true;
            }
        }

        public static Tuple<bool, string> GetPort()
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            AL?.Module(moduleName);
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }
            //string port;//= string.Empty;
            dynamic obj;
            bool _err;
            string cargo;// = string.Empty;

            var getter = GetProxyList();
            if (getter.Item1)
            {
                var json = getter.Item2;
                try
                {
                    obj = JsonConvert.DeserializeObject(json);
                }
                catch (Exception)
                {
                    _err = true;
                    cargo = json;
                    goto exitpoint;
                }

                if (obj.proxyList.Count == 0)
                {
                    var poster = CreateProxy();
                    if (poster.Item1)
                    {
                        json = poster.Item2;
                        try
                        {
                            obj = JsonConvert.DeserializeObject(json);
                            cargo = obj.port;
                            _err = false;
                        }
                        catch (Exception)
                        {
                            _err = true;
                            cargo = json;
                        }
                    }
                    else
                    {
                        _err = true;
                        cargo = poster.Item2;
                    }
                }
                else
                {
                    cargo = obj.proxyList[0].port;
                    _err = false;
                }
            }
            else
            {
                _err = false;
                cargo = getter.Item2;
            }
            exitpoint:
            AL?.Module();
            return new Tuple<bool, string>(_err, cargo);
        }

        private static Tuple<bool, string, string> GetProxyList()
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            AL?.Module(moduleName);
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }

            Tuple<bool, string, string> tupl; // = new Tuple<bool, string, string>(false, string.Empty, string.Empty);

            var F = new Fluent()
                .Verb("GET")
                .Url("http://localhost:8080")
                .Path("proxy")
                .Send();
            if (F.IsOK())
            {
                tupl = new Tuple<bool, string, string>(true, F.Response(), F.ResponseHeaders());
            }
            else
            {
                tupl = new Tuple<bool, string, string>(true, F.StatusMessage(), F.ResponseHeaders());
            }
            AL?.Module();
            return tupl;
        }

        //var head = SimpleREST.DictCreate();
        //var body = SimpleREST.DictCreate();
        //var tupl = SimpleREST.Http("http://localhost:8080/proxy", "GET", head, body);

        //if (tupl.Item1) {// true means it worked
        //    return tupl;
        //}
        //else
        //{
        //    return new Tuple<bool, string, string>(false, tupl.Item2, tupl.Item3);
        //}

        private static Tuple<bool, string, string> CreateProxy()
        {
            var moduleName = MethodBase.GetCurrentMethod().Name;
            AL?.Module(moduleName);
            if (DebugPoints.DebugPointRequested(moduleName.ToUpper()))
            {
                Debugger.Launch();
            }
            //var head = SimpleREST.DictCreate();
            //var body = SimpleREST.DictCreate();
            //body = SimpleREST.DictAdd(body, "trustAllServers", "true");
            //var tupl = SimpleREST.Http("http://localhost:8080/proxy", "POST", head, body);
            //
            //AL?.Module();
            //
            //if (tupl.Item1)
            //{// true means it worked
            //    return tupl;
            //}
            //else
            //{
            //    return new Tuple<bool, string, string>(false, tupl.Item2, tupl.Item3);
            //}
            Tuple<bool, string, string> tupl;//= new Tuple<bool, string, string>(false, string.Empty, string.Empty);

            var F = new Fluent()
                .Verb("POST")
                .Url("http://localhost:8080")
                .Path("proxy")
                .Body("trustAllServers", "true")
                .Send();
            if (F.IsOK())
            {
                tupl = new Tuple<bool, string, string>(true, F.Response(), F.ResponseHeaders());
            }
            else
            {
                tupl = new Tuple<bool, string, string>(true, F.StatusMessage(), F.ResponseHeaders());
            }
            AL?.Module();
            return tupl;
        }

        public static void RegisterApplicationLogging(ref ApplicationLogging ptr)
        {
            AL = ptr;
        }
    }
}
