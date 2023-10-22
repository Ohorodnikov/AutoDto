using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

public enum Visibility
{
    Public,
    Protected,
    Private,
    Internal
}


public enum InheritanceStatus
{
    None,
    Virtual,
    Abstract
}

public record AttributeInfo(string Name, string[] Arguments);

public record BaseDeclarationInfo(Visibility Visibility, bool IsStatic, bool IsPartial, InheritanceStatus InheritanceStatus, string Name, List<AttributeInfo> Attributes);

public abstract class CodeElement
{
    private readonly string _fullCode;

    public CodeElement(string fullCode)
    {
        _fullCode = fullCode;
    }

    public CodeElement(BaseDeclarationInfo declarationInfo)
    {
        Visibility = declarationInfo.Visibility;
        IsStatic = declarationInfo.IsStatic;
        IsPartial = declarationInfo.IsPartial;
        Name = declarationInfo.Name;
        Attributes = declarationInfo.Attributes;
        Inheritance = declarationInfo.InheritanceStatus;
    }

    public Visibility Visibility { get; private set; }
    public bool IsStatic { get; private set; }
    public bool IsPartial { get; private set; }
    public InheritanceStatus Inheritance { get; private set; }
    public string Name { get; private set; }
    public List<AttributeInfo> Attributes { get; private set; }

    public string GenerateCode()
    {
        return _fullCode ?? GenerateCodeFromDeclaration();
    }

    protected abstract string GenerateCodeFromDeclaration();

    protected string GetVisibilityString() => EnumToString(Visibility);
    protected string GetStaticString() => IsStatic ? "static" : string.Empty;
    protected string GetPartialString() => IsPartial ? "partial" : string.Empty;
    protected string GetInheritanceString() => Inheritance == InheritanceStatus.None ? string.Empty : EnumToString(Inheritance);
    protected string EnumToString(Enum @enum) => @enum.ToString().ToLower();

    protected string GetAttributesString()
    {
        var sb = new StringBuilder();

        foreach (var attr in Attributes)
        {
            sb
              .Append('[')
              .Append(attr.Name);

            if (attr.Arguments != null && attr.Arguments.Length > 0)
                sb.Append('(').Append(string.Join(", ", attr.Arguments)).Append(')');

            sb.AppendLine("]");
        }

        return sb.ToString();
    }
}
