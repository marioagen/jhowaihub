namespace WoopiAiHub.Api.Attributes
{
    /// <summary>
    /// Defines that the method does not need to pass the tenant in the header as mandatory.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class OptionalTenantHeaderAttribute : Attribute
    {
    }
}
