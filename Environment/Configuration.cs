namespace HomemadeLMS.Environment
{
    public class ConfigurationComponent
    {
        private readonly Dictionary<PropertyName, bool> boolValues = new();
        private readonly Dictionary<PropertyName, int> intValues = new();
        private readonly Dictionary<PropertyName, string> stringValues = new();

        public void Set(PropertyName name, bool value)
        {
            boolValues[name] = value;
        }

        public void Set(PropertyName name, int value)
        {
            intValues[name] = value;
        }

        public void Set(PropertyName name, string value)
        {
            stringValues[name] = value;
        }

        public bool GetBool(PropertyName name)
        {
            if (!TryGet(name, out bool value))
            {
                ThrowPropertyIsNullException(name);
            }
            return value;
        }

        public int GetInt(PropertyName name)
        {
            if (!TryGet(name, out int value))
            {
                ThrowPropertyIsNullException(name);
            }
            return value;
        }

        public string GetString(PropertyName name)
        {
            if (!TryGet(name, out string value))
            {
                ThrowPropertyIsNullException(name);
            }
            return value;
        }

        public bool Get(PropertyName name, bool defaultValue)
        {
            if (!TryGet(name, out bool value))
            {
                value = defaultValue;
            }
            return value;
        }

        public int Get(PropertyName name, int defaultValue)
        {
            if (!TryGet(name, out int value))
            {
                value = defaultValue;
            }
            return value;
        }

        public string Get(PropertyName name, string defaultValue)
        {
            if (!TryGet(name, out string value))
            {
                value = defaultValue;
            }
            return value;
        }

        public bool TryGet(PropertyName name, out bool value)
            => boolValues.TryGetValue(name, out value);

        public bool TryGet(PropertyName name, out int value)
            => intValues.TryGetValue(name, out value);

        public bool TryGet(PropertyName name, out string value)
        {
            var result = stringValues.TryGetValue(name, out string? nullableValue);
            nullableValue ??= string.Empty;
            value = nullableValue;
            return result;
        }

        private static void ThrowPropertyIsNullException(PropertyName name)
        {
            var errorMessage = $"{name} is not specified";
            throw new NotSupportedException(errorMessage);
        }
    }

    public class ConfigurationGroup
    {
        private readonly Dictionary<ComponentName, ConfigurationComponent> components = new();

        public void SetComponent(ComponentName name, ConfigurationComponent component)
        {
            components[name] = component;
        }

        public ConfigurationComponent GetComponent(ComponentName name)
        {
            components.TryGetValue(name, out ConfigurationComponent? nullableComponent);
            nullableComponent ??= new();
            return nullableComponent;
        }

        public ConfigurationComponent this[ComponentName name]
        {
            get => GetComponent(name);
            set
            {
                SetComponent(name, value);
            }
        }
    }
}