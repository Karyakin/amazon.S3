using System.Runtime.Intrinsics.X86;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace S3.Demo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BucketsController : ControllerBase
    {
        //private  IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;

        public BucketsController(/*IAmazonS3 s3Client,*/ IConfiguration configuration)
        {
            //_s3Client = s3Client;
            _configuration = configuration;
        }


        [HttpPost]
        public async Task<IActionResult> CreateBucketAsync(string bucketName)
        {
            
            var accessKey = _configuration.GetValue<string>("AWS:AccessKey");
            var secretKey = _configuration.GetValue<string>("AWS:SecretKey");
            var client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
            {
                ServiceURL = _configuration.GetSection("AWS:ServiceURL").Value,
            });
            /*var data = await client.ListBucketsAsync();
            var buckets = data.Buckets.Select(b => b.BucketName).ToList();

            if (!buckets.Any())
            {
                bucketName = "aaa";
            }*/
            
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName);
            
            if (bucketExists) 
                return BadRequest($"Bucket {bucketName} already exists.");
            
            await client.PutBucketAsync(bucketName);
            return Created("buckets", $"Bucket {bucketName} created.");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBucketAsync()
        {
            var accessKey = _configuration.GetValue<string>("AWS:AccessKey");
            var secretKey = _configuration.GetValue<string>("AWS:SecretKey");
            var client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
            {
                ServiceURL = _configuration.GetSection("AWS:ServiceURL").Value,
            });
            var data = await client.ListBucketsAsync();
            var buckets = data.Buckets.Select(b => b.BucketName);
            return Ok(buckets);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBucketAsync(string bucketName)
        {
            var accessKey = _configuration.GetValue<string>("AWS:AccessKey");
            var secretKey = _configuration.GetValue<string>("AWS:SecretKey");
            var client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
            {
                ServiceURL = _configuration.GetSection("AWS:ServiceURL").Value,
            });
            await client.DeleteBucketAsync(bucketName);
            return NoContent();
        }
        
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            var config = new AmazonS3Config
            {
                ServiceURL = "https://cmc.cloud.mts.by/"
            };
            var accessKey = _configuration.GetValue<string>("AWS:AccessKey");
            var secretKey = _configuration.GetValue<string>("AWS:SecretKey");
            
            var client = new AmazonS3Client(accessKey, secretKey, config);

            
            //var client = new AmazonS3Client(accessKey, secretKey);
            var data = await client.ListBucketsAsync();
            var buckets = data.Buckets.Select(b => b.BucketName);
        
            return Ok();
        }
        
        [HttpGet("list11")]
        public async Task<IActionResult> GetList11()
        {
            var accessKey = _configuration.GetValue<string>("AWS:AccessKey");
            var secretKey = _configuration.GetValue<string>("AWS:SecretKey");
            var client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
            {
                ServiceURL = _configuration.GetSection("AWS:ServiceURL").Value,

                
                //RegionEndpoint = RegionEndpoint.EUWest1

            });

            try
            {
                ListBucketsResponse response = await client.ListBucketsAsync();
                foreach (S3Bucket bucket in response.Buckets)
                {
                    Console.WriteLine(bucket.BucketName);
                }

                return Ok(new
                {
                    response.Buckets
                });
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown error: " + e.Message);
            }
        
            return Ok();
        }

        [HttpPost("createFolderInBucket")]
        public async Task<IActionResult> CreateFolderInBucket(string bucketName, string folderName)
        {
            try
            {
                var accessKey = _configuration.GetValue<string>("AWS:AccessKey");
                var secretKey = _configuration.GetValue<string>("AWS:SecretKey");
                using (var client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
                       {
                           ServiceURL = _configuration.GetSection("AWS:ServiceURL").Value,
                       }))
                {
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = folderName,
                        ContentBody = "йцу"
                    };
                    await client.PutObjectAsync(putRequest);
                }

                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
