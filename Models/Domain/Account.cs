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
        public const int MinUsernameSize = 1;
        public const int MaxUsernameSize = 100;
        public const int MinPasswordSize = 8;
        public const int MaxPasswordSize = 50;
        public const int PasswordHashSize = 64;

        private const string EmailAddressBase = "@edu.hse.ru";
        private string username;
        private string passwordHash;
        private string? headUsername;

        /// <exception cref="ArgumentException"></exception>
        public Account(string username, string passwordHash, UserRole role, string? headUsername)
        {
            Username = username;
            PasswordHash = passwordHash;
            Role = role;
            HeadUsername = headUsername;

            this.username = Username;
            this.passwordHash = PasswordHash;
        }

        /// <exception cref="ArgumentException"></exception>
        public Account(string accountId, string password)
        {
            SetAccountId(accountId);
            SetPassword(password);

            username = Username;
            passwordHash = PasswordHash;
        }

        public static bool HasPasswordHashValidFormat(string? passwordHash)
            => passwordHash is not null && Regex.IsMatch(passwordHash, $"^[A-F0-9]{{{PasswordHashSize}}}$");

        public static bool HasPasswordValidFormat(string? password)
            => password is not null && MinPasswordSize <= password.Length && password.Length <= MaxPasswordSize;

        public static bool HasUsernameValidFormat(string? username)
            => username is not null && Regex.IsMatch(username, $"^[a-z0-9_.]{{{MinUsernameSize},{MaxUsernameSize}}}$");

        public static string GetUsername(string accountId)
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

        public UserRole Role { get; set; } = UserRole.None;

        /// <exception cref="ArgumentException"></exception>
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

        /// <exception cref="ArgumentException"></exception>
        public string PasswordHash
        {
            get => passwordHash;
            set
            {
                if (!HasPasswordHashValidFormat(value))
                {
                    throw new ArgumentException("Invalid passwordHash format.");
                }
                passwordHash = value;
            }
        }

        /// <exception cref="ArgumentException"></exception>
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

        public string EmailAddress => Username + EmailAddressBase;
        public bool CanEditCourses => Role == UserRole.Teacher || Role == UserRole.Manager;

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

        public bool CanChangeRoleOf(Account other)
        {
            return Username != other.Username && Role == UserRole.Manager && (
                other.Role != UserRole.Manager || other.HeadUsername == Username
            );
        }
    }
}