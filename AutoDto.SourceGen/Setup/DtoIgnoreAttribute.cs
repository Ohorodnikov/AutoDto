using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Setup;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DtoIgnoreAttribute : Attribute
{
    public DtoIgnoreAttribute(params string[] ignored)
    {
    }
}
