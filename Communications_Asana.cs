using QuarterMaster.Communications.Rest;
using QuarterMaster.Configuration;
using QuarterMaster.Debugging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;

namespace QuarterMaster.Communications
{
    internal class Result
    {
        public bool _err;
        public object cargo;
    }

    internal class ResultExtensions
    {
        public string url;
        public string body;
        public string content;
        public string reqhead;
        public string resphead;
    }

    internal class Memberships
    {
        public string project;
        public string section;
    }

    internal class Followers
    {
        public string gid;
    }

    internal class Assignees
    {
        public string gid;
    }

    internal class WPSData
    {
        public string name;
        public string workspace;
        public object[] memberships;
        public object[] followers;
        public string notes;
        public object assignee;
    }

    public class Asana
    {
        private static string _assignee { get; set; }
        private static string _notes { get; set; }
        private static List<string> _followers { get; set; }
        private static List<string> _projects { get; set; }
        private static string _name { get; set; }
        private static string _workspace { get; set; }
        private static string _token { get; set; }
        private static DateTime _due_at { get; set; }
        private static string _task { get; set; }
        private static string _text { get; set; }
        private static string _section { get; set; }
        private static INI _ini { get; set; }

        public string Result { get; set; }
        public string SettingsFile { get; set; }
        public bool Authenticated { get; set; }
        public bool Transmitted { get; set; }

        public Asana()
        {
            Init();
        }

        public Asana(string settingsFile)
        {
            Init();
            SettingsFile = settingsFile;
            if (System.IO.File.Exists(SettingsFile))
            {
                _ini = new INI(SettingsFile);
                var tkn = _ini.IniReadValue("Authorisation", "Token", "");
                if (!string.IsNullOrEmpty(tkn))
                {
                    Token(tkn);
                }
            }
        }

        private void Init()
        {
            _followers = new List<string>();
            _projects = new List<string>();
            _assignee = string.Empty;
            _workspace = string.Empty;
            _due_at = DateTime.MinValue;
            _task = string.Empty;
            Authenticated = false;
            Transmitted = false;
        }

        public Asana Due_At(DateTime date)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            _due_at = date;
            return this;
        }

