namespace WoopiAiHub.Repository.Util
{
    public static class AuditExtensions
    {
        public static bool ValidateEqualValues(object? currentValue, object? newValue)
        {
            if (currentValue == null && newValue == null)
                return true;

            if (currentValue == null || newValue == null)
                return false;

            if (currentValue is byte[] b1 && newValue is byte[] b2)
            {
                return b1.SequenceEqual(b2);
            }

            return Equals(currentValue, newValue);
        }
    }
}
