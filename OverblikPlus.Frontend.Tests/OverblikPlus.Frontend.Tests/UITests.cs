using OpenQA.Selenium.Chrome;

namespace OverblikPlus.Frontend.Tests;

public class UITests
{
    [Fact]
    public void HomePage_Title_Should_Be_Correct()
    {
        using var driver = new ChromeDriver();
        driver.Navigate().GoToUrl("https://overblikplus.dk");

        Assert.Contains("OverblikPlus", driver.Title);
    }
}
