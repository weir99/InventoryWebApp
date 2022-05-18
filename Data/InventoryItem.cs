namespace InventoryWebApp.Data;

public class InventoryItem
{
    public int Id { get; set; }
    
    public string? Name { get; set; }

    public bool Deleted { get; set; }
}
