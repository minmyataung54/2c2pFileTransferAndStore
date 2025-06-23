using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.EventStreams;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using System;
using System.Runtime.CompilerServices;


namespace _2c2pFileTransferAndStore.Utils
{
    public class S3service
    {
        private AmazonS3Client s3Client;
        public class AWSSettings
        {
            public string AccessKey { get; set; } = null!;
            public string SecretKey { get; set; } = null!;
            public string Region { get; set; } = null!;
        }
        public S3service()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var AWSSection = config.GetRequiredSection("AWS");
            AWSSettings awsSettings = new AWSSettings();
            AWSSection.Bind(awsSettings);

            string accessKey = awsSettings.AccessKey ;
            string secretKey = awsSettings.SecretKey;
            string region = awsSettings.Region;

            var awsCredentials = new BasicAWSCredentials(accessKey, secretKey);
            s3Client = new AmazonS3Client(awsCredentials, RegionEndpoint.GetBySystemName(region));

            Console.WriteLine("S3 Client initialized successfully.");
        }
        //Check if the s3 client is initialized and test the connection to S3, and list the buckets in the S3.
        public async Task TestConnection()
        {
            try
            {
                Console.WriteLine("Testing connection to S3...");

                //var response = await s3Client.ListBucketsAsync();
                var response = await s3Client.ListBucketsAsync();
                Console.WriteLine("Connection to S3 is successful");
                if (response.Buckets == null || response.Buckets.Count == 0) 
                    Console.WriteLine("No buckets found in S3.") ;
                //foreach(var bucket in response.Buckets)
                //{
                //    Console.WriteLine($"- {bucket.BucketName}");
                //}
                else
                {
                    Console.WriteLine($"Total Buckets: {response.Buckets.Count}");
                    foreach (var bucket in response.Buckets)
                    {
                        Console.WriteLine($"Bucket Name: {bucket.BucketName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
            }
        }
        //Check if the folder exists in the bucket, return true if it exists and false if it doesn't 
        public async Task<bool> DoesFolderExists(string bucketName, string destinationPath)
        {
            ListObjectsResponse response = null;
            try
            {
                ListObjectsRequest request = new ListObjectsRequest
                {
                    BucketName = bucketName,
                    Prefix = destinationPath
                };
                response = await s3Client.ListObjectsAsync(request);
            }catch (Exception ex)
            {
                Console.WriteLine($"Error checking folder existence: {ex.Message}");
            }
            return (response != null && response.S3Objects != null );
        }
        //Folder creation in s3 based on the config file destinationPath
        public async Task<bool> CreateFoldersAsync(string bucketName, string folderPath)
        {
            Console.WriteLine("folder is creating");
            try
            {
                if (string.IsNullOrEmpty(bucketName))
                {
                    Console.WriteLine("Error: bucketName cannot be null or empty");
                    return false;
                }

                if (string.IsNullOrEmpty(folderPath))
                {
                    Console.WriteLine("Error: filePath cannot be null or empty");
                    return false;
                }

                if (!folderPath.EndsWith("/"))
                {
                    folderPath += "/";
                }
                Console.WriteLine($"{folderPath}");
                
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = folderPath,
                    ContentBody = ""
                };
                PutObjectResponse response = await s3Client.PutObjectAsync(request);
                Console.WriteLine($"Folder '{folderPath}' created successfully in bucket '{bucketName}'.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating folder: {ex.Message}");
                return false;
            }

        }
        //public async Task UploadDirAsync(string bucketName, string filePath)
        //{
        //    string BucketName = bucketName;
        //    string directoryPath = filePath;
        //    Console.WriteLine("Uploading folder to S3 bucket...");
        //    try
        //    {
        //        Console.WriteLine("Starting now");
        //        var directoryTransferUtility = new TransferUtility(s3Client);
        //        await directoryTransferUtility.UploadDirectoryAsync(directoryPath,
        //            BucketName);
        //        Console.WriteLine("Upload folder completed");

        //    }
        //    catch (AmazonS3Exception e)
        //    {
        //        Console.WriteLine($"Error encountered on server. Message:'{e.Message}' when writing an object");
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"Unknown encountered on server. Message:'{e.Message}' when writing an object");
        //    }
        //}
        //public async Task UploadFileSync(string bucketName, string filePath, string destinationPath)
        //{
        //    try
        //    {
        //        Console.WriteLine($"Uploading file '{filePath} on the destination folder'");
        //        var putRequest = new PutObjectRequest
        //        {
        //            BucketName = bucketName,
        //            Key = filePath,
        //            FilePath = destinationPath
        //        };
        //        PutObjectResponse response = await s3Client.PutObjectAsync(putRequest);
        //        Console.WriteLine($"File '{filePath}' uploaded to bucket '{bucketName}' successfully.");
        //    }
        //    catch(AmazonS3Exception e)
        //    {
        //        Console.WriteLine($"Error encountered ***. Message:'{0}' when writing an object", e.Message);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(
        //            "Unknown encountered on server. Message:'{0}' when writing an object"
        //            , e.Message);
        //    }
        //}
        //Check if the bucket exists, return true if it exists and false if it doesn't 
        public async Task<bool> EnsureBucketExists(string bucketName)
        {
            try
            {

                await s3Client.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    MaxKeys = 1
                });
                //Console.WriteLine($"Bucket '{bucketName}' exists.");
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                //Console.WriteLine($"Bucket '{bucketName}' does not exist.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking bucket existence: {ex.Message}");
                return false;

            }
        }
        //Create a new bucket with the specified name and region
        public async Task<bool> CreateBucketAsync(string bucketName, string region)
        {
            try
                
            {
                var request = new PutBucketRequest
                {
                    BucketName = bucketName,
                    BucketRegion = region
                };

                await s3Client.PutBucketAsync(request);
                Console.WriteLine($"Bucket '{bucketName}' created successfully in region '{region}'.");
                return true;
            
            }catch(Exception ex)
            {
                Console.WriteLine($"Error creating bucket: {ex.Message}");
                return false;
            }
        }
        //when finish uploading the file, rename it to the original filename from filename.upload
        public async Task <bool> UploadFileAsyncIncludeTempName(string bucketName, string destinationPath, string filePath)
        {
            
                if (!destinationPath.EndsWith("/"))
                {
                    destinationPath += "/";
                }
                string fileName = Path.GetFileName(filePath);
                string tempFileName = fileName + ".upload";
            try
            {
                var tempFileUploadRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = destinationPath + tempFileName,
                    FilePath = filePath,
                };
                await s3Client.PutObjectAsync(tempFileUploadRequest);

                await s3Client.CopyObjectAsync(new CopyObjectRequest
                {
                    SourceBucket = bucketName,
                    SourceKey = destinationPath + tempFileName,
                    DestinationBucket = bucketName,
                    DestinationKey = destinationPath + fileName
                });

                await s3Client.DeleteObjectAsync(bucketName, tempFileUploadRequest.Key);
                //PutObjectResponse response = await s3Client.PutObjectAsync(fileUploadRequest);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                return false;
            }
        }
        public async Task <bool>UploadFileAsyncIncludeEncryptName(string bucketName, string destinationPath, string filePath)
        {

            if (!destinationPath.EndsWith("/"))
            {
                destinationPath += "/";
            }
            string fileName = Path.GetFileName(filePath);
            string tempFileName = fileName + ".upload";
            try
            {
                var tempFileUploadRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = destinationPath + tempFileName,
                    FilePath = filePath,
                };
                await s3Client.PutObjectAsync(tempFileUploadRequest);

                await s3Client.CopyObjectAsync(new CopyObjectRequest
                {
                    SourceBucket = bucketName,
                    SourceKey = destinationPath + tempFileName,
                    DestinationBucket = bucketName,
                    DestinationKey = destinationPath + "Encrypted_"+ fileName
                });

                await s3Client.DeleteObjectAsync(bucketName, tempFileUploadRequest.Key);
                //PutObjectResponse response = await s3Client.PutObjectAsync(fileUploadRequest);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                return false;
            }
        }






    }
    
}
