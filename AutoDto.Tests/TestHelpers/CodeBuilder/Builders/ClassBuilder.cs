using AutoDto.Tests.TestHelpers.CodeBuilder.Elements;

namespace AutoDto.Tests.TestHelpers.CodeBuilder.Builders;

public class ClassBuilder : BaseElementBuilder<ClassElement>
{
    public ClassBuilder(string name) 
        : base(name)
    {
        AddUsing("System");
    }

    protected string Namespace { get; set; }
    protected List<string> BaseClassAndInterfaces { get; } = new List<string>();

    protected List<Member> Members { get; } = new List<Member>();

    protected HashSet<string> Usings { get; } = new HashSet<string>();

    protected override ClassElement BuildImpl()
    {
        return new ClassElement(Namespace, GetBaseDeclaration(), BaseClassAndInterfaces, Members, Usings);
    }

    public ClassBuilder SetNamespace(string @namespace)
    {
        Namespace = @namespace;
        return this;
    }

    /// <summary>
    /// Add base class or interface
    /// </summary>
    /// <param name="name"></param>
    /// <param name="namespace"></param>
    /// <returns></returns>
    public ClassBuilder AddBase(string name, string @namespace = null)
    {
        BaseClassAndInterfaces.Add(name);
        AddUsing(@namespace);
        return this;
    }

    public ClassBuilder AddMember(Member member)
    {
        Members.Add(member);
        return this;
    }

    public ClassBuilder AddMembers(IEnumerable<Member> members)
    {
        foreach (var member in members)
            AddMember(member);
        
        return this;
    }

    public ClassBuilder AddUsing(string @namespace)
    {
        Usings.Add(@namespace);
        return this;
    }

    public override BaseElementBuilder<ClassElement> AddAttribute(string name, string @namespace = null, params string[] arguments)
    {
        AddUsing(@namespace);
        return base.AddAttribute(name, null, arguments);
    }
}

