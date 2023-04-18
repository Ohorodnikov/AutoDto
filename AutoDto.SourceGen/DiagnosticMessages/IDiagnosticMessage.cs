using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleToAttribute("AutoDto.Tests")]

namespace AutoDto.SourceGen.DiagnosticMessages;

internal interface IDiagnosticMessage
{
    string Id { get; }
    string Title { get; }
    string Description { get; }
    DiagnosticSeverity Severity { get; }
    string[] Parameters { get; }
    DiagnosticDescriptor AsDiagnosticDescriptor();
}

internal abstract class BaseDiagnosticMessage : IDiagnosticMessage
{
    private const string GLOBAL_ID_PREFIX = "AD";
    private const string CATEGORY = "AutoDto";

    protected BaseDiagnosticMessage(string id, string title, string description, DiagnosticSeverity severity, string[] parameters = null)
    {
        _id = id;
        Title = title;
        Description = description;
        Severity = severity;
        Parameters = parameters ?? new string[0];
    }
    protected abstract string IdPrefix { get; }

    private string _id;
    public string Id => string.Concat(GLOBAL_ID_PREFIX + IdPrefix, _id);
    public string Title { get; }
    public string Description { get; }
    public DiagnosticSeverity Severity { get; }
    public string[] Parameters { get; }        

    public DiagnosticDescriptor AsDiagnosticDescriptor()
    {
        return new DiagnosticDescriptor(
            Id,
            Title,
            Description,
            CATEGORY,
            Severity,
            true);
    }
}


