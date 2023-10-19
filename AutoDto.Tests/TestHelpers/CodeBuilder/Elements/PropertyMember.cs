namespace AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

public class PropertyMember : Member
{
    public PropertyMember(string fullCode) : base(fullCode) { }

    public PropertyMember(
        BaseDeclarationInfo declarationInfo,
        string returnType,
        (Visibility visibitity, bool isExists) get,
        (Visibility visibitity, bool isExists) set)
        : base(declarationInfo, returnType)
    {
        Get = get;
        Set = set;
    }

    public (Visibility visibitity, bool isExists) Get { get; }
    public (Visibility visibitity, bool isExists) Set { get; }

    protected override string GenerateCodeFromDeclaration()
    {
        string ToStr((Visibility vsblt, bool isExists) prm, string name)
        {
            if (!prm.isExists)
                return string.Empty;

            var str = prm.vsblt == Visibility ? string.Empty : EnumToString(prm.vsblt);

            return $"{str} {name};";
        }

        return $"{GetDefinitionString()} {{ {ToStr(Get, "get")} {ToStr(Set, "set")} }}";
    }
}

