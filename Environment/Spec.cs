namespace HomemadeLMS.Environment
{
    public enum PropertyType
    {
        Bool,
        Int,
        String,
    }

    public class PropertySpec
    {
        public PropertySpec(PropertyType type, PropertyName name, bool isRequired)
        {
            Type = type;
            Name = name;
            IsRequired = isRequired;
        }

        public bool IsRequired { get; private set; }
        public PropertyName Name { get; private set; }
        public PropertyType Type { get; private set; }
    }

    public class ComponentSpec
    {
        public ComponentSpec(ComponentName name)
        {
            Name = name;
        }

        public ComponentName Name { get; private set; }
        public List<PropertySpec> Properties { get; set; } = new();
    }

    public class ConfigurationSpec
    {
        public List<ComponentSpec> Components { get; set; } = new();
    }

    public class SecretSpec
    {
        public SecretSpec(SecretName name, bool isRequired)
        {
            Name = name;
            IsRequired = isRequired;
        }

        public bool IsRequired { get; private set; }
        public SecretName Name { get; private set; }
    }

    public class SecretManagerSpec
    {
        public List<SecretSpec> Secrets { get; set; } = new();
    }
}