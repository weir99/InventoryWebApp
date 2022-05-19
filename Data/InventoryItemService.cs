using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
namespace InventoryWebApp.Data;



public class InventoryItemService
{
    private string dataDocPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Inventory.csv");
    private string tempDocPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "tmp_Inventory.csv");
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
    // Copies data to temp file, then saves that temp file as original
    public async Task<List<InventoryItem>> UpdateAsync(InventoryItem oldItem, InventoryItem newItem){
        int targetId = oldItem.Id;
        bool changed = false;
        newItem.Id = targetId;
        using (var writer = new StreamWriter(tempDocPath))
        using (var csvW = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
        using (var reader = new StreamReader(dataDocPath))
        using (var csvR = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture)){
            IAsyncEnumerable<InventoryItem> records = csvR.GetRecordsAsync<InventoryItem>();
            csvW.WriteHeader<InventoryItem>();
            csvW.NextRecord();
            await foreach (var record in records){
                if(record.Id == targetId){
                     // Check that we are updating the right thing
                    if(record.Name != oldItem.Name || record.Quantity != oldItem.Quantity){
                        throw new ArgumentException("Object to change doesn't match given", nameof(oldItem));
                    } 
                    else{
                        csvW.WriteRecord(newItem);
                        csvW.NextRecord();
                    }
                    changed = true;
                }
                else{
                    csvW.WriteRecord(record);
                    csvW.NextRecord();
                }
            }
        }
        // Check that we updated something
        if(!changed) throw new ArgumentOutOfRangeException("Cannot find item to update", nameof(oldItem));

        // Clean up
        File.Delete(dataDocPath);
        File.Copy(tempDocPath, dataDocPath);
        File.Delete(tempDocPath);
        return await GetInventoryAsync();
    }

    // Delete existing item, compare IDs to determine what to change
    // Compare oldItem, to expected item based off ID to ensure valid
    // Copies data to temp file, then saves that temp file as original
    // Similar to update, should try to remove repetition
    public async Task<List<InventoryItem>> DeleteAsync(InventoryItem item, string? comment){
        int targetId = item.Id;
        bool changed = false;
        item.Deleted = true;
        item.DeletionComment = comment;
        using (var writer = new StreamWriter(tempDocPath))
        using (var csvW = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
        using (var reader = new StreamReader(dataDocPath))
        using (var csvR = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture)){
            IAsyncEnumerable<InventoryItem> records = csvR.GetRecordsAsync<InventoryItem>();
            csvW.WriteHeader<InventoryItem>();
            csvW.NextRecord();
            await foreach (var record in records){
                if(record.Id == targetId){
                     // Check that we are deleting the right thing
                    if(record.Name != item.Name || record.Quantity != item.Quantity){
                        throw new ArgumentException("Object to delete doesn't match given", nameof(item));
                    } 
                    else{
                        Console.WriteLine("Deleting");
                        Console.WriteLine(item.Deleted);
                        csvW.WriteRecord(item);
                        csvW.NextRecord();
                    }
                    changed = true;
                }
                else{
                    csvW.WriteRecord(record);
                    csvW.NextRecord();
                }
            }
        }
        // Check that we deleted something
        if(!changed) throw new ArgumentOutOfRangeException("Cannot find item to delete", nameof(item));

        // Finish up
        File.Delete(dataDocPath);
        File.Copy(tempDocPath, dataDocPath);
        File.Delete(tempDocPath);
        return await GetInventoryAsync();
    }
    // Undelete existing item, compare IDs to determine what to change
    // Compare oldItem, to expected item based off ID to ensure valid
    // Copies data to temp file, then saves that temp file as original
    // Similar to update, should try to remove repetition
    public async Task<List<InventoryItem>> UndeleteAsync(InventoryItem item){
        int targetId = item.Id;
        bool changed = false;
        item.Deleted = false;
        item.DeletionComment = "";
        using (var writer = new StreamWriter(tempDocPath))
        using (var csvW = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
        using (var reader = new StreamReader(dataDocPath))
        using (var csvR = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture)){
            IAsyncEnumerable<InventoryItem> records = csvR.GetRecordsAsync<InventoryItem>();
            csvW.WriteHeader<InventoryItem>();
            csvW.NextRecord();
            await foreach (var record in records){
                if(record.Id == targetId){
                     // Check that we are undeleting the right thing
                    if(record.Name != item.Name || record.Quantity != item.Quantity){
                        throw new ArgumentException("Object to undelete doesn't match given", nameof(item));
                    } 
                    else{
                        csvW.WriteRecord(item);
                        csvW.NextRecord();
                    }
                    changed = true;
                }
                else{
                    csvW.WriteRecord(record);
                    csvW.NextRecord();
                }
            }
        }
        // Check that we deleted something
        if(!changed) throw new ArgumentOutOfRangeException("Cannot find item to undelete", nameof(item));

        // Finish up
        File.Delete(dataDocPath);
        File.Copy(tempDocPath, dataDocPath);
        File.Delete(tempDocPath);
        return await GetDeletedAsync();
    }
}