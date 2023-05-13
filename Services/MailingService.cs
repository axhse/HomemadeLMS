using HomemadeLMS.Environment;
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
        private readonly object syncRoot = new();
        private readonly Random randomGenerator;
        private readonly List<AuthCertificate> certificates = new();
        private readonly int timeoutInSeconds;
        private readonly string apiKey;
        private readonly string domainName;
        private readonly string serviceEmailAddress;

        public MailingService(ConfigurationComponent mailerConfiguration,
                              ConfigurationComponent hostConfiguration)
        {
            timeoutInSeconds = mailerConfiguration.GetInt(PropertyName.TimeoutInSeconds);
            serviceEmailAddress = mailerConfiguration.GetString(PropertyName.ServiceEmailAddress);
            domainName = hostConfiguration.GetString(PropertyName.DomainName);
            apiKey = Program.SecretManager.Get(SecretName.MailingApiKey);
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

        private async Task SendMail(string emailAddress, string token)
        {
            var resource = $"https://api.mailgun.net/v3/{serviceEmailAddress}/messages";
            var url = $"{domainName}/signin/confirm?token={token}";
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
                Timeout = 1000 * timeoutInSeconds,
            };
            request.AddParameter("from", $"HomemadeLMS <postmaster@{serviceEmailAddress}>");
            request.AddParameter("to", emailAddress);
            request.AddParameter("subject", "Подтверждение авторизации");
            request.AddParameter("template", "confirmsignin");
            request.AddParameter("h:X-Mailgun-Variables", serializedVariableData);

            var response = await client.ExecuteAsync(request);
            response.ThrowIfError();
            if (!response.IsSuccessStatusCode)
            {
                throw new NotSupportedException(
                    $"Request failure: {response.StatusCode} {response.StatusDescription}."
                );
            }
        }
    }
}