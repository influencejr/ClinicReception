using System.Diagnostics.CodeAnalysis;

// A-05: придушення лише атрибутом із поясненням, директива #pragma заборонена
[assembly: SuppressMessage(
    "Design",
    "CA1031:Do not catch general exception types",
    Justification = "Верхній обробник: збій іде в журнал, а не стеком викликів на екран",
    Scope = "member",
    Target = "~M:Program.<Main>$(System.String[])")]
