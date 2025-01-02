using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace OverblikPlus.Frontend.Tests;

public class QRCodeTests
{
    [Fact]
    public void QRCodeScan_Should_NavigateToTaskSteps()
    {
        using var driver = new ChromeDriver();
        driver.Navigate().GoToUrl("https://overblikplus/qrscanner");

        // Simulér scanning af QR-kode
        driver.FindElement(By.Id("qrInput")).SendKeys("task123");
        driver.FindElement(By.Id("scanButton")).Click();

        // Bekræft, at vi blev navigeret til opgavens trin
        Assert.Contains("tasksteps", driver.Url.ToLower());
    }
}