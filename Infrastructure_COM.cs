using mshtml;

using System;

namespace QuarterMaster.Infrastructure
{
    public class InternetExplorer
    {
        private static SHDocVw.InternetExplorer internetExplorer;
        private static string errorMessage;
        private object empty = 0;

        public InternetExplorer()
        {
        }

        public string ErrorMessage
        {
            get
            {
                string temp = errorMessage;
                errorMessage = string.Empty;
                return temp;
            }

            set
            {
                errorMessage = value;
            }
        }

        public void Launch(bool visible = true)
        {
            EventHandlers e = new EventHandlers();
            internetExplorer = new SHDocVw.InternetExplorer();

            // override BeforeNavigate2 event - was in original; no idea what it's there for.
            internetExplorer.BeforeNavigate2 += new
                 SHDocVw.DWebBrowserEvents2_BeforeNavigate2EventHandler(
                         e.OnBeforeNavigate2);

            internetExplorer.Visible = visible;
        }

        public void Navigate(object url)
        {
            internetExplorer.Navigate2(ref url, ref this.empty, ref this.empty, ref this.empty, ref this.empty);
            while (internetExplorer.Busy && (internetExplorer.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_LOADED))
            {
                System.Threading.Thread.Sleep(500);
            }
        }

        public void Quit()
        {
            internetExplorer.Quit();
        }

        public string GetTitle()
        {
            string title;
            do
            {
                try
                {
                    // drawing the title directly from oIE.Document kept crashing
                    // ... thus the reliance on HTMLDocument 
                    HTMLDocument doc = internetExplorer.Document;
                    title = doc.title;
                }
                catch (Exception e)
                {
                    errorMessage = e.Message;
                    System.Threading.Thread.Sleep(100);
                    title = string.Empty;
                }
            }
            while (title.IndexOf("state=acit", StringComparison.CurrentCulture) == -1);

            // keep looping around until state=acit appears in the title
            return title;
        }

        private class EventHandlers
        {
            public void OnBeforeNavigate2(
                object sender,
                ref object url,
                ref object flags,
                ref object target,
                ref object postData,
                ref object headers,
                ref bool cancel)
            {
                // Console.WriteLine("BeforeNavigate2 fired!");
            }
        }
    }
}
