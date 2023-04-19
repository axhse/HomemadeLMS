using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace HomemadeLMS.Models.Domain
{
    public enum UserRole
    {
        None,
        Student,
        Teacher,
        Manager,
    }

    public class Account
    {
        private const string EmailAddressBase = "@edu.hse.ru";
        private string username = string.Empty;

        public Account()
        { }

        /// <exception cref="ArgumentException"></exception>
        public Account(string accountId, string password)
        {
            SetPassword(password);
            SetAccountId(accountId);
        }

        public static string CalculateHash(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] offsets = new byte[] { 103, 13, 209, 60, 17, 53, 2, 41 };
            for (int i = offsets.Length; i >= 1; i--)
            {
                for (int j = 0; j < bytes.Length; j++)
                {
                    bytes[j] += offsets[j % i];
                }
                bytes = SHA256.HashData(bytes);
            }
            return Convert.ToHexString(bytes);
        }

        public static bool HasPasswordValidFormat(string? password)
            => password is not null && 8 <= password.Length && password.Length <= 50;

        public static bool HasUsernameValidFormat(string? username)
            => username is not null && Regex.IsMatch(username, $"^[A-Za-z0-9_.]{{1,100}}$");

        private static string GetUsername(string accountId)
        {
            if (accountId is null)
            {
                return string.Empty;
            }
            accountId = accountId.Trim(' ').ToLower();
            if (accountId.EndsWith(EmailAddressBase))
            {
                return accountId[..^EmailAddressBase.Length];
            }
            return accountId;
        }

        public string? HeadUsername { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.None;

        public string Username
        {
            get => username;
            set
            {
                if (!HasUsernameValidFormat(value))
                {
                    throw new ArgumentException("Invalid username format.");
                }
                username = value;
            }
        }

        public string EmailAddress => Username + EmailAddressBase;

        /// <exception cref="ArgumentException"></exception>
        public void SetPassword(string password)
        {
            if (!HasPasswordValidFormat(password))
            {
                throw new ArgumentException("Invalid username format.");
            }
            PasswordHash = CalculateHash(password);
        }

        /// <exception cref="ArgumentException"></exception>
        public void SetAccountId(string accountId)
        {
            Username = GetUsername(accountId);
        }

        public bool CanChangeRole(Account other)
        {
            return Username != other.Username && Role == UserRole.Manager && (
                other.Role != UserRole.Manager || other.HeadUsername == Username
            );
        }
    }
}