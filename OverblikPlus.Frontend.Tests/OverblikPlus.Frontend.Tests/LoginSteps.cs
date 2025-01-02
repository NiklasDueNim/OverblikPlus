// using OpenQA.Selenium;
// using OpenQA.Selenium.Chrome;
// using OpenQA.Selenium.Support.UI;
// using SeleniumExtras.WaitHelpers;
// using TechTalk.SpecFlow;
// using Xunit;
//
// namespace OverblikPlus.Frontend.Tests;
//
// [Binding]
// public class LoginSteps
// {
//     private IWebDriver driver;
//     private WebDriverWait wait;
//
//     public LoginSteps()
//     {
//         var options = new ChromeOptions();
//         options.AddArgument("--headless");
//         options.AddArgument("--no-sandbox");
//         options.AddArgument("--disable-dev-shm-usage");
//         driver = new ChromeDriver(options);
//         wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));
//     }
//
//     [Given(@"I am on the login page")]
//     public void GivenIAmOnTheLoginPage()
//     {
//         driver.Navigate().GoToUrl("https://yellow-ocean-0f63e7903.4.azurestaticapps.net/Login");
//         wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[placeholder='Email']")));
//     }
//
//     [When(@"I enter a valid email and password")]
//     public void WhenIEnterAValidEmailAndPassword()
//     {
//         driver.FindElement(By.CssSelector("input[placeholder='Email']")).SendKeys("string@hotmail.com");
//         driver.FindElement(By.CssSelector("input[placeholder='Adgangskode']")).SendKeys("Test123!");
//     }
//
//     [When(@"I click the login button")]
//     public void WhenIClickTheLoginButton()
//     {
//         driver.FindElement(By.CssSelector("button.btn-primary")).Click();
//     }
//
//     [Then(@"I should be redirected to the homepage")]
//     public void ThenIShouldBeRedirectedToTheHomepage()
//     {
//         wait.Until(ExpectedConditions.UrlContains("/"));
//         Assert.Contains("/", driver.Url);
//     }
//
//     [AfterScenario]
//     public void TearDown()
//     {
//         driver.Quit();
//     }
// }