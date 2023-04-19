using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text;

namespace HomemadeLMS.Services.Data
{
    public class StringHexConverter : ValueConverter<string?, string?>
    {
        public StringHexConverter()
            : base(text => ToHexString(text), hexString => FromHexString(hexString))
        { }

        public static string? FromHexString(string? hexString)
        {
            if (hexString is null)
            {
                return null;
            }
            return Encoding.UTF8.GetString(Convert.FromHexString(hexString));
        }

        public static string? ToHexString(string? text)
        {
            if (text is null)
            {
                return null;
            }
            return Convert.ToHexString(Encoding.UTF8.GetBytes(text));
        }
    }
}