using System.Text;

namespace AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

public class ClassElement : CodeElement
{
    public ClassElement(string fullCode) : base(fullCode)
    {
    }

    public ClassElement(
        string @namespace,
        BaseDeclarationInfo declarationInfo,
        List<string> baseTypes, //class and/or interfaces
        List<Member> members,
        HashSet<string> usings
        ) : base(declarationInfo)
    {
        Namespace = @namespace;
        BaseTypes = baseTypes;
        Members = members;
        Usings = usings;
    }

    public string Namespace { get; }
    public List<string> BaseTypes { get; }
    public List<Member> Members { get; }
    public HashSet<string> Usings { get; }

    protected override string GenerateCodeFromDeclaration()
    {
        var sb = new StringBuilder();

        var hasNamespace = !string.IsNullOrEmpty(Namespace);

        if (hasNamespace)
        {
            sb
              .Append("namespace ")
              .AppendLine(Namespace)
              .AppendLine("{")
              ;

            foreach (var usng in Usings.Where(x => !string.IsNullOrEmpty(x)))
                sb
                  .Append("using ")
                  .Append(usng)
                  .AppendLine(";")
                  ;

            sb.AppendLine();
        }

        sb
          .Append(GetAttributesString())
          .Append(" ").Append(GetVisibilityString())
          .Append(" ").Append(GetStaticString())
          .Append(" ").Append(GetPartialString())
          .Append(" ").Append(GetInheritanceString())
          .Append(" ").Append("class")
          .Append(" ").Append(Name)
          ;

        if (BaseTypes.Count > 0)
            sb.Append(" : ").Append(string.Join(", ", BaseTypes));

        sb
          .AppendLine()
          .AppendLine("{")
          ;

        foreach (var member in Members)
            sb.AppendLine(member.GenerateCode());

        sb.AppendLine("}");

        if (hasNamespace)
            sb.AppendLine("}");

        return sb.ToString();
    }
}

