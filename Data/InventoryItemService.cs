using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.Expressions;
namespace InventoryWebApp.Data;



public class InventoryItemService
{

    string dataDocPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Inventory.csv");

    public Task<List<InventoryItem>> GetInventoryAsync()
    {
        using (var reader = new StreamReader(dataDocPath))
        using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture)){
            return Task.FromResult((csv.GetRecords<InventoryItem>()).ToList<InventoryItem>());
        }
    }
}
