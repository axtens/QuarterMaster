using OpenQA.Selenium;

namespace QuarterMaster.Browsers
{
    public class Alert
    {
        public IAlert WaitGetAlert(IWebDriver driver, int waitTimeInSeconds = 5)
        {
            return driver.WaitGetAlert(waitTimeInSeconds);
        }
    }
}
