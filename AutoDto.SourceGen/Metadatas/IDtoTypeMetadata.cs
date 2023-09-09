using AutoDto.Setup;
using AutoDto.SourceGen.DiagnosticMessages;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace AutoDto.SourceGen.Metadatas;

internal interface IDtoTypeMetadata
{
    string BlFullName { get; set; }
    string Name { get; set; }
    string Namespace { get; set; }
    Accessibility Accessibility { get; set; }
    TypeDeclarationSyntax TypeDeclaration { get; set; }
    Location Location { get; set; }
    ITypeSymbol TypeSymbol { get; set; }
    List<(Location location, IDiagnosticMessage message)> DiagnosticMessages { get; }
    bool IsMain { get; set; }
    bool IsDefinitionValid { get; }
    RelationStrategy RelationStrategy { get; set; }
    List<IPropertyMetadata> Properties { get; set; }

    string ToClassString();
}
