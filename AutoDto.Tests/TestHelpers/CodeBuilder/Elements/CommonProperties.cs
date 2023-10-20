using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

public static class CommonProperties
{
    public static PropertyMember Id_Int => new PropertyBuilder("Id", typeof(int)).Build();
    public static PropertyMember Name => new PropertyBuilder("Name", typeof(string)).Build();
    public static PropertyMember Description => new PropertyBuilder("Description", typeof(string)).Build();
}
