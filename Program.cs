using System;
using System.Linq;
using Mono.Cecil;

var dll = AssemblyDefinition.ReadAssembly(@"C:\WINDOWS\system32\config\systemprofile\.nuget\packages\imeritas.agent.contracts\1.3.0\lib\net10.0\Imeritas.Agent.Contracts.dll");

Console.WriteLine("=== INTERFACES ===");
var interfaces = dll.MainModule.Types.Where(t => t.IsInterface).OrderBy(t => t.Name);
foreach (var iface in interfaces)
{
    Console.WriteLine($"\nnamespace {iface.Namespace}");
    Console.WriteLine($"interface {iface.Name}");
    Console.WriteLine("{");
    
    foreach (var method in iface.Methods)
    {
        var retType = method.ReturnType.FullName;
        var args = string.Join(", ", method.Parameters.Select(p => $"{p.ParameterType.FullName} {p.Name}"));
        var modifiers = method.ReturnType.IsByReference ? "ref " : "";
        Console.WriteLine($"  {modifiers}{retType} {method.Name}({args});");
    }
    
    foreach (var prop in iface.Properties)
    {
        var get = prop.GetMethod != null ? "get" : "";
        var set = prop.SetMethod != null ? (string.IsNullOrEmpty(get) ? "set" : "; set") : "";
        Console.WriteLine($"  {prop.PropertyType.FullName} {prop.Name} {{ {get}{set} }}");
    }
    
    Console.WriteLine("}");
}

Console.WriteLine("\n=== ENUMS ===");
var enums = dll.MainModule.Types.Where(t => t.IsEnum).OrderBy(t => t.Name);
foreach (var en in enums)
{
    Console.WriteLine($"\nnamespace {en.Namespace}");
    Console.WriteLine($"enum {en.Name}");
    Console.WriteLine("{");
    foreach (var field in en.Fields.Where(f => f.Name != "value__"))
    {
        var value = field.Constant;
        Console.WriteLine($"  {field.Name} = {value},");
    }
    Console.WriteLine("}");
}

Console.WriteLine("\n=== CLASSES ===");
var classes = dll.MainModule.Types.Where(t => t.IsClass && t.IsPublic && !t.Name.StartsWith("<")).OrderBy(t => t.Name).Take(30);
foreach (var cls in classes)
{
    Console.WriteLine($"\nnamespace {cls.Namespace}");
    Console.WriteLine($"class {cls.Name}");
    if (cls.BaseType != null && cls.BaseType.FullName != "System.Object")
    {
        Console.WriteLine($" : {cls.BaseType.FullName}");
    }
    Console.WriteLine("{");
    
    foreach (var prop in cls.Properties)
    {
        var get = prop.GetMethod != null ? "get" : "";
        var set = prop.SetMethod != null ? (string.IsNullOrEmpty(get) ? "set" : "; set") : "";
        Console.WriteLine($"  {prop.PropertyType.FullName} {prop.Name} {{ {get}{set} }}");
    }
    
    Console.WriteLine("}");
}
