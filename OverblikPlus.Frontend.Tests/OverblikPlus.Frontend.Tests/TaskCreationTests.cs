// using OpenQA.Selenium;
// using OpenQA.Selenium.Chrome;
//
// namespace OverblikPlus.Frontend.Tests;
//
// public class TaskCreationTests
// {
//     [Fact]
//     public void CreateTask_Should_DisplaySuccessMessage()
//     {
//         using var driver = new ChromeDriver();
//         driver.Navigate().GoToUrl("https://overblikplus.dk");
//
//         // Udfyld formular
//         driver.FindElement(By.Id("taskTitle")).SendKeys("Ny opgave");
//         driver.FindElement(By.Id("taskDescription")).SendKeys("Dette er en testopgave.");
//
//         // Klik på 'Opret opgave'
//         driver.FindElement(By.Id("createTaskButton")).Click();
//
//         // Bekræft succesmeddelelse
//         var successMessage = driver.FindElement(By.Id("successMessage"));
//         Assert.Contains("Opgave oprettet", successMessage.Text);
//     }
// }
