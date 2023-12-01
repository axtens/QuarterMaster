using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace QuarterMaster.Configuration
{
    public class Config
    {
        public string Text { get; set; }
        public string Path { get; set; }
        public bool Loaded { get; set; }
        public  bool Retrieved { get; set; }
        public bool ErrOnNull { get; set; }

        public Config()
        {
            this.Loaded = false;
            this.Text = string.Empty;
            this.Path = System.IO.Path.ChangeExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, ".cfg");
            if (File.Exists(this.Path))
            {
                this.Text = File.ReadAllText(this.Path);
                this.Text = this.Text.Replace("\r\n", "\n");
                this.Loaded = true;
            }
            else
            {
                this.Text = string.Empty;
                this.Loaded = false;
            }

            return;
        }

        public Config(string filename)
        {
            this.Loaded = false;
            this.Path = filename;
            if (File.Exists(this.Path))
            {
                this.Text = File.ReadAllText(this.Path);
                this.Text = this.Text.Replace("\r\n", "\n");
                this.Loaded = true;
            }
            else
            {
                this.Text = string.Empty;
                this.Loaded = false;
            }

            return;
        }

        public bool Reload(string filename)
        {
            Loaded = false;
            Path = filename;
            if (File.Exists(Path))
            {
                Text = File.ReadAllText(Path);
                Text = Text.Replace("\r\n", "\n");
                Loaded = true;
            }
            else
            {
                Text = string.Empty;
                Loaded = false;
            }
            return Loaded;
        }

        public bool Reload()
        {
            Loaded = false;
            if (File.Exists(Path))
            {
                Text = File.ReadAllText(Path);
                Text = Text.Replace("\r\n", "\n");
                Loaded = true;
            }
            else
            {
                Text = string.Empty;
                Loaded = false;
            }
            return Loaded;
        }

        public string Retrieve(string symbol)
        {
            this.Retrieved = false;
            Regex regex = new Regex("^" + symbol + "=(.*?)$", RegexOptions.Multiline | RegexOptions.CultureInvariant);
            Match match = regex.Match(this.Text);
            if (match.Success)
            {
                this.Retrieved = true;
                return match.Groups[1].Value;
            }
            else
            {
                if (this.ErrOnNull)
                {
                    Console.WriteLine("Error: Cannot find symbol '{0}' in {1}", symbol, this.Path);
                    //Environment.Exit(1);
                    return string.Empty;
                }
                else
                {
                    return null;
                }
            }
        }

        public Dictionary<object, object> RetrieveAll()
        {
            Dictionary<object, object> dict = new Dictionary<object, object>();
            Regex regex = new Regex(@"^(.*?\=.*?)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(this.Text);
            foreach (Match m in matches)
            {
                string[] parts = m.Value.Split(new char[] { '=' }, 2);
                dict[parts[0]] = parts[1];
            }

            return dict;
        }

        public int Retrieve(string symbol, int defaultValue)
        {
            string result = this.Retrieve(symbol, defaultValue.ToString());
            return int.Parse(result);
        }

        public bool Retrieve(string symbol, bool defaultValue)
        {
            string result = this.Retrieve(symbol, defaultValue.ToString());
            return bool.Parse(result);
        }

        public string Retrieve(string symbol, string defaultValue)
        {
            this.Retrieved = false;
            Regex regex = new Regex("^" + symbol + "=(.*?)$", RegexOptions.Multiline);
            Match match = regex.Match(this.Text);
            if (match.Success)
            {
                this.Retrieved = true;
                return match.Groups[1].Value;
            }
            else
            {
                return defaultValue;
            }
        }

        public void Define(string symbol, int value)
        {
            string symbolsValue = value.ToString();
            this.Define(symbol, symbolsValue);
        }

        public void Define(string symbol, string value)
        {
            string symbolValuePair = symbol + "=" + value;
            Regex regex = new Regex("^" + symbol + "=(.*?)$", RegexOptions.Multiline);
            Match match = regex.Match(this.Text);
            if (match.Success)
            {
                this.Text = regex.Replace(this.Text, symbolValuePair);
            }
            else
            {
                this.Text = this.Text + "\n" + symbolValuePair;
            }
        }

        public void Save()
        {
            File.WriteAllText(this.Path, this.Text);
        }

        public void Save(string filename)
        {
            File.WriteAllText(filename, this.Text);
        }

        public string GetFilename()
        {
            return this.Path;
        }

        public bool WasLoaded()
        {
            return this.Loaded;
        }

        public bool WasRetrieved()
        {
            return this.Retrieved;
        }

        public void SetErrorOnNull(bool f = false)
        {
            this.ErrOnNull = f;
            return;
        }
    }
}
