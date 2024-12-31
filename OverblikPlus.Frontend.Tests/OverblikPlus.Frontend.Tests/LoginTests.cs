using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class LoginTests
{
    [Fact]
    public void LoginButton_Click_Should_NavigateToHomePage()
    {
        using var driver = new ChromeDriver();
        driver.Navigate().GoToUrl("https://overblikplus.dk");

        // Find inputfelter og udfyld dem
        driver.FindElement(By.CssSelector("input[placeholder='Email']")).SendKeys("string@hotmail.com.com");
        driver.FindElement(By.CssSelector("input[placeholder='Adgangskode']")).SendKeys("Test123!");

        // Find og klik på login-knappen
        var loginButton = driver.FindElement(By.Id("loginButton"));
        loginButton.Click();

        // Bekræft, at URL'en er ændret til forsiden
        Assert.Contains("/", driver.Url);
    }
}