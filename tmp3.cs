using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

class Program
{
    private const string AuthorizationEndpoint = "YOUR_AUTHORIZATION_ENDPOINT";
    private const string TokenEndpoint = "YOUR_TOKEN_ENDPOINT";
    private const string ClientId = "YOUR_CLIENT_ID";
    private const string RedirectUri = "YOUR_REDIRECT_URI";
    
    static async Task Main(string[] args)
    {
        // Step 1: Generate PKCE code verifier and code challenge
        string codeVerifier = GenerateRandomBase64Url(32);
        string codeChallenge = GenerateCodeChallenge(codeVerifier);

        // Step 2: Construct the authorization URL
        string authorizationUrl = $"{AuthorizationEndpoint}?client_id={ClientId}&response_type=code&redirect_uri={RedirectUri}&code_challenge={codeChallenge}&code_challenge_method=S256";

        // Step 3: Open browser and perform authentication
        string authorizationCode = await PerformAuthentication(authorizationUrl);

        // Step 4: Exchange the authorization code for an access token
        if (!string.IsNullOrEmpty(authorizationCode))
        {
            using (HttpClient client = new HttpClient())
            {
                var tokenRequestContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"client_id", ClientId},
                    {"code", authorizationCode},
                    {"redirect_uri", RedirectUri},
                    {"code_verifier", codeVerifier},
                    {"grant_type", "authorization_code"}
                });

                HttpResponseMessage tokenResponse = await client.PostAsync(TokenEndpoint, tokenRequestContent);

                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenResponseData = await tokenResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Token response: {tokenResponseData}");
                }
                else
                {
                    Console.WriteLine($"Token request failed: {tokenResponse.ReasonPhrase}");
                }
            }
        }
        else
        {
            Console.WriteLine("Authorization code retrieval failed.");
        }
    }

    private static async Task<string> PerformAuthentication(string authorizationUrl)
    {
        using (IWebDriver driver = new ChromeDriver())
        {
            // Open the browser and navigate to the authorization URL
            driver.Navigate().GoToUrl(authorizationUrl);

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

            // Wait for the redirect to happen and extract the authorization code from the URL
            wait.Until(d => d.Url.StartsWith(RedirectUri));

            var uri = new Uri(driver.Url);
            var authorizationCode = uri.Query.Split('&')[0].Split('=')[1];

            return authorizationCode;
        }
    }

    private static string GenerateRandomBase64Url(int length)
    {
        byte[] buffer = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(buffer);
        }
        return Base64UrlEncode(buffer);
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Base64UrlEncode(challengeBytes);
        }
    }
}
