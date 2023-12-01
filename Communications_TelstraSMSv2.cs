using QuarterMaster.Communications.Rest.COM;
using QuarterMaster.Configuration;
using QuarterMaster.Debugging;
using QuarterMaster.Serialization;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace QuarterMaster.Communications
{
    public class TelstraSMSv2
    {
        internal TextInfo textInfo = new CultureInfo("en-AU", false).TextInfo;

        private readonly string clientKey;
        private readonly string clientSecret;
        private readonly string tokenUrl;
        private string accessToken;
        private string expiresIn;
        private readonly string subscriptionsUrl;
        private readonly string smsUrl;

        private readonly Config config;

        public bool Authenticated { get; set; }
        public bool Provisioned { get; set; }
        public bool Sent { get; set; }
        public string Status { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public string Error_Description { get; set; }
        public string To { get; set; }
        public string DeliveryStatus { get; set; }
        public string MessageId { get; set; }
        public string MessageStatusURL { get; set; }
        public string DestinationAddress { get; set; }

        private readonly Dictionary<string, string> Numbers = new Dictionary<string, string>();

        public TelstraSMSv2(string configFile)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            config = new Config(configFile);
            clientKey = config.Retrieve("clientKey");
            clientSecret = config.Retrieve("clientSecret");
            tokenUrl = config.Retrieve("tokenUrl");
            subscriptionsUrl = config.Retrieve("subscriptionsUrl");
            smsUrl = config.Retrieve("smsUrl");
            LoadNumbers();
        }

        public TelstraSMSv2 Authenticate()
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            Authenticated = true; // this is a lie
            return this; // authenticated && provisioned;
        }

        private TelstraSMSv2 AuthenticatedJustBeforeSend()
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            // # Obtain these keys from the Telstra Developer Portal
            // CONSUMER_KEY = "your consumer key"
            // CONSUMER_SECRET = "your consumer secret"
            // curl - X POST - H 'Content-Type: application/x-www-form-urlencoded' \
            // -d "grant_type=client_credentials&client_id=$CONSUMER_KEY&client_secret=$CONSUMER_SECRET&scope=NSMS" \
            // 'https://tapi.telstra.com/v2/oauth/token'

            var postbody = /*HttpUtility.UrlEncode*/("grant_type=client_credentials&client_id=$CLIENT_KEY&client_secret=$CLIENT_SECRET&scope=NSMS"
                    .Replace("$CLIENT_KEY", clientKey).Replace("$CLIENT_SECRET", clientSecret));

            var restful = new RESTful()
                .Url(tokenUrl)
                .Verb("POST")
                .RequestHeader("Content-Type", "application/x-www-form-urlencoded")
                .Body(postbody)
                .Send();

            if (restful.IsOK())
            {
                object response = (new JSON().Load(restful.Response()).Deserialize()).ToObject();
                var d = (Dictionary<string, object>)response;
                if (d.ContainsKey("error"))
                {
                    Error = d["error"].ToString();
                    if (d.ContainsKey("error_description"))
                    {
                        Error_Description = d["error_description"].ToString();
                    }
                    Authenticated = false;
                    return this;
                }
                else
                {
                    accessToken = ((Dictionary<string, object>)response)["access_token"].ToString();
                    expiresIn = ((Dictionary<string, object>)response)["expires_in"].ToString();
                    Authenticated = true;
                }
            }

            const string body = "{\"activeDays\":30}";

            restful = new RESTful()
                .Url(subscriptionsUrl)
                .Verb("POST")
                .RequestHeader("authorization", "Bearer " + accessToken)
                .RequestHeader("cache-control", "no-cache")
                .RequestHeader("content-type", "application/json")
                .Body(body)
                .Send();

            if (restful.IsOK())
            {
                object response = (new JSON().Load(restful.Response()).Deserialize()).ToObject();
                var d = (Dictionary<string, object>)response;
                if (d.ContainsKey("status"))
                {
                    Status = d["status"].ToString();
                    Code = d["code"].ToString();
                    Message = d["message"].ToString();
                    Provisioned = false;
                    return this;
                }
                else
                {
                    DestinationAddress = d["destinationAddress"].ToString();
                    Provisioned = true;
                }
            }

            return this;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public TelstraSMSv2 SendSMS(string type, string destinationNumber, string messageBody)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
                Debugger.Launch();

            AuthenticatedJustBeforeSend();

            if (messageBody?.Length == 0)
            {
                messageBody = new StackFrame(1, true).GetMethod().Name;
            }

            if (0 == type?.Length)
            {
                type = "INFO";
            }

            string number = string.Empty;
            if (Numbers.ContainsKey(destinationNumber))
            {
                number = Numbers[destinationNumber];
            }
            else
            {
                number = destinationNumber;
            }

            var body = $"{{\"to\":\"{number}\",\"body\":\"[{type}] {messageBody}\",\"from\":\"{DestinationAddress}\", \"validity\": 60, \"notifyURL\": \"\", \"replyRequest\": false}}";
            var restful = new RESTful()
                .Url(smsUrl)
                .Verb("POST")
                .Body(body)
                .RequestHeader("Authorization", "Bearer " + accessToken)
                .RequestHeader("Content-Type", "application/json")
                .Send();

            if (restful.IsOK())
            {
                object response = (new JSON().Load(restful.Response()).Deserialize()).ToObject();
                var d = (Dictionary<string, object>)response;
                if (d.ContainsKey("status"))
                {
                    Status = d["status"].ToString();
                    Code = d["code"].ToString();
                    Message = d["message"].ToString();
                    Sent = false;
                }
                else
                {
                    string resp = restful.Response();
                    var stringArray = resp.Split(new char[] { '[', ']' }).Where((_, index) => index % 2 != 0);
                    var messages = (new JSON().Load(stringArray.First()).Deserialize()).ToObject();
                    To = ((Dictionary<string,object>)messages)["to"].ToString();
                    DeliveryStatus = ((Dictionary<string, object>)messages)["deliveryStatus"].ToString();
                    MessageId = ((Dictionary<string, object>)messages)["messageId"].ToString();
                    MessageStatusURL = ((Dictionary<string, object>)messages)["messageStatusURL"].ToString();
                    Sent = true;
                }
            }
            return this; //  sent;
        }

        private TelstraSMSv2 SendTargeted(string _type, string _message)
        {
            TelstraSMSv2 ret = null;
            var recipients = config.Retrieve(textInfo.ToTitleCase(_type) + "Recipients", "").Split(' ');
            foreach (string recipient in recipients)
            {
                ret = SendSMSTimeProcess(_type.ToUpper(System.Globalization.CultureInfo.CurrentCulture), recipient, _message);
            }
            return ret;
        }

        private TelstraSMSv2 SendSMSTimeProcess(string type, string recipient, string message) =>
            SendSMS(type, recipient, $"{DateTime.Now.ToString("HH:mm:ss.ffffzzz")} {Environment.MachineName} {System.Diagnostics.Process.GetCurrentProcess().ProcessName} {message}");

        private TelstraSMSv2 SendSMSTimeProcess(string type, string[] recipients, string message)
        {
            foreach (string recipient in recipients)
            {
                TelstraSMSv2 ret = SendSMS(type, recipient, $"{DateTime.Now.ToString("HH:mm:ss.ffffzzz")} {Environment.MachineName} {System.Diagnostics.Process.GetCurrentProcess().ProcessName} {message}");
            }
            return this;
        }

        public TelstraSMSv2 SendInformation(string message) => SendTargeted("Info", message);
        public TelstraSMSv2 SendWarning(string message) => SendTargeted("Warning", message);
        public TelstraSMSv2 SendError(string message) => SendTargeted("Error", message);
        public TelstraSMSv2 SendDebug(string message) => SendTargeted("Debug", message);

        public TelstraSMSv2 SendInformation(string recipient, string message) => SendSMSTimeProcess("INFO", recipient, message);
        public TelstraSMSv2 SendWarning(string recipient, string message) => SendSMSTimeProcess("WARNING", recipient, message);
        public TelstraSMSv2 SendError(string recipient, string message) => SendSMSTimeProcess("ERROR", recipient, message);
        public TelstraSMSv2 SendDebug(string recipient, string message) => SendSMSTimeProcess("DEBUG", recipient, message);

        public TelstraSMSv2 SendInformation(string[] recipients, string message) => SendSMSTimeProcess("INFO", recipients, message);
        public TelstraSMSv2 SendWarning(string[] recipients, string message) => SendSMSTimeProcess("WARNING", recipients, message);
        public TelstraSMSv2 SendError(string[] recipients, string message) => SendSMSTimeProcess("ERROR", recipients, message);
        public TelstraSMSv2 SendDebug(string[] recipients, string message) => SendSMSTimeProcess("DEBUG", recipients, message);

        private void LoadNumbers()
        {
            string numberNames = config.Retrieve("Numbers", string.Empty);
            if (numberNames != string.Empty)
            {
                string[] names = numberNames.Split(new char[] { ' ' });
                foreach (string name in names)
                {
                    string number = config.Retrieve(name, string.Empty);

                    if (number != string.Empty)
                    {
                        Numbers[name] = number;
                    }
                }
            }
        }
    }
}
