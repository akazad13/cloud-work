using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using ServerlessApplicationsWithAWS.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace ServerlessApplicationsWithAWS.LambdaFunctions.ThumbnailCreate
{
    public class Function
    {
        string ThumbnailBucketName { get; set; } = "";
        public const string THUMBNAIL_BUCKET_ENV_VAR = "ThumbnailBucketName";

        IAmazonS3 S3Client { get; }

        HashSet<string> ValidImageTypeToProcess { get; } = new HashSet<string> { ".png", ".jpg", ".jpeg" };

       
        public Function()
        {
            this.S3Client = new AmazonS3Client();
            var envBucketName = System.Environment.GetEnvironmentVariable(THUMBNAIL_BUCKET_ENV_VAR);
            if (!string.IsNullOrWhiteSpace(envBucketName))
            {
                this.ThumbnailBucketName = envBucketName;
            }
            else
            {
                Console.WriteLine($"Using default thumbnail bucket of {this.ThumbnailBucketName}");
            }
        }

        

        public Function(IAmazonS3 s3Client, string thumbnailBucketName)
        {
            this.S3Client = s3Client;
            this.ThumbnailBucketName = thumbnailBucketName;
        }

       
        public async Task FunctionHandler(object req, ILambdaContext context)
        {
            var data = JsonConvert.DeserializeObject<EventBridgeResponse>(req.ToString());

            string bucketName = data.Detail.Bucket.Name;
            string imageName = data.Detail.Object.Key;

            if (!ValidImageTypeToProcess.Contains(Path.GetExtension(imageName)))
            {
                Console.WriteLine($"Object {bucketName}:{imageName} is not a supported image type");
                return;
            }

            Console.WriteLine($"Start looking for labels in the image {bucketName}:{imageName}");


            using (var response = await S3Client.GetObjectAsync(bucketName, imageName))
            {
                using (Stream responseStream = response.ResponseStream)
                {
                    (Image Image, IImageFormat Format) image;
                    image = await Image.LoadWithFormatAsync(responseStream);

                    image.Image.Mutate(x => x.Resize(200, 200));
                    Console.WriteLine($"Image has been resized, uploading to {bucketName}");
                    using (MemoryStream m = new MemoryStream())
                    {
                        await image.Image.SaveAsync(m, image.Format);
                        PutObjectRequest putRequest = new PutObjectRequest()
                        {
                            BucketName = this.ThumbnailBucketName,
                            Key = $"thumbnail_{imageName}",
                            InputStream = m
                        };
                        await S3Client.PutObjectAsync(putRequest);
                        
                        Console.WriteLine($"Uploaded to {bucketName}");
                    }
                }
            }
            return;
        }
    }
}
