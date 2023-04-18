using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.DiagnosticMessages.Warnings;

internal abstract class WarningDiagnosticMessage : BaseDiagnosticMessage
{
    protected WarningDiagnosticMessage(string id, string title, string description, string[] parameters = null) 
        : base(id, title, description, DiagnosticSeverity.Warning, parameters)
    {
    }

    protected override string IdPrefix => "W";
}
