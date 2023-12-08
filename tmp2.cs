using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

        Console.WriteLine($"Open the following URL in your browser and complete the authentication: {authorizationUrl}");

        // Step 3: Retrieve the authorization code from the user
        Console.Write("Enter the authorization code: ");
        string authorizationCode = Console.ReadLine();

        // Step 4: Exchange the authorization code for an access token
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
