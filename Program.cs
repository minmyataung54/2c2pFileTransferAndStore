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
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string Region { get; set; } = null!;
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

        if (configSetting.IsEncryptEnabled)
        {
            Console.WriteLine("Encryption is enabled. Encrypting file name...");
        }
        FileReader fileReader = new FileReader(filePath);
        
        fileReader.DisplayFileSummary();
        for (int i = 0; i < fileReader.GetFileCount(); i++)
        {
            var file = fileReader.GetFiles()[i];
            string fileName = FileEncrypt.EncryptFileName(file.Name);
            Console.WriteLine($"Encrypted File Name: {fileName}");
        }

        var s3Service = new S3service();

        await s3Service.TestConnection();
        // Testing if the bucket exists and creating it if it does not exist
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
        // Testing if the folder exists in the bucket and creating it if it does not exist
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
        //Getting the file count and uploading files by looping through the files in the local directory
        int totalFiles = fileReader.GetFileCount();
        if (totalFiles == 0)
        {
            Console.WriteLine("No files found in the specified directory.Please ensure the directory contains files.");
            return;
        }
        else if (totalFiles > 1)
        {
            for (var i = 0; i < totalFiles; i++)
            {
                var file = fileReader.GetFiles()[i];
                
                Console.WriteLine($"File {i + 1} : {file.Name}");
                if (configSetting.IsEncryptEnabled)
                {
                    await s3Service.UploadFileAsyncIncludeEncryptName(configSetting.destinationBucket, configSetting.destinationPath, file.FullName);
                }
                else
                {
                    await s3Service.UploadFileAsyncIncludeTempName(configSetting.destinationBucket, configSetting.destinationPath, file.FullName);
                }
                    
            }
            //for(var i=0; i < totalFiles; i++)
            //{
            //    var file = fileReader.GetFiles()[i];
            //    if (configSetting.IsEncryptEnabled)
            //    {
            //        var encryptFileName = FileEncrypt.EncryptFileName(file.Name);
            //        Console.WriteLine($"File {i+1} : Encrypted File Name: {encryptFileName}");
            //        await s3Service.UploadFileAsyncIncludeTempName(configSetting.destinationBucket, configSetting.destinationPath, encryptFileName);
            //    }
            //    else
            //    {
            //                           Console.WriteLine($"File {i+1} : {file.Name}");
            //        await s3Service.UploadFileAsyncIncludeTempName(configSetting.destinationBucket, configSetting.destinationPath, file.FullName);
            //    }
            //}
        }
        else
        {
            var file = fileReader.GetFiles()[0];
            Console.WriteLine($"File : {file.Name}");
            await s3Service.UploadFileAsyncIncludeTempName(configSetting.destinationBucket, configSetting.destinationPath, file.FullName);
        }
        
            
    }
    
        
}
