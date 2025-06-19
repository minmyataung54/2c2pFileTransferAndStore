using Microsoft.Extensions.Configuration;
using _2c2pFileTransferAndStore.Utils;
using System;
using System.IO;
namespace _2c2pFileTransferAndStore;

public class ConfigSettings
{
    public string ID { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string destinationPath { get; set; } = null!;
    public string destinationBucket { get; set; } = null!;
    public bool IsEncryptEnabled { get; set; }
}
public class AWSSettings
{
    public string AccessKey { get; set; } 
    public string SecretKey { get; set; } 
    public string Region { get; set; } 
}

class Program
{
    static async Task Main(string[] args)
    {
        //Config file reading and binding to config section
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var ConfigSection = config.GetRequiredSection("Companyconfig");
        ConfigSettings configSetting = new ConfigSettings();
        ConfigSection.Bind(configSetting);

        //var AWSSection = config.GetRequiredSection("AWS");
        var AWSSection = config.GetRequiredSection("AWS");
        AWSSettings awsSettings = new AWSSettings();
        AWSSection.Bind(awsSettings);


        Console.WriteLine("Company Name: " + configSetting.CompanyName);
        Console.WriteLine("ID: " + configSetting.ID);
        Console.WriteLine("Destination Path: " + configSetting.destinationPath);
        Console.WriteLine("Destination Bucket: " + configSetting.destinationBucket);
        Console.WriteLine("Is Encryption Enabled: " + configSetting.IsEncryptEnabled);

        Console.WriteLine("Enter the file path to transfer and store:");
        string filePath = Console.ReadLine();
        Console.WriteLine($"The file folder that u entered is: {filePath}");

        FileReader fileReader = new FileReader(filePath);
        fileReader.DisplayFileSummary();

        var s3Service = new S3service();

        await s3Service.TestConnection();
        bool BucketExists = await s3Service.EnsureBucketExists(configSetting.destinationBucket);
        if (BucketExists)
        {
            Console.WriteLine($"Bucket {configSetting.destinationBucket} already exists");

        }
        else
        {
            Console.WriteLine($"Bucket {configSetting.destinationBucket} does not exist, creating it now...");
            bool bucketCreated = await s3Service.CreateBucketAsync(configSetting.destinationBucket, awsSettings.Region);
            if(bucketCreated)
            {
                Console.WriteLine($"Bucket {configSetting.destinationBucket}created successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to create bucket {configSetting.destinationBucket}.");
            }
        }
        Console.WriteLine($"Testing Uploading file service");
        bool testFolder= await s3Service.DoesFolderExists(configSetting.destinationBucket, configSetting.destinationPath);
        
        if (testFolder)
        {
            Console.WriteLine($"Folder {configSetting.destinationBucket}/{configSetting.destinationPath} already exists in the bucket.");
        }
        else
        {
            Console.WriteLine($"Folder does not exist in the bucket");
            await s3Service.CreateFoldersAsync(configSetting.destinationBucket, configSetting.destinationPath);
            
            
        }
        //string LocalfileName = Path.GetFileName(filePath);
        //Console.WriteLine($"Local file name is {LocalfileName}");
        //await s3Service.UploadFileAsync(configSetting.destinationBucket, configSetting.destinationPath, filePath);

        int totalFiles = fileReader.GetFileCount();
        if (totalFiles > 1)
        {
            for(var i = 0; i < totalFiles; i++)
            {
                var file = fileReader.GetFiles()[i];
                Console.WriteLine($"File {i + 1} : {file.Name}");
                await s3Service.UploadFileAsync(configSetting.destinationBucket, configSetting.destinationPath, file.FullName);
            }
        }
        
            
    }
    
        
}
