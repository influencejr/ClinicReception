using System.Reflection;
using ClinicReception.Core.Domain;

namespace ClinicReception.Tests;

// Правило залежностей: Core не знає ні про інфраструктуру, ні про консоль, а домен
// усередині Core не знає про прикладний шар. Перше перевіряє ще й компілятор
// (у Core немає посилань на ці проєкти), друге - лише цей тест.
public class ArchitectureTests
{
    private static readonly Assembly Core = typeof(Appointment).Assembly;

    [Theory]
    [InlineData("ClinicReception.Infrastructure")]
    [InlineData("ClinicReception.App")]
    public void Core_ReferencedAssemblies_DoNotIncludeOuterLayer(string outer)
    {
        IEnumerable<string?> names = Core.GetReferencedAssemblies().Select(a => a.Name);

        Assert.DoesNotContain(outer, names);
    }

    [Fact]
    public void DomainTypes_PublicSignatures_UseOnlyDomainAndSystemTypes()
    {
        string[] forbidden =
        {
            "ClinicReception.Core.Services",
            "ClinicReception.Core.Abstractions",
            "ClinicReception.Core.Reporting",
        };
        IEnumerable<Type> domain = Core.GetTypes()
            .Where(t => t.Namespace == "ClinicReception.Core.Domain");

        List<string> violations = domain
            .SelectMany(t => SignatureTypesOf(t).Select(used => (Owner: t, Used: used)))
            .Where(p => forbidden.Any(ns => p.Used.Namespace?.StartsWith(ns, StringComparison.Ordinal) == true))
            .Select(p => $"{p.Owner.Name} -> {p.Used.FullName}")
            .ToList();

        Assert.Empty(violations);
    }

    private static IEnumerable<Type> SignatureTypesOf(Type type)
    {
        const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.DeclaredOnly;
        IEnumerable<Type> fields = type.GetFields(all).Select(f => f.FieldType);
        IEnumerable<Type> properties = type.GetProperties(all).Select(p => p.PropertyType);
        IEnumerable<Type> methods = type.GetMethods(all)
            .SelectMany(m => m.GetParameters().Select(p => p.ParameterType).Append(m.ReturnType));
        return fields.Concat(properties).Concat(methods);
    }
}
