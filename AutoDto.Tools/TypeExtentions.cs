using System.Reflection;

namespace AutoDto.Tools
{
    public static class TypeExtentions
    {
        public static PropertyInfo[] GetPublicInstProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }
    }
}