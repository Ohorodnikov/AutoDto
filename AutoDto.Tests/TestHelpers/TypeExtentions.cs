using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Tests.TestHelpers;

public static class TypeExtentions
{
    public static PropertyInfo[] GetPublicInstProperties(this Type type)
    {
        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
    }
}
