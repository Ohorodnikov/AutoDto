using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoDto.SourceGen;
using AutoDto.Setup;
using System.Diagnostics;

namespace AutoDto.Tests.SourceGeneration;

public class LoadTests : BaseUnitTest
{
    [Fact]
    [Trait("Category", "Load")]
    public void CreateNDtos()
    {
        var blModelsCount = 1000;
        var extraTypesCount = 10_000;
        var bls = CreateBlModels(blModelsCount, out var createdTypes);

        var extraTypes = CreateBlModels(extraTypesCount, out _);

        var dtos = CreateDtoModels(createdTypes, out var createdDtos);

        var systemAssemblyRefs = Generator.GetSystemRefs();
        var commonRefs = Generator.GetCommonRefs();

        var allRefs = systemAssemblyRefs
            .Union(commonRefs)
            ;

        var compilation = CSharpCompilation.Create(
            "MyCompilation",
            syntaxTrees: new[]
            {
                CSharpSyntaxTree.ParseText(bls),
                CSharpSyntaxTree.ParseText(dtos),
                CSharpSyntaxTree.ParseText(extraTypes),
            },
            references: allRefs,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Generator.Compile(compilation); //to see compile errs if any in code

        var driver = CSharpGeneratorDriver.Create(new[] { new DtoFromBlGenerator() });

        var sw = new Stopwatch();
        sw.Start();
        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        sw.Stop();

        Assert.Equal(3 + createdDtos, outputCompilation.SyntaxTrees.Count());
    }

    private string CreateDtoModels(List<string> blTypes, out int createdDtos)
    {
        createdDtos = 0;
        var sb = new StringBuilder();

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using AutoDto.Tests.SourceGeneration.Models.Generated;");
        sb.AppendLine("using AutoDto.Setup;");
        sb.AppendLine();

        sb.AppendLine("namespace AutoDto.Tests.SourceGeneration.Models.Dtos.Generated;");

        sb.AppendLine();
        //var dtos = new List<DtoData>(blTypes.Count);

        foreach (var blType in blTypes)
        {

            var dtosPerBl = Random.Shared.Next(5);

            for (int i = 0; i < dtosPerBl; i++)
            {
                sb.AppendLine();

                createdDtos++;

                if (i == 0 && dtosPerBl > 1)
                {
                    var mainAttr = typeof(DtoMainAttribute).Name.Replace(nameof(Attribute), "");
                    sb.Append("[").Append(mainAttr).Append("]").AppendLine();
                }

                var name = typeof(DtoFromAttribute).Name.Replace(nameof(Attribute), "");

                var strategy = (RelationStrategy)(Random.Shared.Next(1, 4));

                sb
                    .Append("[")
                    .Append(name)
                    .Append("(typeof(")
                    .Append(blType)
                    .Append("), ")
                    .Append(nameof(RelationStrategy))
                    .Append(".")
                    .Append(strategy)
                    .Append(")]")
                    .AppendLine()
                    ;

                sb.Append("public partial class ").Append(blType).Append("_Dto").Append(i).AppendLine("{ }");
            }
        }

        return sb.ToString();
    }

    private string CreateBlModels(int count, out List<string> createdTypes)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();

        sb.AppendLine("namespace AutoDto.Tests.SourceGeneration.Models.Generated;");

        createdTypes = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var className = CreateUniqueName();
            createdTypes.Add(className);
            GenerateClass(className, createdTypes, sb);
        }

        return sb.ToString();
    }

    private string CreateUniqueName()
    {
        return "N" + Guid.NewGuid().ToString("N");
    }

    private StringBuilder GenerateClass(string className, List<string> referenceTypes, StringBuilder sb)
    {
        var propsCount = Random.Shared.Next(5, 15);

        var propTypes = GetBaseTypes();
        propTypes.AddRange(referenceTypes);

        sb.AppendLine();

        sb.AppendLine("[Serializable]");

        sb.Append("public class ").AppendLine(className);
        sb.AppendLine("{");

        for (int i = 0; i < propsCount; i++)
        {
            var propType = propTypes[Random.Shared.Next(propTypes.Count)];

            if (Random.Shared.Next(10) < 4)
                propType = CreateCollectionType(propType);

            sb.Append("     ").Append("public ").Append(propType).Append(" ").Append(CreateUniqueName()).Append(" { get; set; }").AppendLine().AppendLine();
        }

        sb.AppendLine("}");

        return sb;
    }

    private string CreateCollectionType(string elementName)
    {
        var collTypes = CollectionTypes();

        var type = collTypes[Random.Shared.Next(collTypes.Count)];

        if (type == "[]")
            return elementName + "[]";

        var typeName = type.Substring(0, type.IndexOf('`'));

        return $"{typeName}<{elementName}>";

    }

    private List<string> CollectionTypes()
    {
        return new List<string>
        {
            "[]",
            typeof(IEnumerable<>).FullName,
            typeof(List<>).FullName,
            typeof(HashSet<>).FullName,
        };
    }

    private List<string> GetBaseTypes()
    {
        return new List<Type>()
        {
            typeof(string),
            typeof(int),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(DateTime)
        }
        .Select(x => x.FullName)
        .ToList();
    }

     
}
