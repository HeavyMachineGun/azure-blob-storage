using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Collections;
using System.Linq;
// See https://aka.ms/new-console-template for more information

Console.WriteLine("Azure Blob Storage excersise!");
ProcessAsync().GetAwaiter().GetResult();

Console.WriteLine("Azure Blob Storage excersise!");
Console.ReadLine();

    
static async Task ProcessAsync()
{

    string connectionString = "";//your connection string
    string cointainerName = "wtblob-test-files";
    BlobServiceClient serviceClient = new(connectionString);
    BlobContainerClient containerClient;
    containerClient =  serviceClient.GetBlobContainerClient(cointainerName);
    
    var notExits = !await containerClient.ExistsAsync();
    if(notExits)
    {
        containerClient = await serviceClient.CreateBlobContainerAsync(cointainerName);
        Console.WriteLine($"A container named {cointainerName} has been created. ");
    }
    
    await UploadBlobToContainerAsync(null,containerClient);
    var blobsNames = await GetListContainerBlobsAsync(containerClient);
    await DownloadFileAsync(blobsNames,containerClient);
    
    Console.WriteLine("Would you like to delete container? \n Yes\t[1]\n No\t[2]");
    var input = Console.ReadLine();
    var choice = Convert.ToInt64(input);
    if(choice==1)
    {
        await DeleteContainerAsync(containerClient);
        //delete
    }
}

static async Task UploadBlobToContainerAsync(string? fileName, BlobContainerClient containerClient)
{

    fileName ??= "wtfile" + Guid.NewGuid().ToString() +".txt";
    string localFilePath = Path.Combine(TestBlobFiles.LocalPath,fileName);

    await File.WriteAllTextAsync(localFilePath,"This is Nestor test local storage account test, uploading Fucking files");
    BlobClient blobClient= containerClient.GetBlobClient(fileName);
    Console.WriteLine("Uploading to Blob Storage as blob:\n\t {0} \n", blobClient.Uri);

    using FileStream uploadFileStream = File.OpenRead(localFilePath);
    await blobClient.UploadAsync(uploadFileStream,true);
    
    Console.WriteLine("The file was uploaded. We'll verify by listing the blobs next ");
    Console.WriteLine("Press 'Enter' to continue..");
    Console.ReadLine();
}

static async Task<List<string>> GetListContainerBlobsAsync(BlobContainerClient containerClient)
{
    List<string> result = new();
    Console.WriteLine("Listing blobs...");
    
    await foreach(var blob in containerClient.GetBlobsAsync())
    {
        result.Add(blob.Name);
        Console.WriteLine($"\t{blob.Name}");
    }
    
    Console.WriteLine("Press 'Enter' to continue..");
    Console.ReadLine();
    return result;
}

static async Task DownloadFileAsync(List<string> blobsNames, BlobContainerClient containerClient)
{
    var tasks = blobsNames.Select(async fileName =>{
        
        var downloadFilePath = Path.Combine(TestBlobFiles.LocalPath,fileName.Replace(".txt","-Downloaded.txt"));
        
        Console.WriteLine("\n Downloading Blob to \n\t{0}\n",downloadFilePath);
        
        BlobClient blobCLient = containerClient.GetBlobClient(fileName);
        BlobDownloadInfo download = await blobCLient.DownloadAsync();
        
        using(FileStream stream= File.OpenWrite(downloadFilePath))
        {
            await download.Content.CopyToAsync(stream);
        }

        Console.WriteLine("\nLocate the local file in the data directory created earlier to verify it was downloaded.");
    });
    await Task.WhenAll(tasks).ConfigureAwait(true);
}

static async Task DeleteContainerAsync(BlobContainerClient containerClient)
{
     Console.WriteLine("Deleting Blob Container");
     await containerClient.DeleteAsync();
     
     Console.WriteLine("Finished cleaning up.");
}
public static class TestBlobFiles
{
    public static string LocalPath = "./data";
}
