using AutoDto.SourceGen.DiagnosticMessages;
using Microsoft.CodeAnalysis;

namespace AutoDto.SourceGen.DtoAttributeData;

internal interface IDtoAttributeData
{
    List<IDiagnosticMessage> DiagnosticMessages { get; }
    Location Location { get; set; }

    void InitOneValue(int paramOrder, TypedConstant value);
}

internal abstract class DtoAttributeData : IDtoAttributeData
{
    public List<IDiagnosticMessage> DiagnosticMessages { get; } = new List<IDiagnosticMessage>();
    public Location Location { get; set; }

    private Action<TypedConstant>[] actions;

    protected abstract Action<TypedConstant>[] InitActions();

    public void InitOneValue(int paramOrder, TypedConstant value)
    {
        actions ??= InitActions();

        actions[paramOrder](value);
    }
}


