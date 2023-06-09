﻿using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace HomemadeLMS.Models
{
    public static class DataUtils
    {
        public const int MskHourOffset = +3;
        public const char SpaceChar = '\u0020';
        public const string DateTimeStringFormat = "dd.MM.yyyy H:mm";
        public static readonly int MaxNumericStringSize = long.MinValue.ToString().Length;
        
        public static bool IsValidEmailAddress(string? emailAddress)
        {
            if (emailAddress is null)
            {
                return false;
            }
            if (emailAddress.Trim().EndsWith("."))
            {
                return false;
            }
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(emailAddress.Trim().ToLower());
                return mailAddress.Address == emailAddress;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValuable(string? text)
        {
            return text is not null && text.Trim() != string.Empty;
        }

        public static string? CleanSpaces(string? text)
        {
            if (text is null)
            {
                return null;
            }
            text = text.Trim();
            text = Regex.Replace(text, "[\t\r\n\f]", string.Empty);
            text = Regex.Replace(text, $"{SpaceChar}{{2,}}", $"{SpaceChar}");
            return text;
        }

        public static DateTime GetMskDateTime(DateTime dateTime) => dateTime.AddHours(MskHourOffset);

        public static string? GetTrimmed(string? text)
        {
            if (text is null)
            {
                return null;
            }
            return text.Trim();
        }

        public static string DateToMskString(DateTime dateTime)
            => DateToString(GetMskDateTime(dateTime));

        public static string DateToString(DateTime dateTime)
            => dateTime.ToString(DateTimeStringFormat);
    }
}