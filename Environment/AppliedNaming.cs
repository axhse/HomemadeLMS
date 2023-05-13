namespace HomemadeLMS.Environment
{
    public enum ComponentName
    {
        Application,
        Host,
        Mailer,
    }

    public enum PropertyName
    {
        DomainName,
        HasDemoContent,
        HasDevExceptionHandler,
        ServiceEmailAddress,
        TimeoutInSeconds,
    }

    public enum SecretName
    {
        DatabaseConnectionString,
        MailingApiKey,
        ManagerToken,
    }
}