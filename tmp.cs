/*
  <ItemGroup>
    <PackageReference Include="Selenium.WebDriver" Version="4.16.1" />
    <PackageReference Include="Selenium.WebDriver.ChromeDriver" Version="120.0.6099.7100" />
  </ItemGroup>
*/

using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

class Program
{
    static void Main()
    {
        // Set the path to the ChromeDriver executable
        string chromeDriverPath = Path.Combine(AppContext.BaseDirectory, "chromedriver");

        // Initialize ChromeDriver
        using (var driver = new ChromeDriver(chromeDriverPath))
        {
            // Navigate to the Azure AD-protected website
            driver.Navigate().GoToUrl("https://something");

            // Wait for the password input field to be visible
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.FindElement(By.Id("i0116")).Displayed);

            // Assuming Azure AD login page is opened, you need to fill in the username and password
            IWebElement usernameInput = driver.FindElement(By.Id("i0116"));
            usernameInput.SendKeys("your-username");

            IWebElement nextButton = driver.FindElement(By.Id("idSIButton9"));
            nextButton.Click();

            // Wait for the password input field to be visible
            wait.Until(d => d.FindElement(By.Id("i0118")).Displayed);

            IWebElement passwordInput = driver.FindElement(By.Id("i0118"));
            passwordInput.SendKeys("your-password");

            IWebElement signInButton = driver.FindElement(By.Id("idSIButton9"));
            signInButton.Click();

            // Wait for the login to complete and the website to load
            wait.Until(d => d.Url.Contains("your-website-homepage"));

            // Extract the token from local storage
            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
            string accessToken = (string)jsExecutor.ExecuteScript("return localStorage.getItem('your-access-token-key');");

            Console.WriteLine($"Access Token: {accessToken}");
        }
    }
}
