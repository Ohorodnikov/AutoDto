using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.SourceGen.DiagnosticMessages;
using AutoDto.SourceGen.Metadatas;
using AutoDto.SourceGen.TypeParser;
using Microsoft.CodeAnalysis;

namespace AutoDto.SourceGen.MetadataUpdaters.RelationStrategyAppliers;

internal class ReplaceToDtoStrategyApplier : IStrategyApplier
{
    private IPropertyMetadata CreateRelationMetadata(IPropertyMetadata relationFrom, IDtoTypeMetadata relationTo)
    {
        switch (relationFrom.GetCollectionType())
        {
            case CollectionType.NonCollection:
                return PropertyMetadata.CreateObjectProperty(relationFrom.Name, relationTo.TypeSymbol, relationFrom.Accessibility);
            case CollectionType.Enumerable:
                var collectionName = relationFrom.Type.ContainingNamespace + "." + relationFrom.Type.Name;
                return PropertyMetadata.CreateGenericProperty(relationFrom.Name, collectionName, relationTo.TypeSymbol, relationFrom.Accessibility);
            case CollectionType.Array:
                return PropertyMetadata.CreateArrayProperty(relationFrom.Name, relationTo.TypeSymbol, relationFrom.Accessibility);
            default:
                throw new NotImplementedException();
        }
    }

    private bool TryFindMainDto(List<IDtoTypeMetadata> candidates, out IDtoTypeMetadata[] mains)
    {
        if (candidates.Count == 1)
        {
            mains = candidates.ToArray();
            return true;
        }

        mains = candidates.Where(x => x.IsMain).ToArray();

        return mains.Length == 1;
    }

    public void Apply(IDtoTypeMetadata metadata, Dictionary<string, List<IDtoTypeMetadata>> allMetadatas)
    {
        var props2Remove = new List<IPropertyMetadata>();
        var props2Add = new List<IPropertyMetadata>();
        foreach (var prop in metadata.Properties)
        {
            var propType = prop.GetElementTypeName();

            if (!allMetadatas.TryGetValue(propType, out var dtosRef) || dtosRef.Count == 0)
                continue;

            if (TryFindMainDto(dtosRef, out var mains))
            {
                var main = mains[0];

                props2Remove.Add(prop);

                var newMd = CreateRelationMetadata(prop, main);

                if (newMd != null)
                    props2Add.Add(newMd);
            }
            else
            {
                IDiagnosticMessage msg = null;
                if (mains.Length == 0)
                    msg = new MainDtoNotFoundError(propType, dtosRef.Count);

                if (mains.Length > 1)
                    msg = new MoreThanOneMainDtoFoundError(propType, mains.Length);

                if (msg is not null)
                    metadata.DiagnosticMessages.Add((null, msg));
            }
        }

        if (props2Remove.Count != 0)
            metadata.Properties.RemoveAll(x => props2Remove.Contains(x));

        metadata.Properties.AddRange(props2Add);
    }
}

