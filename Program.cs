using Microsoft.Extensions.Configuration;
using _2c2pFileTransferAndStore.Utils;
using System;
using System.IO;
namespace _2c2pFileTransferAndStore;

public class ConfigSettings
{
    //public string ID { get; set; } = null!;
    //public string CompanyName { get; set; } = null!;
    private string _localsourcePath = null!;
    public string localsourcePath
    {
        get => _localsourcePath;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine("Warning: Empty path provided");
                Console.WriteLine("A valid destination path is required. Exiting program.");
                Environment.Exit(1);
            }
            _localsourcePath = value;
        }
    }
    private string _destinationPath = null!;
    public string destinationPath
    {
        get => _destinationPath;
        set
        {
            if(string.IsNullOrEmpty(value))
            {
                Console.WriteLine("Warning: Empty path provided");
                Console.WriteLine("A valid destination path is required. Exiting program.");
                Environment.Exit(1);
                
            }
            _destinationPath = value;
        }
    }
    private string _destinationBucket = null!;
    public string destinationBucket
    {
        get => _destinationBucket;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine("Warning: Empty path provided");
                Console.WriteLine("A valid destination path is required. Exiting program.");
                Environment.Exit(1);
            }
            _destinationBucket = value;
        }
    }
    //public string destinationPath { get; set; } = null!;
    //public string destinationBucket { get; set; } = null!;
    public bool IsEncryptEnabled { get; set; } 
}
public class AWSSettings
{
    
    private string _AccessKey { get; set; } = null!;
    public string AccessKey
    {
        get => _AccessKey;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine("Warning : Empty AWS Access Key provided");
                Console.WriteLine("A valid AWS Access Key is required. Exiting program.");
                Environment.Exit(1);
            }
            _AccessKey = value;
        }
    }
    private string _SecretKey { get; set; } = null!;
    public string SecretKey
    {
        get => _SecretKey;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine("Warning : Empty AWS Secret Key provided");
                Console.WriteLine("A valid AWS Secret Key is required. Exiting Program.");
                Environment.Exit(1);
            }
            _SecretKey = value;
        }
    }

    private string _Region { get; set; } = null!;
    public string Region
    {
        get => _Region;
        set
        {
            if(string.IsNullOrEmpty(value))
            {
                Console.WriteLine("Warning : Empty AWS Region provided");
                Console.WriteLine("A valid AWS Region is required. Exiting Program.");
                Environment.Exit(1);
            }
            _Region = value;
        }
    }
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

        var ConfigSection = config.GetRequiredSection("Company1config");
        ConfigSettings configSetting = new ConfigSettings();
        ConfigSection.Bind(configSetting);

        //var AWSSection = config.GetRequiredSection("AWS");
        var AWSSection = config.GetRequiredSection("AWS");
        AWSSettings awsSettings = new AWSSettings();
        AWSSection.Bind(awsSettings);

        //var Config2Section = config.GetRequiredSection("Company2config");
        //ConfigSettings config2Setting = new ConfigSettings();
        //ConfigSection.Bind(config2Setting);

        //for (int i = 0;  i < 2; i++)
        //{

        //}
        //Console.WriteLine("Company Name: " + configSetting.CompanyName);
        //Console.WriteLine("ID: " + configSetting.ID);
        Console.WriteLine("Destination Path: " + configSetting.destinationPath);
        Console.WriteLine("Destination Bucket: " + configSetting.destinationBucket);
        Console.WriteLine("Is Encryption Enabled: " + configSetting.IsEncryptEnabled);

        
        string filePath = configSetting.localsourcePath;
        Console.WriteLine($"The file folder that u entered is: {filePath}");

        //if (configSetting.IsEncryptEnabled)
        //{
        //    Console.WriteLine("Encryption is enabled. Encrypting file name...");
        //}
        FileReader fileReader = new FileReader(filePath);
        
        fileReader.DisplayFileSummary();
        //for (int i = 0; i < fileReader.GetFileCount(); i++)
        //{
        //    var file = fileReader.GetFiles()[i];
        //    string fileName = FileEncrypt.EncryptFileName(file.Name);
        //    Console.WriteLine($"Encrypted File Name: {fileName}");
        //}
        //Testing S3 service to check if the connection is alaredy established or not 
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
            Console.WriteLine($"Bucket region : {awsSettings.Region}");
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
        int successCount = 0;
        
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

                    bool FileTransfered = await s3Service.UploadFileAsyncIncludeEncryptName(configSetting.destinationBucket, configSetting.destinationPath, file.FullName);
                    if (FileTransfered)
                    {
                        successCount++;
                    }
                }
                else
                {
                    bool FileTransfered = await s3Service.UploadFileAsyncIncludeTempName(configSetting.destinationBucket, configSetting.destinationPath, file.FullName);
                    if (FileTransfered)
                    {
                        successCount++;
                    }
                    
                }
                    
            }
        }
        else
        {
            var file = fileReader.GetFiles()[0];
            Console.WriteLine($"File : {file.Name}");
            bool FileTransfered = await s3Service.UploadFileAsyncIncludeTempName(configSetting.destinationBucket, configSetting.destinationPath, file.FullName);
            if (FileTransfered)
            {
                successCount++;
            }
            
        }
        int failedcount = totalFiles - successCount;
        Console.WriteLine($"Total number of files to upload: {totalFiles}");
        Console.WriteLine($"Total number of files uploaded successfully: {successCount}");
        Console.WriteLine($"Total number of files failed to upload: {failedcount}");

    }
    
        
}
