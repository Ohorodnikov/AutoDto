using System.Text;

namespace AutoDto.Tests.SourceGeneration.Models;

public enum MyEnum
{

}

public class TypeWithMultipleProps
{
    public int Id { get; set; }
    public DateTimeKind Kind { get; set; }
    public StringBuilder StringBuilderProp { get; set; }
    public string MyString { get; set; }
    public MyEnum MyEnum { get; set; }
}
