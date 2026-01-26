namespace WoopiAiHub.Domain.Utils.ErrorLabels
{
    public static class Login
    {
        public const string UserNotFound = "login.userNotFound";
        public const string UserWithoutAccess = "login.userWithoutAccess";
        public const string UserIncorrectPassword = "login.userIncorrectPassword";
        public const string UserTokenMicrosoftInvalid = "login.userTokenMicrosoftInvalid";
        public const string TenantDatabaseNotReady = "login.tenantDatabaseNotReady";
        public const string TenantNotFound = "login.tenantNotFound";
    }
}
