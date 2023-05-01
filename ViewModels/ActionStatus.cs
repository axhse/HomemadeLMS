namespace HomemadeLMS.ViewModels
{
    public enum ActionStatus
    {
        InternalError,
        InvalidFormData,
        NoAccess,
        NoPermission,
        NotFound,
        NotSupported,
        UnknownError,

        PasswordConfirmationError,
        PasswordInvalidFormat,

        HasNoToken,
        InvalidToken,
    }
}