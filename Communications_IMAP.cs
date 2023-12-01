using ImapX;
using ImapX.Enums;

using System.Collections.Generic;
using System.Linq;

namespace QuarterMaster.Communications
{
    public class IMAP
    {
        private readonly ImapX.ImapClient _client;

        private string _host = "";
        private bool _hostDefined;

        private int _port;
        private bool _portDefined;

        private bool _ssl;
        private bool _sslDefined;

        private System.Security.Authentication.SslProtocols _security = System.Security.Authentication.SslProtocols.Default;
        private bool _securityDefined;

        private bool _connected;

        private string _username = "";
        private bool _usernameDefined;

        private string _password = "";
        private bool _passwordDefined;

        private bool _loggedIn;

        private ImapX.Message[] _messages;
        private bool _messagesLoaded;

        private readonly List<long> _uids = new List<long>();
        private bool _uidsDefined;

        public IMAP()
        {
            _client = new ImapClient();
        }

        public IMAP Security(System.Security.Authentication.SslProtocols protocol)
        {
            _security = protocol;
            _securityDefined = true;
            return this;
        }

        public IMAP Connect()
        {
            if (!_connected)
            {
                if (_hostDefined && _portDefined && _sslDefined && _securityDefined)
                {
                    _client.SslProtocol = _security;
                    _client.Port = _port;
                    _client.UseSsl = _ssl;
                    _client.Connect(_host, true);
                    _connected = true;
                }
                else
                {
                    throw new IMAPException("[Host() and/or Port() and/or Ssl() and/or Security() not used.]");
                }
            }
            else
            {
                throw new IMAPException("[Already Connect()ed.]");
            }
            return this;
        }

        public IMAP Disconnect()
        {
            if (_connected)
            {
                _client.Disconnect();
                _client.Dispose();
            }
            else
            {
                throw new IMAPException("[Not Connect()ed.]");
            }
            return this;
        }

        public IMAP Login()
        {
            if (!_loggedIn)
            {
                if (_usernameDefined && _passwordDefined)
                {
                    _client.Login(_username, _password);
                    _loggedIn = true;
                }
                else
                {
                    throw new IMAPException("[Username() and/or Password() not used.]");
                }
            }
            else
            {
                throw new IMAPException("[Already Login()ed.]");
            }
            return this;
        }

        public IMAP Logout()
        {
            if (_loggedIn)
            {
                _client.Logout();
                _loggedIn = false;
            }
            else
            {
                throw new IMAPException("[Not Login()ed.]");
            }
            return this;
        }

        public IMAP Host(string server)
        {
            _host = server;
            _hostDefined = true;
            return this;
        }

        public IMAP Port(int port)
        {
            _port = port;
            _portDefined = true;
            return this;
        }

        public IMAP Ssl(bool ssl)
        {
            _ssl = ssl;
            _sslDefined = true;
            return this;
        }

        public IMAP Username(string username)
        {
            _username = username;
            _usernameDefined = true;
            return this;
        }

        public IMAP Password(string password)
        {
            _password = password;
            _passwordDefined = true;
            return this;
        }

        public int MessageCount()
        {
            //in inbox
            return (int)_client.Folders.Inbox.Exists;
        }

        public List<string> FolderList()
        {
            List<string> answer = new List<string>();
            foreach (Folder f in _client.Folders.ToList())
            {
                answer.Add(f.Name);
            }
            return answer;
        }

        public IMAP RetrieveMessages()
        {
            _client.Behavior.MessageFetchMode = MessageFetchMode.Full | MessageFetchMode.GMailExtendedData;
            ImapX.Collections.MessageCollection messages = _client.Folders.Inbox.Messages;
            messages.Download("ALL");
            _messages = messages.ToArray<Message>();
            _messagesLoaded = true;
            foreach (Message m in _messages)
            {
                _uids.Add(m.UId);
            }
            _uidsDefined = true;
            return this;
        }

        public IMAP RemoveRetrievedMessages()
        {
            if (_uidsDefined)
            {
                var result = _client.Folders.Inbox.Search(_uids.ToArray<long>());
                foreach (Message m in result)
                {
                    m.Remove();
                }
                return this;
            }
            else
            {
                throw new IMAPException("[No UId list defined.]");
            }
        }

        public IMAP ExpungeRemovedRecords()
        {
            _client.Folders.Inbox.Expunge();
            return this;
        }

        public string RetrieveRawMessage(long i)
        {
            if (_messagesLoaded && i >= 0 && i < _messages.Length)
            {
                return _messages[i].DownloadRawMessage();
            }
            else
            {
                throw new IMAPException("[Messages not retrieved.]");
            }
        }

        public Dictionary<string, string> RetrieveBodyPartsZeroParameters(long i)
        {
            if (_messagesLoaded && i >= 0 && i < _messages.Length)
            {
                return _messages[i].BodyParts[0].Parameters;
            }
            else
            {
                throw new IMAPException("[Messages not Load()ed or no messages.]");
            }
        }

        public Dictionary<string, object> RetrieveBodyFromMessageAsDictionary(long i)
        {
            Dictionary<string, object> answer = new Dictionary<string, object>();
            if (_messagesLoaded && i >= 0 && i < _messages.Length)
            {
                ImapX.MessageBody mb = _messages[i].Body;
                answer.Add("HasHtml", mb.HasHtml);
                answer.Add("HasText", mb.HasText);
                answer.Add("Html", mb.Html);
                answer.Add("Text", mb.Text);
                answer.Add("Downloaded", mb.Downloaded);
                return answer;
            }
            else
            {
                throw new IMAPException("[Messages not Load()ed or no messages.]");
            }
        }

        public Dictionary<string, string> RetrieveHeadersFromMessageAsDictionary(long i)
        {
            Dictionary<string, string> answer = new Dictionary<string, string>();
            if (_messagesLoaded && i >= 0 && i < _messages.Length)
            {
                foreach (KeyValuePair<string, string> kvp in _messages[i].Headers)
                {
                    answer.Add(kvp.Key, kvp.Value);
                }
                return answer;
            }
            else
            {
                throw new IMAPException("[Messages not Load()ed or no messages.]");
            }
        }

        public Dictionary<string, object> RetrieveMessageAsDictionary(long i)
        {
            if (_messagesLoaded)
            {
                Dictionary<string, object> FD = new Dictionary<string, object>();
                if (i >= 0 && i < _messages.Length)
                {
                    object data = _messages[i];
                    FD =
                        (from x in data.GetType().GetProperties() select x)
                                .ToDictionary(x => x.Name, x => (
                                    x.GetGetMethod()
                                            .Invoke(data, null) ?? ""
                                            ));
                }

                return FD;
            }
            else
            {
                throw new IMAPException("[RetrieveMessages() not called.]");
            }
        }
    }
}
