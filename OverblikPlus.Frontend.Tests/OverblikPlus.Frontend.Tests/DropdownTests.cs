using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace OverblikPlus.Frontend.Tests;

public class DropdownTests
{
    [Fact]
    public void Dropdown_Should_SelectOption()
    {
        using var driver = new ChromeDriver();
        driver.Navigate().GoToUrl("https://localhost:5226");

        // Find dropdown-menu
        var dropdown = new SelectElement(driver.FindElement(By.Id("taskDropdown")));
        dropdown.SelectByValue("option1"); // Vælg en værdi i dropdown

        // Bekræft, at den valgte værdi er korrekt
        Assert.Equal("option1", dropdown.SelectedOption.GetAttribute("value"));
    }
}