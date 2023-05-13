namespace HomemadeLMS.Environment
{
    public class SecretManager
    {
        private readonly Dictionary<SecretName, string> values = new();

        public void Set(SecretName name, string value)
        {
            values[name] = value;
        }

        public string Get(SecretName name)
        {
            if (!TryGet(name, out string value))
            {
                ThrowPropertyIsNullException(name);
            }
            return value;
        }

        public string Get(SecretName name, string defaultValue)
        {
            if (!TryGet(name, out string value))
            {
                value = defaultValue;
            }
            return value;
        }

        public bool TryGet(SecretName name, out string value)
        {
            var result = values.TryGetValue(name, out string? nullableValue);
            nullableValue ??= string.Empty;
            value = nullableValue;
            return result;
        }

        private static void ThrowPropertyIsNullException(SecretName name)
        {
            var errorMessage = $"{name} is not specified";
            throw new NotSupportedException(errorMessage);
        }
    }
}