using QuarterMaster.Configuration;
using QuarterMaster.Logging;
using QuarterMaster.Serialization;

using RestSharp;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace QuarterMaster.Communications
{
    public class TelstraSMS
    {
        internal TextInfo textInfo = new CultureInfo("en-AU", false).TextInfo;

        internal static ApplicationLogging AL;

        private string consumerKey = string.Empty;
        private string consumerSecret = string.Empty;
        private string url = string.Empty;
        private readonly string authquery = string.Empty;
        private readonly string sendquery = string.Empty;

        private string access_token = string.Empty;
        private string expires_in = string.Empty;

        private readonly string cfgFile = string.Empty;
        private readonly Config config;

        private bool authenticated;

        private string messageId = string.Empty;

        private readonly Dictionary<string, string> Numbers = new Dictionary<string, string>();

        private bool blueLogging = true;

        public TelstraSMS()
        {
            config = new Config();
        }

        public TelstraSMS(string _cfgfile)
        {
            AL?.Module("TelstraSMS");
            cfgFile = _cfgfile;
            config = new Config(cfgFile);
            LoadNumbers();
            AL?.Module();
        }

        public TelstraSMS Authenticate()
        {
            AL?.Module("Authenticate");

            consumerKey = config.Retrieve("consumer.key");
            consumerSecret = config.Retrieve("consumer.secret");
            url = config.Retrieve("authurl", "https://api.telstra.com/v1/oauth/token");

            var client = new RestClient(url);
            var request = new RestRequest("", Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("client_id", consumerKey);
            request.AddParameter("client_secret", consumerSecret);
            request.AddParameter("scope", "SMS");
            request.AddParameter("grant_type", "client_credentials");

            var response = client.Execute(request);
            var content = response.Content;
            if (!string.IsNullOrEmpty(content))
            {
                JSON FJ = new JSON().Load(content).Deserialize();
                object fj = FJ.ToObject();
                try
                {
                    access_token = ((Dictionary<string, object>)fj)["access_token"].ToString();
                    expires_in = ((Dictionary<string, object>)fj)["expires_in"].ToString();
                    authenticated = true;
                }
                catch (Exception)
                {
                    AL?.Fail("access_token not present in authorization packet.");
                }
            } else
            {
                authenticated = false;
                AL?.Fail("No data returned from provider.");
            }
            AL?.Module();
            return this;
        }

        private TelstraSMS SendTargeted(string _type, string _message)
        {
            TelstraSMS ret = null;
            string[] recipients = config.Retrieve(textInfo.ToTitleCase(_type) + "Recipients", "").Split(' ');
            foreach (string recip in recipients)
            {
                ret = SendSMSTimeProcess(_type.ToUpper(System.Globalization.CultureInfo.CurrentCulture), recip, _message);
                if (blueLogging)
                {
                    BlueLog.Module("SendTargeted");
                    BlueLog.Information(DateTime.Now.ToString("HH:mm:ss.ffffzzz"),
                        Environment.MachineName,
                        Process.GetCurrentProcess().ProcessName,
                        _type,
                        recip,
                        "'" + _message + "'",
                        Environment.StackTrace);
                    BlueLog.Module();
                }
            }
            return ret;
        }

        public void BlueLogging(bool flag)
        {
            blueLogging = flag;
        }

        public TelstraSMS SendInformation(string _message) => SendTargeted("Info", _message);
        public TelstraSMS SendWarning(string _message) => SendTargeted("Warning", _message);
        public TelstraSMS SendError(string _message) => SendTargeted("Error", _message);
        public TelstraSMS SendDebug(string _message) => SendTargeted("Debug", _message);

        public TelstraSMS SendInformation(string _numberOrName, string _message)
            => SendSMSTimeProcess("INFO", _numberOrName, _message);

        public TelstraSMS SendWarning(string _numberOrName, string _message)
            => SendSMSTimeProcess("WARNING", _numberOrName, _message);

        public TelstraSMS SendError(string _numberOrName, string _message)
            => SendSMSTimeProcess("ERROR", _numberOrName, _message);

        public TelstraSMS SendInformation(string[] _numberOrName, string _message)
        {
            TelstraSMS ret = null;
            foreach (string _numName in _numberOrName)
            {
                ret = SendSMSTimeProcess("INFO", _numName, _message);
            }
            return ret;
        }

        public TelstraSMS SendWarning(string[] _numberOrName, string _message)
        {
            TelstraSMS ret = null;
            foreach (string _numName in _numberOrName)
            {
                ret = SendSMSTimeProcess("WARNING", _numName, _message);
            }
            return ret;
        }

        public TelstraSMS SendError(string[] _numberOrName, string _message)
        {
            TelstraSMS ret = null;
            foreach (string _numName in _numberOrName)
            {
                ret = SendSMSTimeProcess("ERROR", _numName, _message);
            }
            return ret;
        }

        public TelstraSMS SendSMSTimeProcess(string status, string _numberOrName, string _message)
            => SendSMS(status, _numberOrName,
                $"{DateTime.Now:HH:mm:ss.ffffzzz} {Environment.MachineName} {System.Diagnostics.Process.GetCurrentProcess().ProcessName} {_message}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        public TelstraSMS SendSMS(string status, string _numberOrName, string _message)
        {
            AL?.Module("SendSMS");

            if (_message?.Length == 0)
            {
                _message = new StackFrame(1, true).GetMethod().Name;
            }
            if (0 == status?.Length)
            {
                status = "INFO";
            }

            string number;
            if (Numbers.ContainsKey(_numberOrName))
            {
                number = Numbers[_numberOrName];
            }
            else
            {
                number = _numberOrName;
            }

            url = config.Retrieve("sendurl", "https://api.telstra.com/v1/sms/messages");
            if (authenticated)
            {
                var client = new RestClient(url);
                var request = new RestRequest(string.Empty, Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + access_token);
                request.AddHeader("Accept", "application/json");
                Dictionary<string, string> jsonBody = new Dictionary<string, string>() { ["to"] = number, ["body"] = string.Format("[{0}] {1}", status, _message) };
                request.AddJsonBody(jsonBody);
                var response = client.Execute(request);
                var content = response.Content;
                foreach (KeyValuePair<string, string> kvp in jsonBody)
                {
                    AL?.Inform(kvp.Key, "=>", kvp.Value);
                }
                AL?.Inform("response:", response);
                
                JSON FJ = new JSON().Load(content).Deserialize();
                object fj = FJ.ToObject();

                try
                {
                    string smsStatus = ((Dictionary<string, object>)fj)["status"].ToString();
                    string smsMessage = ((Dictionary<string, object>)fj)["message"].ToString();
                    AL?.Inform(smsStatus, smsMessage);
                }
                catch (Exception E)
                {
                    AL?.Warn(E.Message);
                }

                try
                {
                    messageId = ((Dictionary<string, object>)fj)["messageId"].ToString();
                }
                catch (Exception e)
                {
                    AL?.Warn(e.Message, content);
                }
                AL?.Inform(status, number, _message);

                if (blueLogging)
                {
                    BlueLog.Module("SendSMS");
                    BlueLog.Information(DateTime.Now.ToString("HH:mm:ss.ffffzzz"),
                        Environment.MachineName,
                        Process.GetCurrentProcess().ProcessName,
                        status,
                        number,
                        "'" + _message + "'",
                        Environment.StackTrace);
                    BlueLog.Module();
                }
            }
            else
            {
                AL?.Fail("Could not authenticate TelstraSMS");
            }

            AL?.Module();

            return this;
        }

        public string GetMessageId()
        {
            return messageId;
        }

        private void LoadNumbers()
        {
            string numberNames = config.Retrieve("Numbers", string.Empty);
            if (!string.IsNullOrEmpty(numberNames))
            {
                string[] names = numberNames.Split(new char[] { ' ' });
                foreach (string name in names)
                {
                    string number = config.Retrieve(name, string.Empty);

                    if (!string.IsNullOrEmpty(number))
                    {
                        Numbers[name] = number;
                    }
                }
            }
        }

        public TelstraSMS RegisterApplicationLogging(ref ApplicationLogging ptr)
        {
            AL = ptr;
            return this;
        }
    }
}
