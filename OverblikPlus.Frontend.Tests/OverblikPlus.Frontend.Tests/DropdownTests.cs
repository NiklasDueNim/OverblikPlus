using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace OverblikPlus.Frontend.Tests;

public class DropdownTests
{
    [Fact]
    public void Dropdown_Should_ClickAndSelectOption()
    {
        using var driver = new ChromeDriver();
        driver.Navigate().GoToUrl("https://yellow-ocean-0f63e7903.4.azurestaticapps.net");
        
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        
        var dropdownButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("dropdownMenuButton")));
        dropdownButton.Click();
        
        var dropdownOption = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//a[contains(text(), 'Profil')]")));
        dropdownOption.Click();
        
        Assert.Contains("/user/", driver.Url);
    }
}