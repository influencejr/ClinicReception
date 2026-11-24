# Архітектура рішення

## Шари

| Шар | Де | Що містить | Що йому заборонено |
| --- | --- | --- | --- |
| Домен | `ClinicReception.Core`, теки `Domain`, `Errors` | сутності, `TimeSlot`, `FreeSlotFinder`, стани, власний виняток | знати про файли, консоль, JSON і прикладний шар |
| Прикладний | `ClinicReception.Core`, теки `Services`, `Abstractions`, `Reporting`, `Pricing` | сценарії, інтерфейси сховища й довідника, звіти | знати про формат зберігання і консоль |
| Інфраструктура | `ClinicReception.Infrastructure` | JSON-сховище, довідник у пам'яті, журнал у файл | містити правила домену |
| Застосунок | `ClinicReception.App` | меню, барикада вводу, форматування виводу, композиційний корінь | містити розрахунки й правила |

## Залежності

```
ClinicReception.App ──> ClinicReception.Infrastructure ──> ClinicReception.Core
        └──────────────────────────────────────────────────────────^
tests ──> App, Infrastructure, Core
```

- App → Core: меню викликає сценарії й показує звіти.
- App → Infrastructure: лише композиційний корінь створює JSON-сховище й журнал.
- Infrastructure → Core: сховище реалізує `IAppointmentRepository` і
  `IClinicDirectory`, оголошені в Core.
- Core не посилається ні на що з рішення: це перевіряє компілятор (немає посилання
  в `.csproj`) і тест `ArchitectureTests.Core_ReferencedAssemblies_DoNotIncludeOuterLayer`.
- Домен усередині Core не згадує прикладний шар: тест
  `DomainTypes_PublicSignatures_UseOnlyDomainAndSystemTypes` перевіряє поля,
  властивості й сигнатури методів усіх типів `ClinicReception.Core.Domain`.

`src/Legacy` - навчальний модуль ЛР 6, до застосунку не підключений.

## Композиційний корінь

`src/ClinicReception.App/Program.cs` - єдине місце, де створюються конкретні
класи. Решта коду отримує залежності через конструктор, тож у тестах їх
підмінюють фейковим сховищем, фейковим довідником і фіксованим годинником.

```csharp
JsonClinicStore store = new(dataDirectory);
InMemoryAppointmentRepository repository = new();
store.LoadAppointments(repository);
InMemoryClinicDirectory directory = store.LoadDirectory();
AppointmentService appointments = new(
    repository, new StandardPricingPolicy(), new TextWriterNotifier(Console.Out), logs.CreateLogger<AppointmentService>());
ScheduleService schedule = new(repository, directory, appointments, TimeProvider.System);
ScheduleQueries queries = new(repository, directory);
ConsoleIo io = new(Console.In, Console.Out);
new ReceptionMenu(io, schedule, queries, logs.CreateLogger<ReceptionMenu>()).Run();
store.SaveAppointments(repository.GetAll());
```

## Точки розширення

1. Нова знижка: ще одна реалізація `IPricingPolicy` (як `DiscountPricingPolicy`),
   наявні класи не змінюються, у корені змінюється один рядок.
2. Інше сховище (база даних, CSV): реалізації `IAppointmentRepository` і
   `IClinicDirectory` в інфраструктурі; домен і сценарії не змінюються.
3. Новий пункт меню: рядок у таблиці `_items` класу `ReceptionMenu` і метод
   сценарію; цикл меню не змінюється.
