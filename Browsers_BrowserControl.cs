using OpenQA.Selenium;

using QuarterMaster.Debugging;
using QuarterMaster.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace QuarterMaster.Browsers
{
    public static class BrowserControl
    {
        internal static ApplicationLogging AL;

        internal static object Locker = new object();

        public static void TakeScreenshot(IWebDriver driver, string fileSpec)
        {
            AL?.Module(MethodBase.GetCurrentMethod().Name);
            // 1. Make screenshot of all screen
            try
            {
                Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
                lock (Locker)
                {
                    ss.SaveAsFile(Path.ChangeExtension(fileSpec, ".png"));
                }
            }
            catch (Exception e)
            {
                AL?.Warn("Failure in TakeScreenshot!", "\n" + e.Message, "\n" + e.StackTrace);
            }
            AL?.Module();
            return;
        }

        public static void CookieEnabledFileDownload(IWebDriver driver, string url, string localPath)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }

            AL?.Module(MethodBase.GetCurrentMethod().Name);
            var client = new WebClient();
            client.Headers[HttpRequestHeader.Cookie] = GetCookieString(driver);
            client.DownloadFile(url, localPath);
            AL?.Inform("Downloading from", url, "to", localPath);
            AL?.Module();
        }

        public static string CookieEnabledSiteCheck(IWebDriver driver, string url)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            string result;
            AL?.Module(MethodBase.GetCurrentMethod().Name);
            var client = new WebClient();

            var cookieString = GetCookieString(driver);
            AL?.Inform("Cookie String:", cookieString);

            if (cookieString != null)
            {
                client.Headers[HttpRequestHeader.Cookie] = cookieString;
            }

            AL?.Inform("Checking", url);

            try
            {
                AL?.Inform("Found", url);
                result = client.DownloadString(url);
            }
            catch (Exception E)
            {
                AL?.Warn("Failed to find", url, E.Message);
                result = string.Empty;
            }
            AL?.Module();
            return result;
        }

        public static string GetCookieString(IWebDriver driver)
        {
            var cookies = driver.Manage().Cookies.AllCookies;
            if (cookies.Count == 0)
            {
                return string.Empty;
            }
            else
            {
                return string.Join("; ", cookies.Select(c => string.Format("{0}={1}", c.Name, c.Value)));
            }
        }

        public static void TakeFullScreenshot(IWebDriver driver, string fileSpec)
        {
            lock (Locker)
            {
                GetEntireScreenshot(driver).Save(Path.ChangeExtension(fileSpec, ".png"), ImageFormat.Png);
            }
        }

        // https://stackoverflow.com/questions/16795458/selenium-webdriver-c-sharp-full-website-screenshots-with-chromedriver-and-firefo
        public static Image GetEntireScreenshot(IWebDriver driver)
        {
            // Go to top of screen
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 0)");
            // Get the total size of the page
            var totalWidth = (int)(long)((IJavaScriptExecutor)driver).ExecuteScript("return document.body.offsetWidth"); //documentElement.scrollWidth");
            var totalHeight = (int)(long)((IJavaScriptExecutor)driver).ExecuteScript("return  document.body.parentNode.scrollHeight");
            // Get the size of the viewport
            var viewportWidth = (int)(long)((IJavaScriptExecutor)driver).ExecuteScript("return document.body.clientWidth"); //documentElement.scrollWidth");
            var viewportHeight = (int)(long)((IJavaScriptExecutor)driver).ExecuteScript("return window.innerHeight"); //documentElement.scrollWidth");

            // We only care about taking multiple images together if it doesn't already fit
            if (totalWidth <= viewportWidth && totalHeight <= viewportHeight)
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                return ScreenshotToImage(screenshot);
            }
            // Split the screen in multiple Rectangles
            var rectangles = new List<Rectangle>();
            // Loop until the totalHeight is reached
            for (var y = 0; y < totalHeight; y += viewportHeight)
            {
                var newHeight = viewportHeight;
                // Fix if the height of the element is too big
                if (y + viewportHeight > totalHeight)
                {
                    newHeight = totalHeight - y;
                }
                // Loop until the totalWidth is reached
                for (var x = 0; x < totalWidth; x += viewportWidth)
                {
                    var newWidth = viewportWidth;
                    // Fix if the Width of the Element is too big
                    if (x + viewportWidth > totalWidth)
                    {
                        newWidth = totalWidth - x;
                    }
                    // Create and add the Rectangle
                    var currRect = new Rectangle(x, y, newWidth, newHeight);
                    rectangles.Add(currRect);
                }
            }
            // Build the Image
            var stitchedImage = new Bitmap(totalWidth, totalHeight);
            // Get all Screenshots and stitch them together
            var previous = Rectangle.Empty;
            foreach (var rectangle in rectangles)
            {
                // Calculate the scrolling (if needed)
                if (previous != Rectangle.Empty)
                {
                    var xDiff = rectangle.Right - previous.Right;
                    var yDiff = rectangle.Bottom - previous.Bottom;
                    // Scroll
                    ((IJavaScriptExecutor)driver).ExecuteScript(String.Format("window.scrollBy({0}, {1})", xDiff, yDiff));
                }
                // Take Screenshot
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                // Build an Image out of the Screenshot
                var screenshotImage = ScreenshotToImage(screenshot);
                // Calculate the source Rectangle
                var sourceRectangle = new Rectangle(viewportWidth - rectangle.Width, viewportHeight - rectangle.Height, rectangle.Width, rectangle.Height);
                // Copy the Image
                using (var graphics = Graphics.FromImage(stitchedImage))
                {
                    graphics.DrawImage(screenshotImage, rectangle, sourceRectangle, GraphicsUnit.Pixel);
                }
                // Set the Previous Rectangle
                previous = rectangle;
            }
            return stitchedImage;
        }

        private static Image ScreenshotToImage(Screenshot screenshot)
        {
            Image screenshotImage;
            using (var memStream = new MemoryStream(screenshot.AsByteArray))
            {
                screenshotImage = Image.FromStream(memStream);
            }
            return screenshotImage;
        }

        public static void RegisterApplicationLogging(ref ApplicationLogging ptr)
        {
            AL = ptr;
        }
    }
}
