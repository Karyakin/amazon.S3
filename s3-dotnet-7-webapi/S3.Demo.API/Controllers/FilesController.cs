using System.ComponentModel.DataAnnotations;
using Amazon.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using S3.Demo.API.Models;

namespace S3.Demo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        //private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;

        public FilesController(/*IAmazonS3 s3Client,*/ IConfiguration configuration)
        {
            //_s3Client = s3Client;
            _configuration = configuration;
        }


        [HttpPost]
        public async Task<IActionResult> UploadFileAsync(IFormFile file, string bucketName,[Required] string? prefix)
        {
            var accessKey = _configuration.GetValue<string>("AWS:AccessKey");
            var secretKey = _configuration.GetValue<string>("AWS:SecretKey");
            
            var client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
            {
                ServiceURL = _configuration.GetSection("AWS:ServiceURL").Value,
            });

            switch (bucketName)
            {
                case "presents":
                    bucketName = DateTime.UtcNow.Year + "-" + BucketsName.PRESENTS;
                    break;
                case "avatars":
                    bucketName = DateTime.UtcNow.Year + "-" + BucketsName.AVATARS;
                    break;
                case "promo":
                    bucketName = DateTime.UtcNow.Year + "-" + BucketsName.PROMO;
                    break;
                case "categories":
                    bucketName = DateTime.UtcNow.Year + "-" + BucketsName.CATEGORIES;
                    break;
                default:
                    throw new ArgumentException("Incorrect buckets name!");
            }
            
            
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName);

            if (!bucketExists)
            {
               var putResult = await client.PutBucketAsync(bucketName);
            }
            
            /*if (!bucketExists) 
                return NotFound($"Bucket {bucketName} does not exist.");*/

            var fileName = $"{Guid.NewGuid()}.jpg";
            
            var putObjectRequest = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = string.IsNullOrEmpty(prefix) ? fileName : $"{prefix?.TrimEnd('/')}/{fileName}",
                InputStream = file.OpenReadStream()
            };
            putObjectRequest.Metadata.Add("Content-Type", file.ContentType);
            await client.PutObjectAsync(putObjectRequest);
            
            
            
            var request1 = new ListObjectsV2Request()
            {
                BucketName = bucketName,
                Prefix = prefix
            };
            var result = await client.ListObjectsV2Async(request1);
            var s3Objects = result.S3Objects.Select(s =>
            {
                var urlRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketName,
                    Key = s.Key,
                    Expires = DateTime.UtcNow.AddYears(5)
                };
                return new S3ObjectDto()
                {
                    Name = s.Key.ToString(),
                    PresignedUrl = client.GetPreSignedURL(urlRequest),
                    
                };
            });
            
            return Ok(s3Objects.Select(x=>x.PresignedUrl).FirstOrDefault());
            
            
            
            //return Ok($"File {prefix}/{file.FileName} uploaded to S3 successfully!");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFilesAsync(string bucketName, string? prefix)
        {
            var accessKey = _configuration.GetValue<string>("AWS:AccessKey");
            var secretKey = _configuration.GetValue<string>("AWS:SecretKey");
            var client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
            {
                ServiceURL = _configuration.GetSection("AWS:ServiceURL").Value,
            });
            
            
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName);
            if (!bucketExists) 
                return NotFound($"Bucket {bucketName} does not exist.");
            
            var request = new ListObjectsV2Request()
            {
                BucketName = bucketName,
                Prefix = prefix
            };
            var result = await client.ListObjectsV2Async(request);
            var s3Objects = result.S3Objects.Select(s =>
            {
                var urlRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketName,
                    Key = s.Key,
                    Expires = DateTime.UtcNow.AddDays(1)
                };
                return new S3ObjectDto()
                {
                    Name = s.Key.ToString(),
                    PresignedUrl = client.GetPreSignedURL(urlRequest),
                    
                };
            });
            return Ok(s3Objects);
        }

        [HttpGet("preview")]
        public async Task<IActionResult> GetFileByKeyAsync(string bucketName, string key)
        {
            var accessKey = _configuration.GetValue<string>("AWS:AccessKey");
            var secretKey = _configuration.GetValue<string>("AWS:SecretKey");
            var client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
            {
                ServiceURL = _configuration.GetSection("AWS:ServiceURL").Value,
            });
            
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName);
            if (!bucketExists) 
                return NotFound($"Bucket {bucketName} does not exist.");
            var s3Object = await client.GetObjectAsync(bucketName, key);

            var photoUrl = File(s3Object.ResponseStream, s3Object.Headers.ContentType);
            
            
            return File(s3Object.ResponseStream, s3Object.Headers.ContentType);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFileAsync(string bucketName, string key)
        {
            var accessKey = _configuration.GetValue<string>("AWS:AccessKey");
            var secretKey = _configuration.GetValue<string>("AWS:SecretKey");
            var client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
            {
                ServiceURL = _configuration.GetSection("AWS:ServiceURL").Value,
            });
            
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName);
            
            if (!bucketExists) 
                return NotFound($"Bucket {bucketName} does not exist");
            
            await client.DeleteObjectAsync(bucketName, key);
            return NoContent();
        }

        [HttpGet("getFoldersInBucket")]
        public async Task<IActionResult> GetFoldersInBucket(string bucketName)
        {
            var accessKey = _configuration.GetValue<string>("AWS:AccessKey");
            var secretKey = _configuration.GetValue<string>("AWS:SecretKey");
            var client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
            {
                ServiceURL = _configuration.GetSection("AWS:ServiceURL").Value,
            });

           
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Delimiter = "/"
            };

            ListObjectsV2Response response;
            do
            {
                //var bucketExists = await client.ListObjectsV2Async(request);

                response = await client.ListObjectsV2Async(request);

               var aaa = response.CommonPrefixes;
                foreach (var commonPrefix in response.CommonPrefixes)
                {
                    Console.WriteLine(commonPrefix);
                }

                request.ContinuationToken = response.NextContinuationToken;
            } while (response.IsTruncated);

            return Ok(response);
        }
    }
}
