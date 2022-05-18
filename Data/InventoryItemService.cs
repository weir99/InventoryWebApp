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

    // Add inventory item, probably no async needed, only adding one item
    public void Add(InventoryItem item){
        var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture){
            HasHeaderRecord = false
        };
        using (var stream = File.Open(dataDocPath, FileMode.Append))
        using (var writer = new StreamWriter(stream))
        using (var csv = new CsvWriter(writer, config)){
            csv.NextRecord();
            csv.WriteRecord(item);
        }
    }
}
