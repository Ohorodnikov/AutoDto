using AutoDto.SourceGen.DiagnosticMessages;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoDto.SourceGen.DtoAttributeData;

internal interface IAttributeDataReader
{
    bool TryRead(AttributeData attribute, out IDtoAttributeData attributeData);
}

internal class AttributeDataReader : IAttributeDataReader
{
    private readonly IAttributeDataFactory _dataFactory;

    public AttributeDataReader(IAttributeDataFactory dataFactory)
    {
        _dataFactory = dataFactory;
    }

    public bool TryRead(AttributeData attribute, out IDtoAttributeData attributeData)
    {
        attributeData = null;
        if (attribute == null)
            return false;

        attributeData = _dataFactory.Get(attribute);

        if (attributeData == null)
            return false;

        attributeData.Location = attribute.ApplicationSyntaxReference.GetSyntax().GetLocation();

        var ctorArgs = attribute.ConstructorArguments;

        for (int i = 0; i < ctorArgs.Length; i++)
            attributeData.InitOneValue(i, ctorArgs[i]);

        return true;
    }
}


