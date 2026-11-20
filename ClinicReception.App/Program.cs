using System.Text;
using ClinicReception.Core.Domain;

Console.OutputEncoding = Encoding.UTF8;

var z = new vizyt("V-1001", "Іваненко");
z.Add("PHYS-01", 3, 250m);
z.Add("INJ-01", 12, 90m);
Console.WriteLine(z.ProcessData(false));
Console.WriteLine(z.ProcessData(true));
Console.WriteLine(z.Ck());
Console.WriteLine(z.Chg(1));
Console.Write(z.Rep());
