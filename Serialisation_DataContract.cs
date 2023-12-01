using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace QuarterMaster.Serialization
{
    public static class DataContract
    {
        public static string ToXML<T>(T item)
        {
            var xmlSerializer = new DataContractSerializer(item.GetType());
            using MemoryStream xmlStream = new MemoryStream();
            xmlSerializer.WriteObject(xmlStream, item);
            return Encoding.UTF8.GetString(xmlStream.ToArray());
        }

        public static string ToJSON<T>(T item)
        {
            var jsonSerializer = new DataContractJsonSerializer(item.GetType());
            using MemoryStream jsonStream = new MemoryStream();
            jsonSerializer.WriteObject(jsonStream, item);
            return Encoding.UTF8.GetString(jsonStream.ToArray());
        }
    }
}
