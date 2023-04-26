using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDto.Tests.SourceGeneration.Models;

public class TypeWithNonPublicProperties
{
    public int Id { get; set; }
    public string Name { get; set; }

    private string PrivateProp { get; set; }
    protected string ProtectedProp { get; set; }
    internal string InternalProp { get; set; }
}

public class TypeWithReadOnlyProperty
{
    public int Id { get; set; }
    public string Name { get; set; }

    public string ReadOnlyProp { get; }
}

public class TypeWithSetOnlyProperty
{
    public int Id { get; set; }
    public string Name { get; set; }

    public string SetOnlyProp 
    { 
        set { } 
    }

    public string PrivateReadPublicSetProp { private get; set; }
}

public class TypeWithFields
{
    public int Id { get; set; }
    public string Name { get; set; }

#pragma warning disable CS0169 // The field 'TypeWithFields._privateField' is never used
    private string _privateField;
#pragma warning restore CS0169 // The field 'TypeWithFields._privateField' is never used
    protected string _protectedField;
#pragma warning disable CS0649 // Field 'TypeWithFields._internalField' is never assigned to, and will always have its default value null
    internal string _internalField;
#pragma warning restore CS0649 // Field 'TypeWithFields._internalField' is never assigned to, and will always have its default value null
    public string _publicField;
}

public class TypeWithMethods
{
    public int Id { get; set; }
    public string Name { get; set; }

    private void PrivateMethod() { }
    protected void ProtectedMethod() { }
    internal void InternalMethod() { }
    public void PublicMethod() { }
}

public class TypeWithStaticProperies
{
    public int Id { get; set; }
    public string Name { get; set; }

    private static string PrivateProp { get; set; }
    protected static string ProtectedProp { get; set; }
    internal static string InternalProp { get; set; }
    public static string PublicProp { get; set; }
}

public class TypeWithStaticFields
{
    public int Id { get; set; }
    public string Name { get; set; }

#pragma warning disable CS0169 // The field 'TypeWithStaticFields._privateField' is never used
    private static string _privateField;
#pragma warning restore CS0169 // The field 'TypeWithStaticFields._privateField' is never used
    protected static string _protectedField;
#pragma warning disable CS0649 // Field 'TypeWithStaticFields._internalField' is never assigned to, and will always have its default value null
    internal static string _internalField;
#pragma warning restore CS0649 // Field 'TypeWithStaticFields._internalField' is never assigned to, and will always have its default value null
    public static string _publicField;
}

public class TypeWithStaticMethods
{
    public int Id { get; set; }
    public string Name { get; set; }

    private static void PrivateMethod() { }
    protected static void ProtectedMethod() { }
    internal static void InternalMethod() { }
    public static void PublicMethod() { }
}

public class TypeWithConsts
{
    public int Id { get; set; }
    public string Name { get; set; }

    private const string PRIVATE_CONST = "";
    protected const string PROTECTED_CONST = "";
    internal const string INTERNAL_CONST = "";
    public const string PUBLIC_CONST = "";
}

