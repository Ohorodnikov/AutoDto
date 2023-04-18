using AutoDto.Setup;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Tests.TestHelpers;

public class DtoCodeCreator
{
    public static string DtoTypNamespace = "AutoDto.Tests.Dto";

    public record DtoData(Type Type, RelationStrategy Strategy, string DtoName, List<(string nameSpace, string value)> AdditionalAttributes = null, SyntaxKind Access = SyntaxKind.PublicKeyword);

    public string GetTypeName(Type type)
    {
        var name = type.FullName;
        if (!type.IsGenericType)
            return name;

        var baseName = name.Substring(0, name.IndexOf('`'));

        var argsNames = type.GenericTypeArguments.Select(GetTypeName);

        return $"{baseName}<{string.Join(", ", argsNames)}>";
    }

    public (string nameSpace, string definition) GetDtoFromAttr(Type type, RelationStrategy relation)
    {
        var attr = typeof(DtoFromAttribute);
        var name = attr.Name.Replace(nameof(Attribute), "");

        return (attr.Namespace, $"[{name}(typeof({GetTypeName(type)}), {nameof(RelationStrategy)}.{relation.ToString()})]");
    }

    public (string nameSpace, string definition) GetDtoFromAttr(Type type)
    {
        var attr = typeof(DtoFromAttribute);
        var name = attr.Name.Replace(nameof(Attribute), "");

        return (attr.Namespace, $"[{name}(typeof({GetTypeName(type)}))]");
    }

    public string GetPublicDtoDef(string className)
    {
        return GetDtoClassDef(SyntaxKind.PublicKeyword, className);
    }

    public string GetDtoClassDef(SyntaxKind kind, string name)
    {
        return $"{SyntaxFacts.GetText(kind)} partial class {name}";
    }

    public string GetDtosDefinition(params DtoData[] dtos)
    {
        var dtosFromAttrs = new List<(List<string> attr, string def)>();
        var namespaces = new HashSet<string>();

        int i = 0;
        foreach (var dto in dtos)
        {
            var attr = GetDtoFromAttr(dto.Type, dto.Strategy);
            namespaces.Add(dto.Type.Namespace);
            namespaces.Add(attr.nameSpace);

            foreach (var nameSpace in (dto.AdditionalAttributes?.Select(x => x.nameSpace) ?? new List<string>()))
                namespaces.Add(nameSpace);

            var attrs = dto.AdditionalAttributes?.Select(x => x.value).ToList() ?? new List<string>();

            attrs.Add(attr.definition);

            dtosFromAttrs.Add((attrs, GetDtoClassDef(dto.Access, dto.DtoName)));

            i++;
        }

        var sb = new StringBuilder();

        foreach (var nameSpace in namespaces)
            sb.Append("using ").Append(nameSpace).Append(";").AppendLine();

        sb
            .AppendLine()
            .Append("namespace ").Append(DtoTypNamespace).AppendLine(";")
            .AppendLine();

        foreach (var classData in dtosFromAttrs)
        {

            foreach (var attr in classData.attr)
                sb.AppendLine(attr);

            sb
            .Append(classData.def).Append(" {}")
            .AppendLine()
            .AppendLine()
            ;
        }

        return sb.ToString();
    }
}

