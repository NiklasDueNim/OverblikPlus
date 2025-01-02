// using OpenQA.Selenium;
// using OpenQA.Selenium.Chrome;
//
// namespace OverblikPlus.Frontend.Tests;
//
// public class InputFieldTests
// {
//     [Fact]
//     public void InputField_Should_AcceptText_And_DisplayResult()
//     {
//         using var driver = new ChromeDriver();
//         driver.Navigate().GoToUrl("https://overblikplus.dk");
//
//         // Find inputfelt og indtast tekst
//         var inputField = driver.FindElement(By.Id("usernameInput")); // Ændr ID til dit felt
//         inputField.SendKeys("TestUser");
//
//         // Klik på en knap
//         var submitButton = driver.FindElement(By.Id("submitButton"));
//         submitButton.Click();
//
//         // Bekræft, at teksten vises et sted på siden
//         var result = driver.FindElement(By.Id("resultText")); // Ændr ID til dit resultatfelt
//         Assert.Contains("TestUser", result.Text);
//     }
// }