        public Asana Assignee(string assig)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            _assignee = _ini.IniReadValue("Assignees", assig, "");
            return this;
        }

        public Asana Task(string tsk)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            _task = _ini.IniReadValue("Tasks", tsk, "");
            return this;
        }

        public Asana Text(string txt)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            _text = txt;
            return this;
        }

        public Asana Notes(string note)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            _notes = note;
            return this;
        }

        public Asana Follower(string follower)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            _followers.Add(_ini.IniReadValue("Followers", follower, ""));
            return this;
        }

        public Asana Project(string project)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            _projects.Add(_ini.IniReadValue("Projects", project, ""));
            return this;
        }

        public Asana Name(string nam)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            _name = nam;
            return this;
        }

        public Asana Token(string tok)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            _token = tok;
            Authenticated = GetMe(tok);
            return this;
        }

        public Asana Section(string sec)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            _section = _ini.IniReadValue("Sections", sec, "");
            return this;
        }

        private bool GetMe(string tkn)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            var R = new Fluent()
                .Url("https://app.asana.com")
                .Verb("GET")
                .Path("api")
                .Path("1.0")
                .Path("users")
                .Path("me")
                .Head("Authorization", "Bearer " + tkn);
            var RR = R.Send();
            if (RR.IsOK())
            {
                return RR.Status() == 200;
            }
            else
            {
                return false;
            }
        }

        public Asana Workspace(string worksp)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            _workspace = _ini.IniReadValue("Workspaces", worksp, "");
            return this;
        }

        public Asana CreateNewTask() => Create();

        public Asana UpdateTask()
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            var R = new Fluent()
                .Url("https://app.asana.com")
                .Verb("PUT")
                .Head("Content-Type", "application/x-www-form-urlencoded")
                .Path("api")
                .Path("1.0")
                .Path("tasks");

            if (!string.IsNullOrEmpty(_task))
            {
                R.Path(_task);
            }

            if (!string.IsNullOrEmpty(_token))
            {
                R.Head("Authorization", "Bearer " + _token);
            }

            var body = new List<string>();

            if (!string.IsNullOrEmpty(_assignee))
            {
                body.Add("assignee=" + WebUtility.UrlEncode(_assignee));
            }

            if (!string.IsNullOrEmpty(_notes))
            {
                body.Add("notes=" + WebUtility.UrlEncode(_notes));
            }

            if (_due_at != DateTime.MinValue)
            {
                body.Add("due_at=" + WebUtility.UrlEncode(_due_at.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")));
            }

            if (_projects.Count > 0)
            {
                int f = 0;
                foreach (string project in _projects)
                {
                    body.Add("projects[" + f + "]=" + WebUtility.UrlEncode(_projects[f]));
                    f++;
                }
            }

            R.Body(string.Join("&", body.ToArray()));

            var RR = R.Send();
            if (RR.IsOK())
            {
                Transmitted = RR.Status() == 201;
                Result = SimpleJson.SerializeObject(new Result()
                {
                    _err = false,
                    cargo = new ResultExtensions()
                    {
                        url = RR.URL,
                        body = RR.BODY,
                        reqhead = RR.REQHEAD,
                        resphead = RR.ResponseHeaders(),
                        content = RR.Response()
                    }
                });
            }
            else
            {
                Result = SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = RR.StatusMessage()
                });
            }

            return this;
        }

        public string CreateNewTaskInWorkspaceProjectSection(string section, string name, string notes)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            if (section == null)
            {
                return SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = $"{section} is null in {SettingsFile}"
                });
            }
            if (name == null)
            {
                return SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = $"{name} is null in {SettingsFile}"
                });
            }
            if (notes == null)
            {
                return SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = $"{notes} is null in {SettingsFile}"
                });
            }

            if (Authenticated)
            {
                // Workspace and Project
                Section(section);
                Notes(notes);
                Name(name);
                CreateNewTaskInWorkspaceProjectSection();
                return Result;
            }
            else
            {
                return SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = $"Could not authenticate with Token in {SettingsFile}"
                });
            }
        }

        public string CreateNewTaskInWorkspaceProjectSection(string workspace, string project, string section, string name, string notes)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            if (workspace == null)
            {
                return SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = $"{workspace} is null in {SettingsFile}"
                });
            }
            if (project == null)
            {
                return SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = $"{project} is null in {SettingsFile}"
                });
            }
            if (section == null)
            {
                return SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = $"{section} is null in {SettingsFile}"
                });
            }
            if (name == null)
            {
                return SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = $"{name} is null in {SettingsFile}"
                });
            }
            if (notes == null)
            {
                return SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = $"{notes} is null in {SettingsFile}"
                });
            }

            if (Authenticated)
            {
                Section(section);
                Workspace(workspace);
                Project(project);
                Notes(notes);
                Name(name);
                CreateNewTaskInWorkspaceProjectSection();
                return Result;
            }
            else
            {
                return SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = $"Could not authenticate with Token in {SettingsFile}"
                });
            }
        }

        public Asana CreateNewTaskInWorkspaceProjectSection()
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            // how do we add Followers?

            List<Followers> followers = (from string f in _followers
                                         select new Followers() { gid = f }).ToList();

            //List<Assignees> assignees = new List<Assignees>();
            //assignees.Add(new Assignees() { gid = _assignee });

            //var assig = new Assignees() { gid = _assignee };

            WPSData elt = new WPSData
            {
                name = _name,
                workspace = _workspace,
                notes = _notes,
                memberships = new object[] { new Memberships
                {
                    project = _projects[0],
                    section = _section
                }},
                followers = Enumerable.ToArray(followers),
                assignee = new Assignees()
                {
                    gid = _assignee
                }
            };

            var json = "{\"data\":" + SimpleJson.SerializeObject(elt) + "}";

            var R = new Fluent()
                .Verb("POST")
                .Head("Authorization", "Bearer " + _token)
                .Head("Content-Type", "application/json")
                .Url("https://app.asana.com")
                .Path("api")
                .Path("1.0")
                .Path("tasks")
                .Body(json);

            var RR = R.Send();
            if (RR.IsOK())
            {
                Transmitted = RR.Status() == 201;
                Result = SimpleJson.SerializeObject(new Result()
                {
                    _err = false,
                    cargo = new ResultExtensions()
                    {
                        url = RR.URL,
                        body = RR.BODY,
                        reqhead = RR.REQHEAD,
                        resphead = RR.ResponseHeaders(),
                        content = RR.Response()
                    }
                });
            }
            else
            {
                Result = SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = RR.StatusMessage()
                });
            }

            return this;
        }

        public string CommentTask(string tsk, string txt)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            if (tsk == null)
            {
                return SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = "task cannot be null"
                });
            }

            if (Authenticated)
            {
                Task(tsk);
                Text(txt);
                CommentTask();
                return Result;
            }
            else
            {
                return SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = $"Could not authenticate with Token in {SettingsFile}"
                });
            }
        }

        public string CommentTask(string tkn, string tsk, string txt)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            Token(tkn);
            Task(tsk);
            Text(txt);
            return CommentTask().Result;
        }

        public Asana CommentTask()
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            var R = new Fluent()
                .Url("https://app.asana.com")
                .Verb("POST")
                .Head("Content-Type", "application/x-www-form-urlencoded")
                .Path("api")
                .Path("1.0")
                .Path("tasks");

            if (!string.IsNullOrEmpty(_task))
            {
                R.Path(_task);
                R.Path("stories");
            }

            if (!string.IsNullOrEmpty(_token))
            {
                R.Head("Authorization", "Bearer " + _token);
            }

            var body = new List<string>();

            if (!string.IsNullOrEmpty(_text))
            {
                body.Add("text=" + WebUtility.UrlEncode(_text));
            }

            R.Body(string.Join("&", body.ToArray()));

            var RR = R.Send();
            if (RR.IsOK())
            {
                Transmitted = RR.Status() == 201;
                Result = SimpleJson.SerializeObject(new Result()
                {
                    _err = false,
                    cargo = new ResultExtensions()
                    {
                        url = RR.URL,
                        body = RR.BODY,
                        reqhead = RR.REQHEAD,
                        resphead = RR.ResponseHeaders(),
                        content = RR.Response()
                    }
                });
            }
            else
            {
                Result = SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = RR.StatusMessage()
                });
            }

            return this;
        }

        public Asana Create()
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            var R = new Fluent();
            R.Url("https://app.asana.com/api/1.0/tasks")
                .Verb("POST")
                .Head("Content-Type", "application/x-www-form-urlencoded");

            if (!string.IsNullOrEmpty(_token))
            {
                R.Head("Authorization", "Bearer " + _token);
            }

            var body = new List<string>();

            if (!string.IsNullOrEmpty(_assignee))
            {
                body.Add("assignee=" + WebUtility.UrlEncode(_assignee));
            }

            if (!string.IsNullOrEmpty(_notes))
            {
                body.Add("notes=" + WebUtility.UrlEncode(_notes));
            }

            if (_followers.Count > 0)
            {
                int f = 0;
                foreach (string follower in _followers)
                {
                    body.Add("followers[" + f + "]=" + WebUtility.UrlEncode(_followers[f]));
                    f++;
                }
            }

            if (_projects.Count > 0)
            {
                int f = 0;
                foreach (string project in _projects)
                {
                    body.Add("projects[" + f + "]=" + WebUtility.UrlEncode(_projects[f]));
                    f++;
                }
            }

            if (!string.IsNullOrEmpty(_name))
            {
                body.Add("name=" + WebUtility.UrlEncode(_name));
            }

            if (!string.IsNullOrEmpty(_workspace))
            {
                body.Add("workspace=" + WebUtility.UrlEncode(_workspace));
            }

            if (_due_at != DateTime.MinValue)
            {
                body.Add("due_at=" + WebUtility.UrlEncode(_due_at.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")));
            }

            R.Body(string.Join("&", body.ToArray()));

            var RR = R.Send();
            if (RR.IsOK())
            {
                Transmitted = RR.Status() == 201;
                Result = SimpleJson.SerializeObject(new Result()
                {
                    _err = false,
                    cargo = new ResultExtensions()
                    {
                        url = RR.URL,
                        body = RR.BODY,
                        reqhead = RR.REQHEAD,
                        resphead = RR.ResponseHeaders(),
                        content = RR.Response()
                    }
                });
            }
            else
            {
                Result = SimpleJson.SerializeObject(new Result()
                {
                    _err = true,
                    cargo = RR.StatusMessage()
                });
            }
            return this;
        }
    }
}
