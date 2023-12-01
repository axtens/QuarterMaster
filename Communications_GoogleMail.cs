using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace QuarterMaster.Communications
{
    public class GoogleMail
    {
        public Tuple<UserCredential, string> Mail_Authenticate(string clientIdJsonPath, string account = "", bool debug = false)
        {
            var credentialSavePath = Path.Combine(
                Path.GetDirectoryName(clientIdJsonPath),
                Path.GetFileNameWithoutExtension(clientIdJsonPath) + (string.IsNullOrEmpty(account) ? "" : "_" + account));
            Directory.CreateDirectory(credentialSavePath);
            Console.WriteLine($"Saving credentials in {credentialSavePath}");
            return Mail_AuthenticateToPath(clientIdJsonPath, credentialSavePath, debug);
        }

        public Tuple<UserCredential, string> Mail_AuthenticateToPath(string clientIdJsonPath, string credentialSavePath = "", bool debug = false)
        {
            if (debug) Debugger.Launch();
            UserCredential credential;

            if (clientIdJsonPath?.Length == 0)
            {
                return null;
            }

            string[] Scopes = {
                GmailService.Scope.MailGoogleCom
            };

            string credPath;
            using (var stream = new FileStream(clientIdJsonPath, FileMode.Open, FileAccess.Read))
            {
                credPath = Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);

                credPath = credentialSavePath?.Length == 0
                    ? Path.Combine(credPath, ".credentials/GMailLibrary.json")
                    : credentialSavePath;

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            return new Tuple<UserCredential, string>(credential, credPath);
        }

        public GmailService Mail_CreateService(UserCredential credential, string ApplicationName) =>
            new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            }
            );

        public Google.Apis.Gmail.v1.Data.Message MessageFromMailMessage(MailMessage mailMessage) {
            return new Google.Apis.Gmail.v1.Data.Message
            {
                Raw = Base64UrlEncode(MimeKit.MimeMessage.CreateFromMailMessage(mailMessage).ToString())
            };
        }

        private static string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            // Special "url-safe" base64 encode.
            return Convert.ToBase64String(inputBytes)
              .Replace('+', '-')
              .Replace('/', '_')
              .Replace("=", "");
        }

        public string Message_Send(GmailService service, string userId, MesgBlock blk, bool debug = false)
        {
            if (debug) Debugger.Launch();

            MailMessage mm = new MailMessage
            {
                Subject = blk.Subject
            };
            foreach (var addr in blk.To.Split(';'))
            {
                mm.To.Add(new MailAddress(addr));
            }
            mm.Sender = new MailAddress(blk.From);
            mm.From = new MailAddress(blk.From);
            foreach (var att in blk.Attachments.Split('|'))
            {
                mm.Attachments.Add(new Attachment(att));
            }
            mm.Body = blk.Body;
            mm.BodyEncoding = Encoding.UTF8;

            var mimeMessage = MimeKit.MimeMessage.CreateFromMailMessage(mm);
            var message = new Google.Apis.Gmail.v1.Data.Message
            {
                Raw = Base64UrlEncode(mimeMessage.ToString())
            };

            Google.Apis.Gmail.v1.Data.Message response = service.Users.Messages.Send(message, "me").Execute();

            return JsonConvert.SerializeObject(new JSONResponse() { Cargo = response });
        }

        public Google.Apis.Gmail.v1.Data.Message Message_Get_Sharp(GmailService service, string userId, string messageId, bool debug = false)
        {
            if (debug) Debugger.Launch();

            var request = service.Users.Messages.Get(userId, messageId);
            request.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;
            request.PrettyPrint = true;

            var message = request.Execute();
            return message;
        }

        public string Message_Get(GmailService service, string userId, string messageId, bool debug = false)
        {
            var message = Message_Get_Sharp(service, userId, messageId, debug);
            if (message != null)
            {
                return JsonConvert.SerializeObject(new JSONResponse() { Cargo = message });
            }
            else
            {
                return JsonConvert.SerializeObject(new JSONResponse() { Error = $"{userId} has no message {messageId}" });
            }
        }

        public Google.Apis.Gmail.v1.Data.Message Message_ChangeLabels_Sharp(GmailService service, string userId, string messageId, string addLabels, string removeLabels, bool debug = false)
        {
            if (debug) Debugger.Launch();

            var request = service.Users.Messages.Get(userId, messageId);
            request.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Raw;

            var message = request.Execute();
            if (message != null)
            {
                var response = service.Users.Messages.Modify(new ModifyMessageRequest
                {
                    AddLabelIds = addLabels.Split(','),
                    RemoveLabelIds = removeLabels.Split(',')
                }, userId, messageId).Execute();
                return response;
            }
            return null;
        }

        public string Message_ChangeLabels(GmailService service, string userId, string messageId, string addLabels, string removeLabels, bool debug = false)
        {
            var response = Message_ChangeLabels_Sharp(service, userId, messageId, addLabels, removeLabels, debug);
            return response != null
            ? JsonConvert.SerializeObject(new JSONResponse() { Cargo = response })
                : JsonConvert.SerializeObject(new JSONResponse() { Error = $"{userId} has no message {messageId}" });
        }

        public List<Google.Apis.Gmail.v1.Data.Message> Messages_ListByQuery_Sharp(GmailService service, string userId, string query, bool debug = false)
        {
            if (debug) Debugger.Launch();
            var msgList = new List<Google.Apis.Gmail.v1.Data.Message>();
            //var details = new Dictionary<string,MessagePart>();
            //var result = new JSONResponse();

            var request = service.Users.Messages.List(userId);
            request.MaxResults = 500;
            request.Q = query;

            while (true)
            {
                var response = request.Execute();
                if (response?.Messages != null)
                {
                    msgList.AddRange(response.Messages);
                    Console.WriteLine($"{msgList.Count} loaded");
                }
                var npt = response.NextPageToken;
                if (string.IsNullOrEmpty(npt)) break;
                request.PageToken = npt;
            }
            return msgList;
        }

        public string Messages_ListByQuery(GmailService service, string userId, string query, bool debug = false)
        {
            JSONResponse result = new JSONResponse
            {
                Cargo = Messages_ListByQuery_Sharp(service, userId, query, debug)
            };
            return JsonConvert.SerializeObject(result);
        }

        public string Labels_List(GmailService service, string userId, bool debug = false)
        {
            if (debug) Debugger.Launch();

            var request = service.Users.Labels.List(userId);
            var response = request.Execute();
            return JsonConvert.SerializeObject(new JSONResponse() { Cargo = response });
        }
    }
    public class JSONResponse
    {
        public string Error { get; set; }
        public object Cargo { get; set; }
        public object Crew { get; set; }
    }

    public class MesgBlock
    {
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Attachments { get; set; }
    }
}