using AutoDto.Setup;
using AutoDto.SourceGen.DiagnosticMessages;
using AutoDto.SourceGen.Metadatas;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Text;

namespace AutoDto.SourceGen.TypeParser;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal class DtoTypeMetadata : IDtoTypeMetadata
{
    public string BlFullName { get; set; }
    public string Name { get; set; }
    public string Namespace { get; set; }
    public Accessibility Accessibility { get; set; }
    public TypeDeclarationSyntax TypeDeclaration { get; set; }
    public ITypeSymbol TypeSymbol { get; set; }
    public List<(Location location, IDiagnosticMessage message)> DiagnosticMessages { get; } = new();
    public bool IsMain { get; set; }
    public RelationStrategy RelationStrategy { get; set; }
    public List<IPropertyMetadata> Properties { get; set; } = new();
    public Location Location { get; set; }

    public bool IsDefinitionValid => !DiagnosticMessages.Any(x => x.message.Severity == DiagnosticSeverity.Error);

    public string ToClassString()
    {
        var sb = new StringBuilder();
        sb
            .AppendLine($"namespace {Namespace};")
            .AppendLine()
            .Append(SyntaxFacts.GetText(Accessibility))
            .Append(" partial class ")
            .Append(Name)
            .AppendLine()
            .AppendLine("{");

        foreach (var prop in Properties)
            sb.AppendLine(prop.ToPropertyString());

        sb.Append("}");

        return sb.ToString();
    }

    private string GetDebuggerDisplay()
    {
        return $"{Accessibility} {Namespace} {Name}";
    }
}


