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

        public MailingService()
        {
            randomGenerator = new Random(DateTime.UtcNow.Millisecond);
        }

        public void CreateRequest(string emailAddress)
        {
            lock (syncRoot)
            {
                var token = GenerateToken();
                token = emailAddress;   // TODO: remove
                certificates.Add(new(emailAddress, token));
                SendMail(emailAddress, token);
            }
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

        private void SendMail(string emailAddress, string token)
        {
            // TODO
        }
    }
}