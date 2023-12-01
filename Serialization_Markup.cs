using System.Collections.Generic;
using System.Globalization;

using TidyManaged;

namespace QuarterMaster.Serialization
{
    public class Markup
    {
        private readonly List<string> _xml = new List<string>();

        public Markup()
        {
        }

        public Markup(string tag, string value)
        {
            GatherTag(tag, value);
        }

        public Markup(string tag, Markup[] markups)
        {
            string values = string.Empty;
            foreach (Markup m in markups)
            {
                values += m.ToString();
            }
            GatherTag(tag, values);
        }

        public Markup(string tag, Markup markup)
        {
            GatherTag(tag, markup.ToString());
        }

        public Markup(string tag, params object[] maybeMarkup)
        {
            string values = string.Empty;
            foreach (object o in maybeMarkup)
            {
                values += o.ToString();
            }
            GatherTag(tag, values);
        }

        public Markup Append(string tag, string value)
        {
            GatherTag(tag, value);
            return this;
        }

        public Markup Append(string tag, Markup[] markups)
        {
            string values = string.Empty;
            foreach (Markup m in markups)
            {
                values += m.ToString();
            }
            GatherTag(tag, values);
            return this;
        }

        private void GatherTag(string tagg, string value)
        {
            _xml.Add(string.Format(CultureInfo.InvariantCulture, "<{0}>{1}</{0}>", tagg, value));
        }

        public static string Tag(string tagg, string value)
        {
            return string.Format(CultureInfo.InvariantCulture, "<{0}>{1}</{0}>", tagg, value);
        }

        public static string Tag(string tagg, object value)
        {
            return string.Format(CultureInfo.InvariantCulture, $"<{tagg}>{value}</{tagg}>", tagg, value);
        }

        public static string Tag(string tagg, string parameters, object value)
        {
            var pars = parameters.Length > 0 ? " " + parameters : parameters;
            return string.Format(CultureInfo.InvariantCulture, $"<{tagg}{pars}>{value}</{tagg}>", tagg, value);
        }

        override public string ToString()
        {
            return string.Join("\n", _xml.ToArray());
        }
    }

    public static class MarkupFormatters
    {
        public static string FormatXml(string xml)
        {
            using Document doc = Document.FromString(xml);
            doc.AddXmlDeclaration = true;
            doc.CharacterEncoding = EncodingType.Utf8;
            doc.UseXmlParser = true;
            doc.OutputXml = true;
            doc.IndentAttributes = true;
            doc.IndentBlockElements = AutoBool.Yes;
            doc.MakeClean = true;
            doc.CleanAndRepair();
            return doc.Save();
        }

        public static string FormatHtml(string input)
        {
            using Document doc = Document.FromString(input);
            doc.DropEmptyParagraphs = true;
            //doc.DropEmptyElements = true;
            doc.DropEmptyParagraphs = true;
            doc.IndentAttributes = true;
            doc.IndentBlockElements = AutoBool.Yes;
            doc.MakeClean = true;
            doc.OutputBodyOnly = AutoBool.Yes;
            doc.OutputXhtml = true;
            doc.CharacterEncoding = EncodingType.Utf8;
            doc.CleanAndRepair();
            return doc.Save();
        }
    }
}
