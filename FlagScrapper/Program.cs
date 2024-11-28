using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

public class Program
{
    public static async Task Main(string[] args)
    {
        string connectionString = "";
        string databaseName = "";
        string flagsFolder = @"";
        
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        
        var gridFS = new GridFSBucket(database);
        
        var countryCollection = database.GetCollection<BsonDocument>("Country");
        var countries = await countryCollection.Find(new BsonDocument()).ToListAsync();

        foreach (var country in countries)
        {
            string abbreviation = country["Abbreviation"].AsString.ToLower();
            string id = country["_id"].AsObjectId.ToString();

            // Search for a file with the same name as the abbreviation in the Flags folder
            string imagePath = Path.Combine(flagsFolder, $"{abbreviation}.png");
            if (File.Exists(imagePath))
            {
                Console.WriteLine($"Found flag for {abbreviation}, uploading to GridFS...");
                
                using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    // Define metadata for the file
                    var options = new GridFSUploadOptions
                    {
                        Metadata = new BsonDocument
                        {
                            { "CountryId", id }
                        }
                    };
                    
                    ObjectId fileId = await gridFS.UploadFromStreamAsync($"{abbreviation}.png", fs, options);

                    Console.WriteLine($"Uploaded {abbreviation}.png with file ID: {fileId}");
                }
            }
            else
                Console.WriteLine($"No flag found for {abbreviation}");
        }

        Console.WriteLine("Operation completed.");
    }
}
