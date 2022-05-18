using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.Expressions;
namespace InventoryWebApp.Data;



public class InventoryItemService
{
    private string dataDocPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Inventory.csv");
    private int recordCount; //Tracks number of records for creating IDs

    //Gets next ID to be added
    public int NextId() => recordCount + 1;
    public async Task<List<InventoryItem>> GetInventoryAsync()
    {
        using (var reader = new StreamReader(dataDocPath))
        using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture)){
            List<InventoryItem> inventory = new List<InventoryItem>();
            IAsyncEnumerable<InventoryItem> records = csv.GetRecordsAsync<InventoryItem>();
            await foreach (var record in records){
                inventory.Add(record);
                ++recordCount;
            }
            return inventory;
        }
    }

    // Add inventory item, probably no async needed, only adding one item
    public async Task<List<InventoryItem>> AddAsync(InventoryItem item){

        List<InventoryItem> inventory = await GetInventoryAsync();

        var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture){
            HasHeaderRecord = false
        };
        item.Id = recordCount; // Set item ID
        inventory.Add(item);
        using (var stream = File.Open(dataDocPath, FileMode.Append))
        using (var writer = new StreamWriter(stream))
        using (var csv = new CsvWriter(writer, config)){
            csv.NextRecord();
            csv.WriteRecord(item);
        }
        return inventory;
    }
}
