using System.Web.Script.Serialization;

namespace QuarterMaster.Serialization
{
    public class JSON
    {
        private string _json = "";
        private object _deserialized;

        public JSON()
        {
        }

        public JSON(string json = "")
        {
            this._json = json;
        }

        public JSON Clear()
        {
            this._json = string.Empty;
            this._deserialized = null;
            return this;
        }

        public JSON Load(string json = "")
        {
            this._json = json;
            return this;
        }

        public JSON Deserialize()
        {
            this._deserialized = (new JavaScriptSerializer()).DeserializeObject(this._json);
            return this;
        }

        public object ToObject()
        {
            return (object)this._deserialized;
        }
    }
}
