using Newtonsoft.Json;

using System;
using System.Diagnostics;

using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML.Messaging;
using Twilio.Types;


namespace QuarterMaster.Communications
{
    public class Twilio
    {        // private readonly List<Uri> mediaUrlList;

        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string FromNumber { get; set; }
        public string CallTwiMLText { get; set; }

        // public void AddRichTextMedia(string url, bool debug = false)
        // {
        //     mediaUrlList.Add(new Uri(url));
        // }

        public Twilio()
        {
            // mediaUrlList = new List<Uri>();
        }

        public Twilio(bool debug = false)
        {
            // mediaUrlList = new List<Uri>();
        }

        public Twilio(string accountSid, string authToken, bool debug = false)
        {
            AccountSid = accountSid;
            AuthToken = authToken;
            Authenticate(debug);
        }

        public Twilio(string accountSid, string authToken, string fromNumber, bool debug = false)
        {
            AccountSid = accountSid;
            AuthToken = authToken;
            FromNumber = fromNumber;
            Authenticate(debug);
        }

        public string Authenticate(bool debug = false)
        {
            if (debug) Debugger.Launch();

            var result = new JSONResponse();
            if (String.IsNullOrEmpty(AccountSid))
            {
                result.Error = "AccountSid is null or empty";
                result.Cargo = null;
            }
            if (String.IsNullOrEmpty(AuthToken))
            {
                result.Error = "AuthToken is null or empty";
                result.Cargo = null;
            }
            if (result.Error == null)
            {
                TwilioClient.Init(AccountSid, AuthToken);
            }
            return JsonConvert.SerializeObject(result);
        }

        public string Call(string to, bool debug = false) => Call(FromNumber, to, CallTwiMLText, debug);

        public string Call(string to, string text, bool debug = false) => Call(FromNumber, to, text, debug);

        public string Call(string from, string to, string text, bool debug = false)
        {
            if (debug) Debugger.Launch();

            var result = new JSONResponse();
            try
            {
                var message = CallResource.Create(
                    to: new PhoneNumber(to),
                    from: new PhoneNumber(from),
                    twiml: new Twiml(text));
                result.Error = null;
                result.Cargo = message;
            }
            catch (Exception e)
            {
                result.Error = e.Message;
                result.Cargo = null;
            }
            return JsonConvert.SerializeObject(result);
        }
        public string Text(string to, string body, bool debug = false) => Text(FromNumber, to, body, debug);

        public string Text(string from, string to, string body, bool debug = false)
        {
            if (debug) Debugger.Launch();

            var result = new JSONResponse();
            try
            {
                var message = MessageResource.Create(
                    to: new PhoneNumber(to),
                    from: new PhoneNumber(from),
                    body: body);

                result.Error = null;
                result.Cargo = message;
            }
            catch (Exception e)
            {
                result.Error = e.Message;
                result.Cargo = null;
            }
            return JsonConvert.SerializeObject(result);
        }

        // public string RichText(string to, string body, bool debug = false) => RichText(fromNumber, to, body, debug);
        // 
        // public string RichText(string from, string to, string body, bool debug = false)
        // {
        //     if (debug) Debugger.Launch();
        // 
        //     var result = new JSONResponse();
        //     try
        //     {
        //         var message = MessageResource.Create(
        //             body: body,
        //             from: new Twilio.Types.PhoneNumber(from),
        //             to: new Twilio.Types.PhoneNumber(to),
        //             mediaUrl: mediaUrlList
        //             );
        // 
        //         result.Error = null;
        //         result.Cargo = message;
        //     }
        //     catch (Exception e)
        //     {
        //         result.Error = e.Message;
        //         result.Cargo = null;
        //     }
        //     return JsonConvert.SerializeObject(result);
        // }

    }
}
