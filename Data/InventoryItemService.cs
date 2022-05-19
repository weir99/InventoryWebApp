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
        return await GetConditionalInventoryAsync((InventoryItem item)=> !item.Deleted);
    }

    public async Task<List<InventoryItem>> GetDeletedAsync(){
        return await GetConditionalInventoryAsync((InventoryItem item) => item.Deleted);
    }

    public async Task<List<InventoryItem>> GetConditionalInventoryAsync(Func<InventoryItem, bool> Condition){
        recordCount = 0;
        using (var reader = new StreamReader(dataDocPath))
        using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture)){
            List<InventoryItem> inventory = new List<InventoryItem>();
            IAsyncEnumerable<InventoryItem> records = csv.GetRecordsAsync<InventoryItem>();
            await foreach (var record in records){
                if(Condition(record)) inventory.Add(record); //Deleted items elsewhere
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
        item.Id = ++recordCount; // Set item ID
        inventory.Add(item);
        using (var stream = File.Open(dataDocPath, FileMode.Append))
        using (var writer = new StreamWriter(stream))
        using (var csv = new CsvWriter(writer, config)){
            csv.NextRecord();
            csv.WriteRecord(item);
        }
        return inventory;
    }

    // Update existing item, compare IDs to determine what to change
    // Compare oldItem, to expected item based off ID to ensure valid
    // 
    public async Task<List<InventoryItem>> UpdateAsync(InventoryItem oldItem, InventoryItem newItem){
        return await GetInventoryAsync();
    }
}
