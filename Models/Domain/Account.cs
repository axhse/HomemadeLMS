using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace HomemadeLMS.Models.Domain
{
    public enum UserRole
    {
        Manager,
        None,
        Student,
        Teacher,
    }

    public class Account
    {
        public const int MaxNameSize = 200;
        public const int MaxPasswordSize = 50;
        public const int MaxTelegramUsernameSize = 32;
        public const int MaxUsernameSize = 100;
        public const int MinPasswordSize = 8;
        public const int MinTelegramUsernameSize = 5;
        public const int MinUsernameSize = 1;
        public const int PasswordHashSize = 64;
        public const string EmailAddressBase = "@edu.hse.ru";
        public static readonly int MaxAccountIdSize = MaxUsernameSize + EmailAddressBase.Length;

        private string username;
        private string? headUsername;
        private string? name;
        private string? passwordHash;
        private string? telegramUsername;

        public Account(string username, UserRole role)
        {
            Username = username;
            Role = role;
            this.username = Username;
        }

        public Account(string accountId, UserRole role, string? password = null)
        {
            Role = role;
            SetAccountId(accountId);
            if (password is not null)
            {
                SetPassword(password);
            }

            username = Username;
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

        public static string GetEmailAddress(string username) => username + EmailAddressBase;

        public static string GetUsername(string? accountId)
        {
            if (accountId is null)
            {
                return string.Empty;
            }
            accountId = accountId.Trim().ToLower();
            if (accountId.EndsWith(EmailAddressBase))
            {
                return accountId[..^EmailAddressBase.Length];
            }
            return accountId;
        }

        public static bool HasPasswordHashValidFormat(string? passwordHash)
            => passwordHash is not null && Regex.IsMatch(passwordHash, $"^[A-F0-9]{{{PasswordHashSize}}}$");

        public static bool HasPasswordValidFormat(string? password)
            => password is not null && MinPasswordSize <= password.Length && password.Length <= MaxPasswordSize;

        public static bool HasTelegramUsernameValidFormat(string? username)
            => username is not null && Regex.IsMatch(username, $"^[A-Za-z0-9_]{{{MinTelegramUsernameSize},{MaxTelegramUsernameSize}}}$");

        public static bool HasUsernameValidFormat(string? username)
            => username is not null && Regex.IsMatch(username, $"^[a-z0-9_.]{{{MinUsernameSize},{MaxUsernameSize}}}$");

        public static bool HasNameValidFormat(string? name)
        {
            name = DataUtils.CleanSpaces(name);
            return name is null || name.Length <= MaxNameSize;
        }

        public UserRole Role { get; set; }

        public string? PasswordHash
        {
            get => passwordHash;
            set
            {
                if (value is not null && !HasPasswordHashValidFormat(value))
                {
                    throw new ArgumentException("Invalid passwordHash format.");
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

        public string? HeadUsername
        {
            get => headUsername;
            set
            {
                if (value is not null && !HasUsernameValidFormat(value))
                {
                    throw new ArgumentException("Invalid username format.");
                }
                headUsername = value;
            }
        }

        public string? Name
        {
            get => name;
            set
            {
                var newName = DataUtils.CleanSpaces(value);
                if (!HasNameValidFormat(newName))
                {
                    throw new ArgumentException("Invalid name format.");
                }
                name = newName;
            }
        }

        public string? TelegramUsername
        {
            get => telegramUsername;
            set
            {
                if (value is not null && !HasTelegramUsernameValidFormat(value))
                {
                    throw new ArgumentException("Invalid telegram username format.");
                }
                telegramUsername = value;
            }
        }

        public bool CanEditCourses => Role == UserRole.Teacher || Role == UserRole.Manager;
        public string EmailAddress => GetEmailAddress(Username);

        public void SetAccountId(string accountId)
        {
            Username = GetUsername(accountId);
        }

        public void SetPassword(string password)
        {
            if (!HasPasswordValidFormat(password))
            {
                throw new ArgumentException("Invalid username format.");
            }
            PasswordHash = CalculateHash(password);
        }

        public bool CanChangeRoleOf(Account other)
        {
            return Username != other.Username && Role == UserRole.Manager && (
                other.Role != UserRole.Manager || other.HeadUsername == Username
            );
        }

        public bool IsPasswordCorrect(string? password) => password is not null
            && HasPasswordValidFormat(password) && CalculateHash(password) == PasswordHash;
    }
}