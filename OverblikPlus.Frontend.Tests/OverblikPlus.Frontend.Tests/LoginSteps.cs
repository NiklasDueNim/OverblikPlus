using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using TechTalk.SpecFlow;

namespace OverblikPlus.Frontend.Tests;


[Binding]
public class LoginSteps
{
    private IWebDriver driver;
    private WebDriverWait wait;

    [Given(@"I am on the login page")]
    public void GivenIAmOnTheLoginPage()
    {
        // Start Chrome-driveren og navigér til login-siden
        driver = new ChromeDriver();
        driver.Navigate().GoToUrl("https://yellow-ocean-0f63e7903.4.azurestaticapps.net");
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    [When(@"I enter a valid email and password")]
    public void WhenIEnterAValidEmailAndPassword()
    {
        // Udfyld loginformularen
        driver.FindElement(By.CssSelector("input[placeholder='Email']")).SendKeys("string@hotmail.com");
        driver.FindElement(By.CssSelector("input[placeholder='Adgangskode']")).SendKeys("Test123!");
    }

    [When(@"I click the login button")]
    public void WhenIClickTheLoginButton()
    {
        // Klik på login-knappen
        driver.FindElement(By.CssSelector("button.btn-primary")).Click();
    }

    [Then(@"I should be redirected to the homepage")]
    public void ThenIShouldBeRedirectedToTheHomepage()
    {
        // Vent til vi bliver navigeret til forsiden
        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.UrlContains("/tasks"));

        // Bekræft, at vi er på forsiden
        Assert.Contains("/tasks", driver.Url);

        // Luk browseren
        driver.Quit();
    }
}
