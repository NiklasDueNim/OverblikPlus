using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace OverblikPlus.Frontend.Tests;

public class ValidationTests
{
    [Fact]
    public void EmptyInput_Should_ShowValidationError()
    {
        using var driver = new ChromeDriver();
        driver.Navigate().GoToUrl("https://localhost:5226/tasks");

        // Klik på 'Opret opgave' uden at udfylde felter
        driver.FindElement(By.Id("createTaskButton")).Click();

        // Bekræft fejlmeddelelse
        var errorMessage = driver.FindElement(By.Id("errorMessage"));
        Assert.Contains("Felterne må ikke være tomme", errorMessage.Text);
    }
}