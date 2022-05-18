using CsvHelper.Configuration.Attributes;
namespace InventoryWebApp.Data;


public class InventoryItem
{
    [Name("id")]
    public int Id { get; set; }
    
    [Name("name")]
    public string? Name { get; set; }

    [Name("quantity")]
    public int Quantity { get; set; }

    [Name("deleted")]
    public bool Deleted { get; set; } = false;
}
