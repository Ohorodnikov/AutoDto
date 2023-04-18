using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Tests.CompilerMessages.Models;

public class MasterType
{
    public int Id { get; set; }
    public string Name { get; set; }

    public RelationType RelationType { get; set; }
}

public class RelationType
{
    public int Id { get; set; }
    public string Name { get; set; }
}
