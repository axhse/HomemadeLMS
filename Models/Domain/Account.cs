using System.Text.RegularExpressions;

namespace HomemadeLMS.Models.Domain
{
    public enum UserRole
    {
        None,
        Student,
        Teacher,
        Manager,
        Administrator,
    }

    public class Account
    {
        private const string EmailAddressBase = "@edu.hse.ru";
        private string passwordHash = string.Empty;
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
            return text;
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

        public UserRole Role { get; set; } = UserRole.None;

        public string PasswordHash
        {
            get => passwordHash;
            set
            {
                if (!HasPasswordValidFormat(value))
                {
                    throw new ArgumentException("Invalid password format.");
                }
                passwordHash = value;
            }
        }

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
            PasswordHash = CalculateHash(password);
        }

        /// <exception cref="ArgumentException"></exception>
        public void SetAccountId(string accountId)
        {
            Username = GetUsername(accountId);
        }
    }
}