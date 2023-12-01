using System.Collections.Generic;
using System.IO;
using System.Net.Mail;

namespace QuarterMaster.Communications
{
    public static class MailAttachments
    {
        public static string ProcessAttachmentsToFile(MailMessage msg, string destinationFolder)
        {
            List<string> answer = new List<string>();
            foreach (Attachment attachment in msg.Attachments)
            {
                byte[] allBytes = new byte[attachment.ContentStream.Length];
                int bytesRead = attachment.ContentStream.Read(allBytes, 0, (int)attachment.ContentStream.Length);
                string destinationFile = Path.Combine(destinationFolder, attachment.ContentId + "$" + attachment.Name);
                BinaryWriter writer = new BinaryWriter(
                    new FileStream(
                        destinationFile,
                        FileMode.OpenOrCreate,
                        FileAccess.Write,
                        FileShare.None));
                writer.Write(allBytes);
                answer.Add(destinationFile);
                writer.Close();
            }

            return string.Join("^", answer.ToArray());
        }
    }
}
