//using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.Common.Lib;

using Ionic.Zip;

using Microsoft.ClearScript;
using Microsoft.ClearScript.Windows;

//using OpenPop.Pop3;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
//using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

using QuarterMaster.Browsers;
using QuarterMaster.Communications;
using QuarterMaster.Communications.Rest;
using QuarterMaster.Communications.Rest.COM;
using QuarterMaster.Configuration;
using QuarterMaster.Data;
using QuarterMaster.Debugging;
//using QuarterMaster.GoogleAdWords;
using QuarterMaster.GoogleOAuth2;
using QuarterMaster.Infrastructure;
using QuarterMaster.Logging;
using QuarterMaster.Serialization;

using S22.Imap;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace QuarterMaster.Scripting
{
    public class Ruleset
    {
        public static JScriptEngine JSE;
        private static bool runOnceOnly;

        private static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            //T.Error().Send(Process.GetCurrentProcess().ProcessName + ": unhandled exception caught: " + e.Message);
            //T.Error().Send(e.StackTrace);
            //T.Error().Send(string.Format(CultureInfo.InvariantCulture, "Runtime terminating: {0}", args.IsTerminating));
            System.Console.WriteLine(e.Message);
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        public Ruleset(bool runOnce = false)
        {
            runOnceOnly = runOnce;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += MyHandler;

            JSE = new JScriptEngine(WindowsScriptEngineFlags.EnableDebugging | WindowsScriptEngineFlags.EnableJITDebugging);
            JSE.Script.globalThis = JSE.Evaluate("({})");
            JSE.AddHostObject("CSHost", new HostFunctions());
            JSE.AddHostObject("CSExtendedHost", new ExtendedHostFunctions());

            JSE.AddHostType("CSMarkup", typeof(Markup));
            JSE.AddHostType("CSMarkupFormatters", typeof(MarkupFormatters));
            JSE.AddHostType("CSAsana", typeof(Asana));

            /// System
            JSE.AddHostType("CSString", typeof(String));
            JSE.AddHostType("CSEnvironment", typeof(Environment));
            JSE.AddHostType("CSEnvironmentVariableTarget", typeof(System.EnvironmentVariableTarget));
            JSE.AddHostType("CSConsole", typeof(System.Console));
            JSE.AddHostType("CSFile", typeof(File));
            JSE.AddHostType("CSFileInfo", typeof(FileInfo));
            JSE.AddHostType("CSDirectory", typeof(Directory));
            JSE.AddHostType("CSPath", typeof(Path));
            JSE.AddHostType("CSSearchOption", typeof(SearchOption));
            JSE.AddHostType("CSEncoding", typeof(Encoding));
            JSE.AddHostType("CSMemoryStream", typeof(MemoryStream));
            JSE.AddHostType("CSTimeSpan", typeof(TimeSpan));
            JSE.AddHostType("CSThread", typeof(Thread));
            JSE.AddHostType("CSProcess", typeof(Process));
            JSE.AddHostType("CSProcessStartInfo", typeof(ProcessStartInfo));
            JSE.AddHostType("CSSearchOption", typeof(SearchOption));
            JSE.AddHostType("CSUri", typeof(Uri));
            JSE.AddHostType("CSWebClient", typeof(WebClient));
            JSE.AddHostType("CSStreamReader", typeof(StreamReader));
            JSE.AddHostType("CSStream", typeof(Stream));
            JSE.AddHostType("CSBitmap", typeof(Bitmap));
            JSE.AddHostType("CSImageFormat", typeof(ImageFormat));
            JSE.AddHostType("CSDebugger", typeof(Debugger));
            JSE.AddHostType("CSImageCodecInfo", typeof(ImageCodecInfo));
            JSE.AddHostType("CSEncoder", typeof(System.Drawing.Imaging.Encoder));
            JSE.AddHostType("CSEncoderParameter", typeof(EncoderParameter));
            JSE.AddHostType("CSEncoderParameters", typeof(EncoderParameters));
            JSE.AddHostType("CSDateTime", typeof(DateTime));
            JSE.AddHostType("CSDateTimeKind", typeof(DateTimeKind));
            JSE.AddHostType("CSConvert", typeof(System.Convert));
            JSE.AddHostType("CSStringBuilder", typeof(StringBuilder));
            /// Mail
            JSE.AddHostType("CSMailMessage", typeof(MailMessage));
            JSE.AddHostType("CSMailAddress", typeof(System.Net.Mail.MailAddress));
            JSE.AddHostType("CSAttachment", typeof(System.Net.Mail.Attachment));
            JSE.AddHostType("CSNetworkCredential", typeof(NetworkCredential));
            JSE.AddHostType("CSSmtpClient", typeof(SmtpClient));

            /// Chrome
            JSE.AddHostType("CSChromeDriver", typeof(ChromeDriver));
            JSE.AddHostType("CSChromeOptions", typeof(ChromeOptions));
            JSE.AddHostType("CSChromeDriverService", typeof(ChromeDriverService));

            /// Firefox
            //JSE.AddHostType("CSFirefoxBinary", typeof(OpenQA.Selenium.Firefox.FirefoxBinary));
            //JSE.AddHostType("CSFirefoxDriver", typeof(FirefoxDriver));
            //JSE.AddHostType("CSFirefoxProfileManager", typeof(FirefoxProfileManager));
            //JSE.AddHostType("CSFirefoxProfile", typeof(FirefoxProfile));
            //JSE.AddHostType("CSFirefoxDriverCommandExecutor", typeof(FirefoxDriverCommandExecutor));
            //JSE.AddHostType("CSFirefoxOptions", typeof(FirefoxOptions));
            //JSE.AddHostType("CSFirefoxDriverService", typeof(FirefoxDriverService));

            /// PhantomJS
            // JSE.AddHostType("CSPhantomJSDriver", typeof(PhantomJSDriver));
            // JSE.AddHostType("CSPhantomJSOptions", typeof(PhantomJSOptions));
            // JSE.AddHostType("CSPhantomJSDriverService", typeof(PhantomJSDriverService));

            /// Selenium
            JSE.AddHostType("CSBy", typeof(By));
            JSE.AddHostType("CSJavascriptExecutor", typeof(IJavaScriptExecutor));
            JSE.AddHostType("CSActions", typeof(Actions));
            JSE.AddHostType("CSDriverService", typeof(OpenQA.Selenium.DriverService));
            JSE.AddHostType("CSRemoteWebDriver", typeof(RemoteWebDriver));
            JSE.AddHostType("CSExpectedConditions", typeof(SeleniumExtras.WaitHelpers.ExpectedConditions));
            JSE.AddHostType("CSPlatform", typeof(Platform));
            JSE.AddHostType("CSPlatformType", typeof(PlatformType));
            JSE.AddHostType("CSProxy", typeof(Proxy));
            JSE.AddHostType("CSProxyKind", typeof(ProxyKind));
            JSE.AddHostType("CSIWebDriver", typeof(IWebDriver));
            JSE.AddHostType("CSITakesScreenshot", typeof(ITakesScreenshot));
            JSE.AddHostType("CSScreenshot", typeof(Screenshot));
            JSE.AddHostType("CSSelectElement", typeof(SelectElement));
            JSE.AddHostType("CSCookie", typeof(OpenQA.Selenium.Cookie));

            /// HTMLAgilityPack
            JSE.AddHostType("CSHtmlDocument", typeof(HtmlAgilityPack.HtmlDocument));
            JSE.AddHostType("CSHtmlNode", typeof(HtmlAgilityPack.HtmlNode));
            JSE.AddHostType("CSHtmlNodeCollection", typeof(HtmlAgilityPack.HtmlNodeCollection));
            JSE.AddHostType("CSHtmlAttribute", typeof(HtmlAgilityPack.HtmlAttribute));
            JSE.AddHostType(typeof(HapCssExtensionMethods));

            /// Axtension
            JSE.AddHostType("CSApplicationLogging", typeof(ApplicationLogging));
            JSE.AddHostType("CSConfig", typeof(Config));
            JSE.AddHostType("CSSimpleREST", typeof(SimpleREST));
            //JSE.AddHostType("CSRESTful", typeof(RESTful));
            //JSE.AddHostType("CSFluentREST", typeof(RESTful2));
            JSE.AddHostType("CSFluentREST", typeof(RESTful));
            JSE.AddHostType("CSProcesses", typeof(Processes));
            JSE.AddHostType("CSMail", typeof(Mail));
            JSE.AddHostType("CSDatabase", typeof(SQL));
            //JSE.AddHostType("CSTelstraSMS", typeof(TelstraSMSv2));
            JSE.AddHostType("CSAsana", typeof(Asana));
            JSE.AddHostType("CSXML", typeof(XML));
            JSE.AddHostType("CSSQL", typeof(SQL));
            JSE.AddHostType("CSGoogleOAuth2", typeof(OAuth2));
            JSE.AddHostType("CSDebugPoints", typeof(DebugPoints));
            JSE.AddHostType("CSConfiguration", typeof(Configuration.Configuration));
            JSE.AddHostType("CSBlueLog", typeof(BlueLog));
            JSE.AddHostType("CSCrypto", typeof(Crypto));

            // email
            JSE.AddHostObject("CSExtendedIMAP", new ExtendedIMAP());
            JSE.AddHostType("CSImapClient", typeof(S22.Imap.ImapClient));
            JSE.AddHostType("CSAuthMethod", typeof(AuthMethod));
            JSE.AddHostType("CSSearchCondition", typeof(SearchCondition));
            JSE.AddHostType("CSSearchOption", typeof(SearchOption));
            JSE.AddHostType("CSFetchOptions", typeof(FetchOptions));
            JSE.AddHostType("CSMessageFlag", typeof(MessageFlag));
            //JSE.AddHostType("CSPop3Client", typeof(Pop3Client));
            //JSE.AddHostType("CSPop3Message", typeof(OpenPop.Mime.Message));
            JSE.AddHostType("CSSslProtocols", typeof(System.Security.Authentication.SslProtocols));
            //JSE.AddHostType("CSImapXAttachment", typeof(ImapX.Attachment));
            //JSE.AddHostType("CSImapXEnvelope", typeof(ImapX.Envelope));
            //JSE.AddHostType("CSImapXFolder", typeof(ImapX.Folder));
            //JSE.AddHostType("CSImapXMailAddress", typeof(ImapX.MailAddress));
            //JSE.AddHostType("CSImapXMessage", typeof(ImapX.Message));
            //JSE.AddHostType("CSImapXMessageBody", typeof(ImapX.MessageBody));
            //JSE.AddHostType("CSImapXMessageContent", typeof(ImapX.MessageContent));

            //wmi
            //JSE.AddHostType("CSManagementObjectSearcher", typeof(ManagementObjectSearcher));
            //JSE.AddHostType("CSManagementObject", typeof(System.Management.ManagementObject));
            //JSE.AddHostType("CSManagementObjectSearcher", typeof(ManagementObjectSearcher));
            //JSE.AddHostType("CSManagementObjectSearcher", typeof(ManagementObjectSearcher));
            //JSE.AddHostType("CSManagementObjectSearcher", typeof(ManagementObjectSearcher));

            JSE.AddHostType("CSINI", typeof(INI));
            //JSE.AddHostType("CSGoogleAdwords", typeof(AdWords));
            //JSE.AddHostType("CSManagedCustomerTreeNode", typeof(ManagedCustomerTreeNode));
            //JSE.AddHostType("CSAdWordsUser", typeof(AdWordsUser));
            JSE.AddHostType("CSOAuth2Flow", typeof(OAuth2Flow));
            //JSE.AddHostType("CSAdWordsAppConfig", typeof(AdWordsAppConfig));
            //
            JSE.AddHostType("CSZipFile", typeof(ZipFile));

            JSE.AddHostType("CSBrowserControl", typeof(BrowserControl));
        }

        public Tuple<bool, object> Run(string scriptText, Config cfg)
        {
            return Run(scriptText, cfg, new Dictionary<string, object>());
        }

        public Tuple<bool, object> Run(string scriptText, Dictionary<string, object> settings)
        {
            return Run(scriptText, new Config(), settings);
        }

        public Tuple<bool, object> Run(string scriptText)
        {
            return Run(scriptText, new Config(), new Dictionary<string, object>());
        }

        public Tuple<bool, object> Run(string scriptText, Config cfg, Dictionary<string, object> settings)
        {
            bool ok = false;

            JSE.AddHostObject("CSCFG", cfg);
            JSE.AddHostObject("CSSettings", settings);
            //JSE.Execute("function include(s){if (CSFile.Exists(s)) {eval(CSFile.ReadAllText(s));}}");
            //JSE.Execute("function parseJSON(s){return CSFile.Exists(s) ? eval('('+CSFile.ReadAllText(s)+')') : {};}");
            object evalResponse;
            try
            {
                using (JSE)
                {
                    evalResponse = JSE.Evaluate(scriptText);
                    ok = true;
                }
            }
            catch (ScriptEngineException sex)
            {
                evalResponse = String.Format("{0}\r\n{1}\r\n{2}\r\n", sex.ErrorDetails, sex.Message, sex.StackTrace);
                ok = false;
            }
            //if (runOnceOnly)
            //{
            //    JSE.Dispose();
            //}
            return new Tuple<bool, object>(ok, evalResponse);
        }
    }
}
