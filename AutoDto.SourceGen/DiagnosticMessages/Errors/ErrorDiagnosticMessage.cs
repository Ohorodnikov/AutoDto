using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.DiagnosticMessages.Errors;

internal abstract class ErrorDiagnosticMessage : BaseDiagnosticMessage
{
    protected ErrorDiagnosticMessage(string id, string title, string description, string[] parameters = null) 
        : base(id, title, description, DiagnosticSeverity.Error, parameters)
    {
    }
    protected override string IdPrefix => "E";
}
