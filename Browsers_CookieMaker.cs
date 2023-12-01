using OpenQA.Selenium;

using QuarterMaster.Logging;

using System;

namespace QuarterMaster.Browsers
{
    public class CookieMaker
    {
        internal static ApplicationLogging AL;

        private static ICookieJar cookieJar;

        public void OpenCookieJar(ICookieJar jar)
        {
            cookieJar = jar;
        }

        public void TossIn(string Name, string Value, string Domain, string Path, string Expiry)
        {
            DateTime _expiry = new DateTime(1970, 1, 1) + new TimeSpan(long.Parse(Expiry) * 10000);
            Cookie cky = new Cookie(Name, Value, Domain, Path, _expiry);
            cookieJar.AddCookie(cky);
        }

        public void TossIn(ICookieJar Jar, string Name, string Value, string Domain, string Path, string Expiry)
        {
            DateTime _expiry = new DateTime(1970, 1, 1) + new TimeSpan(long.Parse(Expiry) * 10000);
            Cookie cky = new Cookie(Name, Value, Domain, Path, _expiry);
            try
            {
                Jar.AddCookie(cky);
            }
            catch (Exception exc)
            {
                AL?.Fail(exc.Message, cky);
            }
        }

        public static void RegisterApplicationLogging(ref ApplicationLogging ptr)
        {
            AL = ptr;
        }
    }
}
