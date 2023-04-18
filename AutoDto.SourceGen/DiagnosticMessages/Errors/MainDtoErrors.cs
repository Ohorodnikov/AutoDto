using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.DiagnosticMessages.Errors;

internal class MainDtoNotFoundError : ErrorDiagnosticMessage
{
    public MainDtoNotFoundError(string blTypeName, int dtosCount)
        : base(
            "3",
            "Main DTO not found",
            "Found {0} DTOs for type {1} and all without [DtoMain] attribute",
            new[] { dtosCount.ToString(), blTypeName })
    {
    }
}

internal class MoreThanOneMainDtoFoundError : ErrorDiagnosticMessage
{
    public MoreThanOneMainDtoFoundError(string blTypeName, int mainDtosCount)
        : base(
            "4",
            "More than 1 Main DTO found",
            "Found {0} DTOs with [DtoMain] attribute for type {1}. Only 1 DTO with [DtoMain] attribute is allowed",
            new[] { mainDtosCount.ToString(), blTypeName })
    {
    }
}
