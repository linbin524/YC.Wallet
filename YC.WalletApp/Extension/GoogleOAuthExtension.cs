using System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;
using YC.WalletApp.Domain.PartViewControl;

namespace YC.WalletApp.Extension
{
   public  class GoogleOAuthExtension
    {
        private const string ClientId = "1005493756913-2pdfnu0m9cm8t5u0c5sla9g1tdm912jn.apps.googleusercontent.com";
        private const string ClientSecret = "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";
        public GoogleAuthService _authService;
        public GoogleOAuthExtension() {
            InitializeAuthService();
        }
        public void InitializeAuthService()
        {
            var clientId = SecurityConfig.CreateSecureString(ClientId);
            var clientSecret = SecurityConfig.CreateSecureString(ClientSecret);

            _authService = new GoogleAuthService(clientId, clientSecret)
            {
                RedirectUri = "http://localhost:8080/",
                Scopes = new[] { "openid", "profile", "email" }
            };

            
            //_authService.LoginSuccess += user => Dispatcher.Invoke(() =>
            //    tempUserInfo = $"Welcome {user.name}\nEmail: {user.email}");

            //_authService.LoginFailed += ex => Dispatcher.Invoke(() =>
            //    MessageBox.Show($"Error: {ex.Message}"));

            //_authService.LoginCanceled += () => Dispatcher.Invoke(() =>
            //    MessageBox.Show("Login canceled"));
        }

        public void DisposeAuthService() {
            _authService.Dispose();
        }
        
    }

    // GoogleAuthService.cs

    public sealed class GoogleAuthService : IDisposable
    {
        private const string AuthEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
        private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
        private const string RevokeEndpoint = "https://oauth2.googleapis.com/revoke";

        private readonly SecureString _clientId;
        private readonly SecureString _clientSecret;
        private readonly HttpClient _httpClient;
        private string _codeVerifier;

        public string RedirectUri { get; set; } = "http://localhost:8080/";
        public string[] Scopes { get; set; } = { "openid", "profile", "email" };
        public TokenResponse CurrentTokens { get; private set; }

        public event Action<UserInfo> LoginSuccess;
        public event Action<Exception> LoginFailed;
        public event Action LoginCanceled;

        public GoogleAuthService(SecureString clientId, SecureString clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;

            _httpClient = new HttpClient(new HttpClientHandler
            {
                UseProxy = false,
                //ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) =>
                //    cert.GetCertHashString() == "GOOGLE_CERT_HASH" // 替换实际证书哈希
            });
        }

        public void StartLogin()
        {
            try
            {
                _codeVerifier = GenerateCodeVerifier();
                var codeChallenge = GenerateCodeChallenge(_codeVerifier);

                var authUrl = BuildAuthUrl(codeChallenge);
                // 使用系统默认浏览器打开
                Process.Start(new ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                });

                // 异步监听回调
                _ = ListenForRedirectAsync();
                //AuthBrowserWindow.Show(authUrl, HandleRedirect);
            }
            catch (Exception ex)
            {
                LoginFailed?.Invoke(ex);
            }
        }
        private async Task ListenForRedirectAsync()
        {
            try
            {
                var authCode = await ListenForAuthCode();
                CurrentTokens = await ExchangeCodeForToken(authCode);
                var userInfo = await GetUserInfo();
                LoginSuccess?.Invoke(userInfo);
            }
            catch (Exception ex)
            {
                LoginFailed?.Invoke(ex);
            }
        }

        private async Task<string> ListenForAuthCode()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(RedirectUri);
            listener.Start();

            var timeoutTask = Task.Delay(TimeSpan.FromMinutes(2));
            var contextTask = listener.GetContextAsync();

            var completedTask = await Task.WhenAny(contextTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                listener.Stop();
                throw new TimeoutException("登录超时");
            }

            var context = await contextTask;
            var code = context.Request.QueryString["code"];
            var error = context.Request.QueryString["error"];

            // 返回响应给浏览器
            var response = context.Response;
            var responseString = "<html><body>登录成功，可以关闭此窗口</body></html>";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
            listener.Stop();

