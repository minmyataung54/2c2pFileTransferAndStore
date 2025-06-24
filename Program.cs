using Microsoft.Extensions.Configuration;
using _2c2pFileTransferAndStore.Utils;
using System;
using System.IO;
namespace _2c2pFileTransferAndStore;

public class ConfigSettings
{
    //public string ID { get; set; } = null!;
    //public string CompanyName { get; set; } = null!;
    private string _encryptlocalsourcePath = string.Empty;
    public string encryptlocalsourcePath
    {
        get => _encryptlocalsourcePath;
        set
        {
            Console.WriteLine(value);
            Console.WriteLine(string.IsNullOrEmpty(value));
            if (string.IsNullOrEmpty(value))
            {
                
                Console.WriteLine("Warning: Empty path provided");
                Console.WriteLine("A valid encrypt source path is required. Exiting program.");
                Environment.Exit(1);
            }
            _encryptlocalsourcePath = value;
        }
    }
    private string _noencryptlocalsourcePath = string.Empty;
    public string noencryptlocalsourcePath
    {
        get => _noencryptlocalsourcePath;
        set
        {
            if(string.IsNullOrEmpty(value))
            {
                Console.WriteLine("Warning: Empty path provided");
                Console.WriteLine("A valid no encrypt source path is required. Exiting program.");
                Environment.Exit(1);
            }
            _noencryptlocalsourcePath = value;
        }
    }
    private string _destinationPath = string.Empty;
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
    private string _destinationBucket = string.Empty;
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
    
    private string _AccessKey { get; set; } = string.Empty;
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
    private string _SecretKey { get; set; } = string.Empty;
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

    private string _Region { get; set; } = string.Empty;
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
        Console.WriteLine("Encrypt Local Source Path: " + configSetting.encryptlocalsourcePath);
        Console.WriteLine("NO Encrypt Local Source Path: " + configSetting.noencryptlocalsourcePath);
        Console.WriteLine("Destination Path: " + configSetting.destinationPath);
        Console.WriteLine("Destination Bucket: " + configSetting.destinationBucket);
        //Console.WriteLine("Is Encryption Enabled: " + configSetting.IsEncryptEnabled);
        //bool EncryptEnabled;

        //var filePath = configSetting.encryptlocalsourcePath;
        //var noencryptfilePath = configSetting.noencryptlocalsourcePath;
        //if(filePath.EndsWith("Encrypt"))
        //{
        //    EncryptEnabled = true;
        //}
        //else if(filePath.EndsWith("No-encrypt"))
        //{
        //     EncryptEnabled = false;
        //}
        //else
        //{
        //    Console.WriteLine("Warning: Invalid file path provided. Please ensure the path ends with either 'Encrypt' or 'No-encrypt'.");
        //    Environment.Exit(1);
        //}
        //Console.WriteLine($"The file folder that u entered is: {encryptfilePath}");

        //if (configSetting.IsEncryptEnabled)
        //{
        //    Console.WriteLine("Encryption is enabled. Encrypting file name...");
        //}
        //FileReader fileReader = new FileReader(encryptfilePath);

        //fileReader.DisplayFileSummary();
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
            if (bucketCreated)
            {
                Console.WriteLine($"Bucket {configSetting.destinationBucket}created successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to create bucket {configSetting.destinationBucket}.");
                Environment.Exit(1);
            }
        }


        // Testing if the folder exists in the bucket and creating it if it does not exist
        Console.WriteLine($"Testing Uploading file service");
        bool testFolder = await s3Service.DoesFolderExists(configSetting.destinationBucket, configSetting.destinationPath);

        if (testFolder)
        {
            Console.WriteLine($"Folder {configSetting.destinationBucket}/{configSetting.destinationPath} already exists in the bucket.");
        }
        else
        {
            Console.WriteLine($"Folder does not exist in the bucket");
            await s3Service.CreateFoldersAsync(configSetting.destinationBucket, configSetting.destinationPath);

        }
        string encryptFilePath = configSetting.encryptlocalsourcePath;
        string noencryptFilePath = configSetting.noencryptlocalsourcePath;
        //FileReader fileReader = new FileReader(encryptFilePath);
        //bool IsFolderEncrypt = fileReader.FolderEndsWithEncrypt();
        //FileReader noencryptfileReader = new FileReader(configSetting.noencryptlocalsourcePath);

        //Getting the file count and uploading files by looping through the files in the local directory
        int totalFiles = 0;
        int successCount = 0;
        int encryptCount = 0;
        int not_encryptCount = 0;

        var pathsToProcess = new[]
        {
            new{ Path = configSetting.encryptlocalsourcePath, IsEncrypt = true },
            new{ Path = configSetting.noencryptlocalsourcePath, IsEncrypt = false}
        };

        foreach (var pathInfo in pathsToProcess)
        {
            FileReader fileReader = new FileReader(pathInfo.Path);
            int fileCount = fileReader.GetFileCount();
            totalFiles += fileCount;

            if (fileCount > 0)
            {
                Console.WriteLine($"Total Files {fileCount} processing from {pathInfo.Path}");
                for (var i = 0; i < fileCount; i++)
                {
                    var file = fileReader.GetFiles()[i];
                    if (pathInfo.IsEncrypt)
                    {
                        bool FileTransfered = await s3Service.UploadFileAsyncIncludeEncryptName(configSetting.destinationBucket, configSetting.destinationPath, file.FullName);
                        if (FileTransfered)
                        {
                            successCount++;
                            encryptCount++;
                        }
                    }
                    else
                    {
                        bool FileTransfered = await s3Service.UploadFileAsyncIncludeTempName(configSetting.destinationBucket, configSetting.destinationPath, file.FullName);
                        if (FileTransfered)
                        {
                            successCount++;
                            not_encryptCount++;
                        }
                    }
                }
            }
            else if (fileCount == 0)
            {
                Console.WriteLine("No files found in the specified directory.Please ensure the directory contains files.");

            }
        }

        //if (totalFiles == 0)
        //{
        //    Console.WriteLine("No files found in the specified directory.Please ensure the directory contains files.");
        //    return;
        //}
        //else if (totalFiles >= 1)
        //{
        //    for (var i = 0; i < totalFiles; i++)
        //    {

        //        var file = fileReader.GetFiles()[i];

        //        Console.WriteLine($"File {i + 1} : {file.Name}");
        //        if (IsFolderEncrypt)
        //        {
        //            bool FileTransfered = await s3Service.UploadFileAsyncIncludeEncryptName(configSetting.destinationBucket, configSetting.destinationPath, file.FullName);
        //            if (FileTransfered)
        //            {
        //                successCount++;
        //                encryptCount++;
        //            }
        //        }
        //        else
        //        {
        //            bool FileTransfered = await s3Service.UploadFileAsyncIncludeTempName(configSetting.destinationBucket, configSetting.destinationPath, file.FullName);
        //            if (FileTransfered)
        //            {
        //                successCount++;
        //                not_encryptCount++;
        //            }

        //        }

        //    }
    
        
        
        int failedcount = totalFiles - successCount;
        Console.WriteLine($"Total number of files to upload: {totalFiles}");
        Console.WriteLine($"Total number of files uploaded successfully: {successCount}");
        Console.WriteLine($"Total number of files failed to upload: {failedcount}");
        Console.WriteLine("Total number of files encrypted and uploaded: " + encryptCount);
        Console.WriteLine("Total number of files not encrypted and uploaded: " + not_encryptCount);
    }

}
