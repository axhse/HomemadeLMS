using HomemadeLMS.Environment;
using RestSharp;
using RestSharp.Authenticators;
using System.Text.Json;

namespace HomemadeLMS.Services
{
    public class MailingService
    {
        private readonly int apiTimeoutInSeconds;
        private readonly string apiEmailAddress;
        private readonly string apiKey;

        public MailingService(ConfigurationComponent mailerConfiguration)
        {
            apiTimeoutInSeconds = mailerConfiguration.GetInt(PropertyName.ApiTimeoutInSeconds);
            apiEmailAddress = mailerConfiguration.GetString(PropertyName.ApiEmailAddress);
            apiKey = Program.SecretManager.Get(SecretName.MailingApiKey);
        }

        public async Task SendConfirmationMail(string emailAddress, string confirmationUrl)
        {
            var resource = $"https://api.mailgun.net/v3/{apiEmailAddress}/messages";
            var variableData = new Dictionary<string, string>()
            {
                ["url"] = confirmationUrl
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
                Timeout = 1000 * apiTimeoutInSeconds,
            };
            request.AddParameter("from", $"HomemadeLMS <postmaster@{apiEmailAddress}>");
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