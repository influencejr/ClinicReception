# ClinicReception

Навчальний проєкт з дисципліни «Конструювання програмного забезпечення».
Варіант № 4: запис пацієнтів на прийом до лікаря. Консольний застосунок
створює запис на прийом, додає до нього послуги й рахує вартість і тривалість
візиту.

## Склад рішення

- `ClinicReception.Core` — логіка предметної області (бібліотека класів):
  пацієнт, лікар, запис на прийом, послуга, стани запису.
- `ClinicReception.App` — консольний застосунок (введення й виведення).

## Вимоги

.NET SDK 8.0 або новіший, Git.

## Складання та запуск

```bash
git clone https://github.com/influencejr/ClinicReception.git
cd ClinicReception
dotnet build
dotnet run --project ClinicReception.App
```

## Стандарт кодування

Правила описано у файлі [CODING_STANDARD.md](CODING_STANDARD.md) і закріплено
в `.editorconfig`, `Directory.Build.props` та аналізаторах Roslyn і StyleCop.
Перевірка:

```bash
dotnet format --verify-no-changes
dotnet build
```

## Автор

Вовк Давід Олександрович, група 471
