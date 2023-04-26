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

        var attrName = attribute.AttributeClass.ToDisplayString();

        attributeData = _dataFactory.Get(attribute);

        if (attributeData == null)
        {
            LogHelper.Logger.Information("Attribute {attrName} is not supported", attrName);
            return false;
        }

        LogHelper.Logger.Information("Init attribute {attrName}", attrName);

        attributeData.Location = attribute.ApplicationSyntaxReference.GetSyntax().GetLocation();

        var ctorArgs = attribute.ConstructorArguments;

        for (int i = 0; i < ctorArgs.Length; i++)
        {
            var arg = ctorArgs[i];
            LogHelper.Logger.Debug("Init {i} param", i);
            attributeData.InitOneValue(i, arg);
        }

        return true;
    }
}
