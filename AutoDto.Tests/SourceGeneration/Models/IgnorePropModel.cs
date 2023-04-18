using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoDto.Tests.SourceGeneration.Models;

public class IgnorePropModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime IgnoreDateTime { get; set; }
}
