using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.EventStreams;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using System;




namespace _2c2pFileTransferAndStore.Utils
{
    public class S3service
    {
        private AmazonS3Client s3Client;
        public class AWSSettings
        {
            public string AccessKey { get; set; } 
            public string SecretKey { get; set; }
            public string Region { get; set; }
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
       
        public async Task TestConnection()
        {
            if (s3Client == null)
            {
                Console.WriteLine("S3 Client is not initialized.");
                return;
            }

            try
            {
                Console.WriteLine("Testing connection to S3...");

                //var response = await s3Client.ListBucketsAsync();
                var response = await s3Client.ListBucketsAsync();
                Console.WriteLine("Connection to S3 is successful");
                //foreach(var bucket in response.Buckets)
                //{
                //    Console.WriteLine($"- {bucket.BucketName}");
                //}
                Console.WriteLine($"Total Buckets: {response.Buckets.Count}");
                foreach(var bucket in response.Buckets)
                {
                    Console.WriteLine($"Bucket Name: {bucket.BucketName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
            }
        }
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
        public async Task<bool> EnsureBucketExists(string bucketName)
        {
            try
            {
                var response = await s3Client.ListBucketsAsync();
                if(response.Buckets.Any(b => b.BucketName.Equals(bucketName, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine($"Bucket '{bucketName}' already exists.");
                    return true;
                }else
                {
                    Console.WriteLine($"Bucket '{bucketName}' does not exist.");
                    return false;
                }

            }catch (Exception ex)
            {
                Console.WriteLine($"Error checking bucket existence: {ex.Message}");
                return false;
            }
        }
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
    }
    
}
