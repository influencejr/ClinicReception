using System.Text;
using ClinicReception.Core.Domain;

Console.OutputEncoding = Encoding.UTF8;

var visit = new Visit("V-1001", "Іваненко");
visit.AddLine("PHYS-01", 3, 250m);
visit.AddLine("INJ-01", 12, 90m);
Console.WriteLine(visit.CalculateTotal(false));
Console.WriteLine(visit.CalculateTotal(true));
Console.WriteLine(visit.IsValid());
Console.WriteLine(visit.TryChangeStatus(Visit.StatusConfirmed));
Console.Write(visit.BuildReport());
