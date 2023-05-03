using RestSharp;
using RestSharp.Authenticators;
using System.Text.Json;

namespace HomemadeLMS.Services
{
    public class AuthCertificate
    {
        public const int LifeTimeSeconds = 3600;

        public AuthCertificate(string emailAddress, string token)
        {
            EmailAddress = emailAddress;
            Token = token;
            CreationTime = DateTime.UtcNow;
        }

        public string EmailAddress { get; private set; }
        public string Token { get; private set; }
        public DateTime CreationTime { get; private set; }

        public bool IsExpired => ExpirationTime < DateTime.UtcNow;
        public DateTime ExpirationTime => CreationTime.AddSeconds(LifeTimeSeconds);
    }

	public class MailingService
    {
        public const int RequestTimeoutMilliseconds = 30 * 1000;    // 30 seconds

        private readonly object syncRoot = new();
        private readonly Random randomGenerator;
        private readonly List<AuthCertificate> certificates = new();

        public MailingService()
        {
            randomGenerator = new Random(DateTime.UtcNow.Millisecond);
        }

        public async Task CreateRequest(string emailAddress)
        {
            string token;
            lock (syncRoot)
            {
                token = GenerateToken();
                certificates.Add(new(emailAddress, token));
            }
            await SendMail(emailAddress, token);
        }

        public string? GetEmailAddress(string? token)
        {
            if (token is null)
            {
                return null;
            }
            lock (syncRoot)
            {
                for (int index = certificates.Count - 1; index >= 0; index--)
                {
                    var certificate = certificates[index];
                    if (certificate.IsExpired)
                    {
                        certificates.RemoveAt(index);
                    }
                    if (certificate.Token == token)
                    {
                        certificates.RemoveAt(index);
                        return certificate.EmailAddress;
                    }
                }
            }
            return null;
        }

        private string GenerateToken()
        {
            var bytes = new byte[32];
            randomGenerator.NextBytes(bytes);
            return Convert.ToHexString(bytes);
        }

        private static async Task SendMail(string emailAddress, string token)
        {
            var apiDomain = Program.AppConfig.ServiceConfig.MailingServiceConfig.ApiDomain;
            var apiKey = Program.AppConfig.ServiceConfig.MailingServiceConfig.ApiKey;
            var selfUrlBase = Program.AppConfig.ServiceConfig.SelfBaseUrl;

            var resource = $"https://api.mailgun.net/v3/{apiDomain}/messages";
            var url = $"{selfUrlBase}/signin/confirm?token={token}";
            var variableData = new Dictionary<string, string>()
            {
                ["url"] = url
            };
            var serializedVariableData = JsonSerializer.Serialize(variableData);

            var clientOptions = new RestClientOptions()
            {
                Authenticator = new HttpBasicAuthenticator("api", apiKey)
            };
            var client = new RestClient(clientOptions);
            var request = new RestRequest(resource)
            {
                Method = Method.Post,
                Timeout = RequestTimeoutMilliseconds,
            };
            request.AddParameter("from", $"HomemadeLMS <postmaster@{apiDomain}>");
            request.AddParameter("to", emailAddress);
            request.AddParameter("subject", "Подтверждение авторизации");
            request.AddParameter("template", "confirmsignin");
            request.AddParameter("h:X-Mailgun-Variables", serializedVariableData);

            var result = await client.ExecuteAsync(request);
            if (!result.IsSuccessful)
            {
                throw new NotSupportedException("Mail sending is failed.");
            }
        }
    }
}