using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

public class LoginTests
{
    [Fact]
    public void LoginButton_Click_Should_NavigateToHomePage()
    {
        using var driver = new ChromeDriver();
        driver.Navigate().GoToUrl("https://yellow-ocean-0f63e7903.4.azurestaticapps.net/Login"); 

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        
        var emailInput = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("input[placeholder='Email']")));
        emailInput.SendKeys("test@test.com");
        
        var passwordInput = driver.FindElement(By.CssSelector("input[placeholder='Adgangskode']"));
        passwordInput.SendKeys("Password123");
        
        var loginButton = driver.FindElement(By.CssSelector("button.btn-primary"));
        loginButton.Click();
        
        Assert.Contains("/", driver.Url);
    }
}