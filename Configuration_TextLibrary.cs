using QuarterMaster.Serialization;

using System;
using System.Collections.Generic;

namespace QuarterMaster.Configuration
{
    public class TextLibrary
    {
        public TextLibrary(string tlbFile)
        {
            if (System.IO.File.Exists(tlbFile))
            {
                Found = true;
                File = tlbFile;
                Lines = new List<string>(System.IO.File.ReadAllLines(tlbFile));
            }
        }

        public string File { get; } = string.Empty;
        public List<string> Lines { get; }
        public bool Found { get; }

        public string Get1(string _section) =>
            Get_AsOneLine(this, _section, string.Empty, new Dictionary<string, object>());

        public string Get1(string _section, string _default) =>
            Get_AsOneLine(this, _section, _default, new Dictionary<string, object>());

        public string Get1(string _section, string _default, string json) =>
            Get_AsOneLine(this, _section, _default, JsonToDict(json));

        private static string Get_AsOneLine(TextLibrary _instance, string _section, string _default, Dictionary<string, object> _replacements)
        {
            string[] result = Get_AsArray(_instance, _section, new string[] { _default }, _replacements);
            return result[0];
        }

        public string[] GetArray(string _section) =>
            TextLibrary.Get_AsArray(this, _section, new string[] { }, new Dictionary<string, object>());

        public string[] GetArray(string _section, string[] _default) =>
            TextLibrary.Get_AsArray(this, _section, _default, new Dictionary<string, object>());

        public string[] GetArray(string _section, string[] _default, string json) =>
            TextLibrary.Get_AsArray(this, _section, _default, JsonToDict(json));

        private static string[] Get_AsArray(
            TextLibrary _instance,
            string _section,
            string[] _default,
            Dictionary<string, object> _replacements)
        {
            if (!_instance.Found)
            {
                return _default;
            }
            List<string> ls = new List<string>();
            bool insideBlock = false;
            bool foundBlock = false;
            foreach (string line in _instance.Lines)
            {
                if (line == "[" + _section + "]")
                {
                    insideBlock = true;
                    foundBlock = true;
                }
                else if (line.StartsWith("[", StringComparison.CurrentCulture))
                {
                    if (insideBlock)
                    {
                        break;
                    }
                }
                else
                {
                    if (insideBlock)
                    {
                        ls.Add(line);
                    }
                }
            }
            if (!foundBlock)
            {
                return _default;
            }
            else
            {
                //string result = sb.ToString();
                for (var i = 0; i < ls.Count; i++)
                {
                    var s = ls[i];
                    foreach (KeyValuePair<string, object> nvp in _replacements)
                    {
                        ls[i] = s.Replace("{" + nvp.Key + "}", $"{nvp.Value}");
                    }
                }
                return ls.ToArray();
            }
        }

        public string GetBlock(string _section) =>
            TextLibrary.Get_AsTextBlock(this, _section, string.Empty, new Dictionary<string, object>());

        public string GetBlock(string _section, string _default) =>
            TextLibrary.Get_AsTextBlock(this, _section, _default, new Dictionary<string, object>());

        public string GetBlock(string _section, string _default, string json) =>
            TextLibrary.Get_AsTextBlock(this, _section, _default, JsonToDict(json));

        private static string Get_AsTextBlock(TextLibrary _instance, string _section, string _default, Dictionary<string, object> _replacements)
        {
            string[] result = Get_AsArray(_instance, _section, new string[] { _default }, _replacements);
            return String.Join("\r\n", result);
        }

        private Dictionary<string, object> JsonToDict(string json)
        {
            var fj = new JSON(json);
            return (Dictionary<string, object>)fj.Deserialize().ToObject();
        }
    }
}
