namespace ClinicReception.Core.Legacy;

public class LegacyItem
{
    public string ServiceCode { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class LegacyAppointment
{
    public int Id { get; set; }
    public int Status { get; set; }
    public decimal Total { get; set; }
    public string Email { get; set; } = "";
    public List<LegacyItem> Items { get; set; } = new();

    public virtual void AddItem(LegacyItem item) => Items.Add(item);
}

public class ArchivedAppointment : LegacyAppointment
{
    public ArchivedAppointment(int id) => Id = id;

    public override void AddItem(LegacyItem item)
    {
        throw new NotSupportedException("Архів змінювати не можна");
    }
}
