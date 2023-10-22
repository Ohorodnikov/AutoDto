using AutoDto.Setup;
using AutoDto.SourceGen;
using AutoDto.Tests.TestHelpers;
using AutoDto.Tests.TestHelpers.CodeBuilder.Builders;
using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoDto.Tests.SourceGeneration;

public class LoadTestFixture : IDisposable
{
    public List<ClassElement> BlClasses { get; private set; }
    public List<ClassElement> DtoClasses { get; private set; }
    public List<ClassElement> ExtraClasses { get; private set; }

    public CSharpCompilation Compilation { get; private set; }

    public LoadTestFixture()
    {
        var blModelsCount = 1000;
        var extraTypesCount = 10_000;

        BlClasses = CreateBlModels(blModelsCount);
        ExtraClasses = CreateBlModels(extraTypesCount);
        DtoClasses = CreateDtoModels(BlClasses);

        var generator = new GeneratorRunner();

        var allRefs = generator.GetSystemRefs()
               .Union(generator.GetCommonRefs());

        SyntaxTree CreateFile(List<ClassElement> classes)
        {
            var fileContent = string.Join(Environment.NewLine, classes.Select(x => x.GenerateCode()));
            return CSharpSyntaxTree.ParseText(fileContent);
        }

        var inputSourceTrees = new[]
            {
                CreateFile(BlClasses),
                CreateFile(DtoClasses),
                CreateFile(ExtraClasses),
            };

        inputSourceTrees =
                      BlClasses.Select(x => CSharpSyntaxTree.ParseText(x.GenerateCode()))
            .Union(DtoClasses.Select(x => CSharpSyntaxTree.ParseText(x.GenerateCode())))
            .Union(ExtraClasses.Select(x => CSharpSyntaxTree.ParseText(x.GenerateCode())))
            .ToArray()
            ;

        Compilation = CSharpCompilation.Create(
            "MyCompilation",
            syntaxTrees: inputSourceTrees,
            references: allRefs,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        generator.Compile(Compilation); //to see compile errs if any in code

    }

    public void Dispose()
    {

    }

    private List<ClassElement> CreateDtoModels(List<ClassElement> blTypes)
    {
        var res = new List<ClassElement>(blTypes.Count * 3);

        foreach (var blType in blTypes)
        {
            var dtosPerBl = Random.Shared.Next(5);
            var blName = blType.Name;

            for (int i = 0; i < dtosPerBl; i++)
            {
                var dtoBuilder = new DtoClassBuilder(blName + "_Dto_" + i, DtoClassBuilder.DtoAttributeType.DtoFrom, blName);

                dtoBuilder
                    .SetNamespace("AutoDto.Tests.SourceGeneration.Models.Dtos")
                    .AddUsing("System")
                    .AddUsing("System.Collections")
                    .AddUsing("System.Collections.Generic")
                    .AddUsing("AutoDto.Tests.SourceGeneration.Models")
                    .AddUsing("AutoDto.Setup");

                if (i == 0 && dtosPerBl > 1)
                    dtoBuilder.AddAttribute(typeof(DtoMainAttribute));

                dtoBuilder.SetRelationStrategy((RelationStrategy)(Random.Shared.Next(1, 4)));

                res.Add(dtoBuilder.Build());
            }
        }

        return res;
    }

    private List<ClassElement> CreateBlModels(int count)
    {
        var res = new List<ClassElement>(count);

        var createdTypes = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var className = CreateUniqueName();
            createdTypes.Add(className);
            res.Add(GenerateClass(className, createdTypes));
        }

        return res;
    }

    private string CreateUniqueName()
    {
        return "N" + Guid.NewGuid().ToString("N");
    }

    private ClassElement GenerateClass(string className, List<string> referenceTypes)
    {
        var propsCount = Random.Shared.Next(5, 15);

        var propTypes = GetBaseTypes().Union(referenceTypes).ToList();

        var classBuilder = new ClassBuilder(className)
            .SetNamespace("AutoDto.Tests.SourceGeneration.Models")
            .AddUsing("System")
            .AddUsing("System.Collections")
            .AddUsing("System.Collections.Generic")
            .AddAttribute("Serializable")
            .As<ClassBuilder>()
            ;

        for (int i = 0; i < propsCount; i++)
        {
            var propType = propTypes[Random.Shared.Next(propTypes.Count)];

            if (Random.Shared.Next(10) < 4)
                propType = CreateCollectionType(propType);

            var propBuilder = new PropertyBuilder(CreateUniqueName(), propType);

            classBuilder.AddMember(propBuilder.Build());
        }

        return classBuilder.Build();
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

[CollectionDefinition("Load collection")]
public class LoadTestCollection : ICollectionFixture<LoadTestFixture>
{

}

[Collection("Load collection")]
public class LoadTests : BaseUnitTest
{
    private readonly LoadTestFixture _testFixture;

    public LoadTests(LoadTestFixture testFixture)
    {
        _testFixture = testFixture;
    }

    [Fact]
    [Trait("Category", "Load")]
    public void CreateNDtos()
    {
        var driver = CSharpGeneratorDriver.Create(new[] { new DtoFromBlGenerator(true) });

        driver.RunGeneratorsAndUpdateCompilation(_testFixture.Compilation, out var outputCompilation, out var diagnostics);

        Assert.Equal(_testFixture.Compilation.SyntaxTrees.Length + _testFixture.DtoClasses.Count, outputCompilation.SyntaxTrees.Count());
    }
}
