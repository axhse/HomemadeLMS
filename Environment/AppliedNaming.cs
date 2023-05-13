namespace HomemadeLMS.Environment
{
    public enum ComponentName
    {
        Application,
        AuthService,
        Host,
    }

    public enum PropertyName
    {
        ApiEmailAddress,
        ApiTimeoutInSeconds,
        DomainName,
        HasDemoContent,
        HasDevExceptionHandler,
        RequestLifetimeInMinutes,
    }

    public enum SecretName
    {
        DatabaseConnectionString,
        MailingApiKey,
        ManagerToken,
    }
}