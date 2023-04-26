using AutoDto.SourceGen.DiagnosticMessages;
using AutoDto.SourceGen.DiagnosticMessages.Errors;
using AutoDto.SourceGen.DiagnosticMessages.Warnings;
using AutoDto.SourceGen.Helpers;
using AutoDto.SourceGen.Metadatas;
using AutoDto.SourceGen.TypeParser;
using Microsoft.CodeAnalysis;

namespace AutoDto.SourceGen.MetadataUpdaters.AllMetadatasUpdaters;

internal class ConflictNamesChecker : IAllMetadatasUpdater
{
    public void UpdateAllMetadata(Dictionary<string, List<IDtoTypeMetadata>> metadatas)
    {
        LogHelper.Logger.Information("Apply ConflictNamesChecker");
        foreach (var metadata in metadatas.SelectMany(x => x.Value))
        {
            CheckOwnMembers(metadata);
            CheckInheritedMembers(metadata);
        }
    }

    private void CheckOwnMembers(IDtoTypeMetadata metadata)
    {
        var members = metadata.TypeSymbol.GetMembers();

        CheckMembers(metadata, members, true);
    }

    private void CheckInheritedMembers(IDtoTypeMetadata metadata)
    {
        var visibleMembers = metadata.TypeSymbol.BaseType.GetAllMembersFromAllBaseTypes()
            .Where(IsVisible)
            .ToList();

        CheckMembers(metadata, visibleMembers, false);
    }

    private void CheckMembers(IDtoTypeMetadata metadata, IEnumerable<ISymbol> members, bool allNotPropIsError)
    {
        void AddDiagnostic(IDiagnosticMessage message)
        {
            metadata.DiagnosticMessages.Add((metadata.Location, message));
        }

        void CheckOnSpecialName(string specialName)
        {
            var specialMember = FindMemberByName(members, specialName);

            if (specialMember != null)
                AddDiagnostic(new ReservedMemberConflicError(specialName, metadata.Name));
        }

        var props2remove = new List<IPropertyMetadata>();
        foreach (var prop in metadata.Properties)
        {
            var visibleOwnMembers = FindMemberByName(members, prop.Name);

            if (visibleOwnMembers == null)
            {
                CheckOnSpecialName("get_" + prop.Name);
                CheckOnSpecialName("set_" + prop.Name);
                continue;
            }

            if (visibleOwnMembers.Kind == SymbolKind.Property && !visibleOwnMembers.IsStatic) //exclude only non static visible property
                props2remove.Add(prop);

            AddDiagnostic(GetDiagnostic(visibleOwnMembers, prop.Name, metadata.Name, allNotPropIsError));
        }

        metadata.Properties.RemoveAll(x => props2remove.Contains(x));
    }

    private IDiagnosticMessage GetDiagnostic(ISymbol member, string memberName, string dtoName, bool allNotPropIsError)
    {
        if (member.Kind == SymbolKind.Property)
            return member.IsStatic
                ? new MemberConflictError(memberName, dtoName)
                : new PropertyConflictWarning(memberName, dtoName);

        if (allNotPropIsError || GetAccessibility(member) == Accessibility.Public)
            return new MemberConflictError(memberName, dtoName);
        
        return new MemberConflictWarning(memberName, dtoName);
    }

    private ISymbol FindMemberByName(IEnumerable<ISymbol> members, string name)
    {
        return members.FirstOrDefault(x =>
        {
            var nameToCompare = x.Name;
            if (x is IMethodSymbol ms && ms.MethodKind == MethodKind.Constructor)
                nameToCompare = ms.ContainingType.Name;

            return nameToCompare == name;
        });
    }

    private Accessibility[] _visibleAccessability = new[]
    {
        Accessibility.Public,
        Accessibility.Protected,
        Accessibility.Internal,
        Accessibility.ProtectedOrInternal,
        Accessibility.ProtectedAndInternal
    };

    private bool IsVisible(ISymbol symbol)
    {
        if (!_visibleAccessability.Contains(GetAccessibility(symbol)))
            return false;

        if (symbol is IMethodSymbol methodSymbol)
        {
            if (!methodSymbol.CanBeReferencedByName)
                return false;

            if (methodSymbol.MethodKind == MethodKind.Destructor || methodSymbol.MethodKind == MethodKind.Constructor)
                return false;

            return true;
        }

        if (symbol is IPropertySymbol propertySymbol)
            return true;

        if (symbol is IFieldSymbol fs)
            return true;

        return true;
    }

    private Accessibility GetAccessibility(ISymbol symbol)
    {
        if (symbol is IMethodSymbol ms)
            return ms.DeclaredAccessibility;

        if (symbol is IPropertySymbol ps)
            return ps.DeclaredAccessibility;

        if (symbol is IFieldSymbol fs)
            return fs.DeclaredAccessibility;

        throw new NotImplementedException();
    }
}
