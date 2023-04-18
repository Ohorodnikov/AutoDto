using AutoDto.Setup;
using AutoDto.SourceGen.DiagnosticMessages;
using AutoDto.SourceGen.Helpers;
using AutoDto.SourceGen.Metadatas;
using Microsoft.CodeAnalysis;

namespace AutoDto.SourceGen.DtoAttributeData;

internal class DtoFromData : DtoAttributeData
{
    protected override Action<TypedConstant>[] InitActions()
    {
        return new[]
        {
            ParseBlTypeParam,
            ParseRelationStrategy,
        };
    }

    public INamedTypeSymbol BlTypeSymbol { get; set; }
    public RelationStrategy RelationStrategy { get; set; }
    public IEnumerable<IPropertySymbol> Properties { get; set; }

    private void ParseBlTypeParam(TypedConstant parameter)
    {
        var typeSymbol = (INamedTypeSymbol)parameter.Value;

        BlTypeSymbol = typeSymbol;

        Properties = GetProperties(typeSymbol);
    }

    private IEnumerable<IPropertySymbol> GetProperties(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol
                        .GetAllMembersFromAllBaseTypes()
                        .OfType<IPropertySymbol>()
                        .Where(x => x.DeclaredAccessibility == Accessibility.Public)
                        .Where(x => !x.IsStatic)
                        .Where(x => !x.IsWriteOnly)
                        .Where(x => x.GetMethod != null && x.GetMethod.DeclaredAccessibility == Accessibility.Public)
                        .ToList();
    }

    private void ParseRelationStrategy(TypedConstant parameter)
    {
        Enum.TryParse<RelationStrategy>(parameter.Value.ToString(), out var strategy);

        RelationStrategy = strategy;
    }
}
