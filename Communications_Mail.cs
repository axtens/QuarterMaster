using System;
using System.Net;
using System.Net.Mail;

namespace QuarterMaster.Communications
{
    public class Mail
    {
        public string Postoffice { set; get; }
        public int Port { set; get; }
        public string Account { set; get; }
        public string Password { set; get; }
        public bool SSL { set; get; }
        public string ErrorString;

        public bool Send(string From, string To, string Subject, string Body)
        {
            if (Postoffice == null || Account == null || Password == null || 0 == Port)
            {
                throw new Exception("Postoffice, Account, Password or Port not set.");
            }

            bool answer = false;
            MailMessage mail = new MailMessage()
            {
                From = new MailAddress(From)
            };
            string[] aTos = To.Split(';');
            foreach (var sTo in aTos)
            {
                mail.To.Add(sTo);
            }
            mail.Subject = Subject;
            mail.Body = Body;

            SmtpClient smtp = new SmtpClient(Postoffice, Port)
            {
                EnableSsl = SSL,
                Credentials = new NetworkCredential(Account, Password)
            };
            try
            {
                smtp.Send(mail);
                answer = true;
            }
            catch (Exception ex)
            {
                ErrorString = ex.ToString();
                answer = false;
            }
            finally
            {
                mail.Dispose();
                smtp.Dispose();
            }

            return answer;
        }

        public bool Send(string account, string password, string postoffice, int port, bool ssl, string from, string to, string subject, string message)
        {
            this.Account = account;
            this.Password = password;
            this.Postoffice = postoffice;
            this.Port = port;
            this.SSL = ssl;
            return Send(from, to, subject, message);
        }

        public void Shriek(string From, string To, string Subject, string Body)
        {
            MailMessage mail = new MailMessage()
            {
                From = new MailAddress(From)
            };
            string[] aTos = To.Split(';');
            foreach (var sTo in aTos)
            {
                mail.To.Add(sTo);
            }
            mail.Subject = Subject;
            mail.Body = Body;

            SmtpClient smtp = new SmtpClient(Postoffice, Port)
            {
                EnableSsl = SSL,
                Credentials = new NetworkCredential(Account, Password)
            };
            try
            {
                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                ErrorString = ex.ToString();
            }
            finally
            {
                mail.Dispose();
                smtp.Dispose();
            }

            return;
        }

        public void SendMessage(string account, string password, string postoffice, int port, bool ssl, string from, string to, string subject, string message)
        {
            this.Account = account;
            this.Password = password;
            this.Postoffice = postoffice;
            this.Port = port;
            this.SSL = ssl;
            Shriek(from, to, subject, message);
        }
    }
}

