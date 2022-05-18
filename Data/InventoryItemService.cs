using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.Expressions;
namespace InventoryWebApp.Data;



public class InventoryItemService
{



    public Task<IEnumerable<InventoryItem>> GetInventoryAsync()
    {
        using (var reader = new StreamReader(""))
        using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture)){
            return Task.FromResult(csv.GetRecords<InventoryItem>());
        }
    }
}
