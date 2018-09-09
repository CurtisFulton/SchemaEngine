namespace SchemaCompare.SchemaEngine.Extensions
{
    public static class StringExtension
    {
        public static bool HasValue(this string value) => !string.IsNullOrWhiteSpace(value);
        public static bool IsEmpty(this string value) => string.IsNullOrWhiteSpace(value);
    }
}