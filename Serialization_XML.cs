using System;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace QuarterMaster.Serialization
{
    public static class XML
    {
        /// <summary>Returns the contents of the datatable as xml</summary>
        /// <param name="dataTable">table to be serialized</param>
        /// <param name="dataTableName">name to be applied to table</param>
        /// <returns>string</returns>
        public static string GetXmlOfDataTable(DataTable dataTable, string dataTableName )
        {
            dataTable.TableName = dataTableName;
            MemoryStream ms = new MemoryStream();
            dataTable.WriteXml(ms);
            ms.Position = 0;
            byte[] bytes = new byte[ms.Length];
            ms.Read(bytes, 0, (int)ms.Length);
            return Encoding.Default.GetString(bytes);
        }

        /// <summary>Serializes the data in the object to the designated file path</summary>
        /// <typeparam name="T">Type of Object to serialize</typeparam>
        /// <param name="dataToSerialize">Object to serialize</param>
        /// <param name="filePath">FilePath for the XML file</param>
        public static void Serialize<T>(T dataToSerialize, string filePath)
        {
            try
            {
                using Stream stream = File.Open(filePath, FileMode.Create, FileAccess.ReadWrite);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                XmlTextWriter writer = new XmlTextWriter(stream, Encoding.Default)
                {
                    Formatting = Formatting.Indented
                };
                serializer.Serialize(writer, dataToSerialize);
                writer.Close();
            }
            catch
            {
                throw;
            }
        }

        public static string PrettyXml(string xml, bool ansi = true, bool omitXMLDeclaration = true, bool indent = true, bool newLineOnAttributes = true)
        {
            var stringBuilder = new StringBuilder();

            var element = XElement.Parse(xml);

            var settings = new XmlWriterSettings
            {
                Encoding = ansi ? Encoding.ASCII : Encoding.Default,
                OmitXmlDeclaration = omitXMLDeclaration,
                Indent = indent,
                NewLineOnAttributes = newLineOnAttributes
            };

            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                element.Save(xmlWriter);
            }

            return stringBuilder.ToString();
        }

        public static XmlDocument ConvertHtmlToXml(TextReader reader)
        {
            // setup SgmlReader
            Sgml.SgmlReader sgmlReader = new Sgml.SgmlReader
            {
                DocType = "HTML",
                WhitespaceHandling = WhitespaceHandling.All,
                CaseFolding = Sgml.CaseFolding.ToLower,
                InputStream = reader
            };

            // create document
            XmlDocument doc = new XmlDocument
            {
                PreserveWhitespace = true,
                XmlResolver = null
            };
            doc.Load(sgmlReader);
            return doc;
        }

        public static XmlDocument ConvertHtmlToXml(StreamReader reader)
        {
            // setup SgmlReader
            Sgml.SgmlReader sgmlReader = new Sgml.SgmlReader
            {
                DocType = "HTML",
                WhitespaceHandling = WhitespaceHandling.All,
                CaseFolding = Sgml.CaseFolding.ToLower,
                InputStream = reader
            };

            // create document
            XmlDocument doc = new XmlDocument
            {
                PreserveWhitespace = true,
                XmlResolver = null
            };
            doc.Load(sgmlReader);
            return doc;
        }
    }
}