            if (!string.IsNullOrEmpty(error))
                throw new AuthException($"登录失败: {error}");

            return code;
        }

        public async Task RevokeTokenAsync()
        {
            if (CurrentTokens?.access_token == null) return;

            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("token", CurrentTokens.access_token)
        });

            var response = await _httpClient.PostAsync(RevokeEndpoint, content);
            response.EnsureSuccessStatusCode();

            CurrentTokens = null;
        }

        private string BuildAuthUrl(string codeChallenge)
        {
            return $"{AuthEndpoint}?" +
                   $"client_id={SecurityConfig.SecureStringToString(_clientId)}&" +
                   $"redirect_uri={WebUtility.UrlEncode(RedirectUri)}&" +
                   $"response_type=code&" +
                   $"scope={WebUtility.UrlEncode(string.Join(" ", Scopes))}&" +
                   $"code_challenge={codeChallenge}&" +
                   $"code_challenge_method=S256";
        }

        private async void HandleRedirect(Uri uri)
        {
            try
            {
                var queryParams = ParseQueryParameters(uri);

                if (queryParams.TryGetValue("error", out var error))
                    throw new AuthException($"Authorization failed: {error}");

                if (!queryParams.TryGetValue("code", out var code))
                {
                    LoginCanceled?.Invoke();
                    return;
                }

                CurrentTokens = await ExchangeCodeForToken(code);
                var userInfo = await GetUserInfo();
                LoginSuccess?.Invoke(userInfo);
            }
            catch (Exception ex)
            {
                LoginFailed?.Invoke(ex);
            }
        }

        private async Task<TokenResponse> ExchangeCodeForToken(string code)
        {
            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("client_id", SecurityConfig.SecureStringToString(_clientId)),
            new KeyValuePair<string, string>("client_secret", SecurityConfig.SecureStringToString(_clientSecret)),
            new KeyValuePair<string, string>("redirect_uri", RedirectUri),
            new KeyValuePair<string, string>("code_verifier", _codeVerifier),
            new KeyValuePair<string, string>("grant_type", "authorization_code")
        });

            var response = await _httpClient.PostAsync(TokenEndpoint, content);
            response.EnsureSuccessStatusCode();

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(
                await response.Content.ReadAsStringAsync());

            tokenResponse.expires_at = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in);
            return tokenResponse;
        }

        private async Task<UserInfo> GetUserInfo()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, UserInfoEndpoint);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", CurrentTokens.access_token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<UserInfo>(
                await response.Content.ReadAsStringAsync());
        }

        private string GenerateCodeVerifier()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[64];
            rng.GetBytes(bytes);
            return Base64UrlEncode(bytes);
        }

        private string GenerateCodeChallenge(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            byte[] challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Base64UrlEncode(challengeBytes);
        }

        private string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        private Dictionary<string, string> ParseQueryParameters(Uri uri)
        {
            var parameters = new Dictionary<string, string>(StringComparer.Ordinal);
            var query = uri.Query.TrimStart('?').Split('&');

            foreach (var param in query)
            {
                var parts = param.Split('=');
                if (parts.Length == 2)
                {
                    parameters[WebUtility.UrlDecode(parts[0])] =
                        WebUtility.UrlDecode(parts[1]);
                }
            }
            return parameters;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _clientId?.Dispose();
            _clientSecret?.Dispose();
        }
    }

    public class TokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public DateTime expires_at { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
        public string token_type { get; set; }
    }

    public class UserInfo
    {
        public string sub { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string picture { get; set; }
    }

    public class AuthException : Exception
    {
        public AuthException(string message) : base(message) { }
    }

    public static class SecurityConfig
    {
        public static SecureString CreateSecureString(string input)
        {
            var secure = new SecureString();
            foreach (char c in input)
                secure.AppendChar(c);
            secure.MakeReadOnly();
            return secure;
        }

        public static string SecureStringToString(SecureString secureString)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.SecureStringToBSTR(secureString);
                return Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(ptr);
            }
        }

        public static byte[] ProtectData(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            return ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        }

        public static string UnprotectData(byte[] protectedData)
        {
            byte[] bytes = ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }
    }


}
