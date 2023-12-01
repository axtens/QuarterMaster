using System;
using System.IO;
using System.Net;

namespace QuarterMaster.Communications
{
    public static class Downloader
    {
        public static bool BinaryDownload(string uri, string fileName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                return false;
            }

            // Check that the remote file was found. The ContentType
            // check is performed since a request for a non-existent
            // image file might be redirected to a 404-page, which would
            // yield the StatusCode "OK", even though the image was not
            // found.
            if ((response.StatusCode == HttpStatusCode.OK
                || response.StatusCode == HttpStatusCode.Moved
                || response.StatusCode == HttpStatusCode.Redirect)
                && response.ContentType.StartsWith("audio", StringComparison.OrdinalIgnoreCase))
            {
                // if the remote file was found, download it
                using (Stream inputStream = response.GetResponseStream())
                using (Stream outputStream = File.OpenWrite(fileName))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
                response.Dispose();
                return true;
            }
            else
            {
                response.Dispose();
                return false;
            }
        }
    }
}
