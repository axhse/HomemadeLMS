namespace HomemadeLMS.Environment
{
    public static class Builder
    {
        public static ConfigurationGroup BuildConfiguration(ConfigurationSpec configurationSpec)
        {
            var configuration = new ConfigurationGroup();
            var configurationRoot = GetConfigurationRoot();
            foreach (var componentSpec in configurationSpec.Components)
            {
                var component = new ConfigurationComponent();
                var componentRoot = configurationRoot.GetSection($"{componentSpec.Name}");
                foreach (var propertySpec in componentSpec.Properties)
                {
                    bool isValueSpecified = false;
                    if (propertySpec.Type == PropertyType.Bool)
                    {
                        var nullableValue = componentRoot.GetValue<bool?>($"{propertySpec.Name}");
                        if (nullableValue is bool value)
                        {
                            component.Set(propertySpec.Name, value);
                        }
                        isValueSpecified = true;
                    }
                    if (propertySpec.Type == PropertyType.Int)
                    {
                        var nullableValue = componentRoot.GetValue<int?>($"{propertySpec.Name}");
                        if (nullableValue is int value)
                        {
                            component.Set(propertySpec.Name, value);
                        }
                        isValueSpecified = true;
                    }
                    if (propertySpec.Type == PropertyType.String)
                    {
                        var nullableValue = componentRoot.GetValue<string?>($"{propertySpec.Name}");
                        if (nullableValue is string value)
                        {
                            component.Set(propertySpec.Name, value);
                        }
                        isValueSpecified = true;
                    }
                    if (propertySpec.IsRequired && !isValueSpecified)
                    {
                        var errorMessage = $"{componentSpec.Name}.{propertySpec.Name} is required.";
                        throw new ArgumentException(errorMessage);
                    }
                }
                configuration.SetComponent(componentSpec.Name, component);
            }
            return configuration;
        }

        public static SecretManager BuildSecretManager(SecretManagerSpec secretManagerSpec)
        {
            var secretManager = new SecretManager();
            var secretRoot = GetSecretRoot();
            foreach (var secretSpec in secretManagerSpec.Secrets)
            {
                var nullableValue = secretRoot.GetValue<string?>($"{secretSpec.Name}");
                if (nullableValue is string value)
                {
                    secretManager.Set(secretSpec.Name, value);
                }
                if (secretSpec.IsRequired && nullableValue is null)
                {
                    throw new ArgumentException($"{secretSpec.Name} is required.");
                }
            }
            return secretManager;
        }

        private static IConfigurationRoot GetConfigurationRoot()
        {
            var configurationRoot = new ConfigurationBuilder()
               .AddJsonFile($"configuration.{GetBuildName()}.json", false, false)
               .Build();
            return configurationRoot;
        }

        private static IConfigurationRoot GetSecretRoot()
        {
            var secretRoot = new ConfigurationBuilder()
               .AddJsonFile($"secrets.{GetBuildName()}.json", false, false)
               .Build();
            return secretRoot;
        }

        private static string GetBuildName()
        {
            var buildNameRoot = new ConfigurationBuilder()
               .AddJsonFile($"buildName.json", false, false)
               .Build();
            string? buildName = buildNameRoot.GetValue<string?>("BuildName");
            if (buildName is null)
            {
                throw new ArgumentException("BuildName is not specified.");
            }
            return buildName;
        }
    }
}