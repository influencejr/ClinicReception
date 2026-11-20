# Аудит класу-Бога AppointmentManager (ЛР 4, завдання 5)

Код: `src/ClinicReception.Core/Legacy/` — `AppointmentManager`, `IAppointmentStore`,
`LegacyAppointment`, `LegacyItem`, `ArchivedAppointment`, а також допоміжні
`FileAppointmentStore` і `SmtpMailer`.

| Принцип | Доказ у коді | Наслідок | Засіб |
| --- | --- | --- | --- |
| SRP | `Place` рахує суму, ставить `Status = 1`, пише `appointments.json` через `_store.SaveToFile` і надсилає лист `_mailer.Send`; `BuildCsvReport` ще й пише `report.csv` та друкує в консоль | зміна теми листа або перехід із файлу на базу даних змушує правити клас, у якому рахується ціна прийому | розділити на `AppointmentService`, `IAppointmentRepository`, `AppointmentCsvReport` і `INotifier` |
| OCP | ланцюг `if (patientType == "regular") … else if (patientType == "insured") … else if (patientType == "staff")` у `Place` | новий тип пацієнта (наприклад, пенсіонер зі знижкою 20 %) — ще одна гілка в `Place` і повторна перевірка всього методу | `IPricingPolicy` з окремими реалізаціями, вибір політики за словником у `Program.cs` |
| LSP | `LegacyAppointment archived = new ArchivedAppointment(...)` в `Archive`, а `ArchivedAppointment.AddItem` кидає `NotSupportedException` | `Archive` падає на першій же послузі, `archive.json` не пишеться; будь-який код, що приймає `LegacyAppointment` і додає послугу (наприклад, майбутнє редагування запису), доведеться захищати перевіркою `is ArchivedAppointment` | `ArchivedAppointment` містить `Appointment` усередині й не має методу `AddItem` |
| ISP | `IAppointmentStore` має 7 методів: `Add`, `GetById`, `SaveToFile`, `SendEmail`, `ExportCsv`, `Backup`, `PrintInvoice`; `FileAppointmentStore` чотири з них заглушує `NotImplementedException` | зміна формату рахунку (`PrintInvoice`) чи резервного копіювання зачіпає кожну реалізацію сховища, зокрема тестову | вузькі `IAppointmentRepository` (3 методи) і `INotifier` (1 метод) |
| DIP | поля `_store = new FileAppointmentStore()` і `_mailer = new SmtpMailer()` | `Place` неможливо перевірити тестом без диска й SMTP-сервера; перехід на SMS-сповіщення — правка `AppointmentManager` | усі залежності — параметри конструктора у вигляді інтерфейсів |

## Дві цифри

**Причин для зміни в `AppointmentManager` — шість:**

1. правила знижок і порогу великого чека;
2. спосіб і місце зберігання записів;
3. спосіб сповіщення пацієнта (SMTP, тема, текст листа);
4. формат CSV-звіту;
5. куди виводиться звіт (файл `report.csv`, консоль);
6. правила архівування.

**Із семи методів `IAppointmentStore` класу справді потрібен один** —
зберегти запис (`SaveToFile`, після виправлення це `IAppointmentRepository.Add`).
Решта шість або не викликаються, або належать іншим відповідальностям
(`SendEmail` — сповіщенню, `ExportCsv` і `PrintInvoice` — звітам, `Backup` —
адмініструванню).

## Інші знахідки

- У `BuildCsvReport` після рядка даних немає `Environment.NewLine`, тож усі
  записи злипаються в один рядок.
- `Status = 1` — магічне число замість стану з `VisitStatus`.
- `Archive` для запису без послуг «успішно» зберігає порожній архів, а для
  запису з послугами падає: поведінка залежить від даних, а не від типу.
