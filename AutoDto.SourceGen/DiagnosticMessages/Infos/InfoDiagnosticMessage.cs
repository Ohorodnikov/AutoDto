using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.DiagnosticMessages.Infos;

internal abstract class InfoDiagnosticMessage : BaseDiagnosticMessage
{
    public InfoDiagnosticMessage(string id, string title, string description, string[] parameters = null)
        : base(id, title, description, DiagnosticSeverity.Info, parameters)
    {
    }

    protected override string IdPrefix => "I";
}

internal class CompilationSourceNotValidInfo : InfoDiagnosticMessage
{
    public CompilationSourceNotValidInfo() 
        : base("1", "Source is not valid", "AutoDtoGenerator was not ran. Compilation is not valid. Fix all errors related to DTOs.")
    {
    }
}
