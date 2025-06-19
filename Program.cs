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
            //Console.WriteLine($"Bucket {configSetting.destinationBucket} already exists");
            
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
        //await s3Service.UploadFileSync(configSetting.destinationBucket, filePath, configSetting.destinationPath);
    }
        
}
