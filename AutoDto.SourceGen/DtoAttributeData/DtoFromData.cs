using AutoDto.Setup;
using AutoDto.SourceGen.DiagnosticMessages;
using AutoDto.SourceGen.Helpers;
using AutoDto.SourceGen.Metadatas;
using Microsoft.CodeAnalysis;

namespace AutoDto.SourceGen.DtoAttributeData;

internal class DtoForData : DtoFromData { }

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

        LogHelper.Logger.Information("Set bl type as {blName} with {propCount} properties", typeSymbol.Name, Properties.Count());
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

        LogHelper.Logger.Information("Set relation strategy {strategy}", strategy);
    }
}
