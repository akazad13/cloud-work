using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;

using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using ServerlessAppsForImgProcessing.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ServerlessAppsForImgProcessing.functions.Thumbnail;

public class Function
{
    /// <summary>
    /// The default minimum confidence used for detecting labels.
    /// </summary>
    public const string DEFAULT_THUMBNAIL_BUCKET = "lab4-stack-thambnailimagebucket-11i3c725058x8";

    public const string THUMBNAIL_BUCKET_ENVIRONMENT_VARIABLE_NAME = "ThumbnailBucketName";

    IAmazonS3 S3Client { get; }


    HashSet<string> SupportedImageTypes { get; } = new HashSet<string> { ".png", ".jpg", ".jpeg" };

    string ThumbnailBucketName { get; set; } = DEFAULT_THUMBNAIL_BUCKET;

    /// <summary>
    /// Default constructor used by AWS Lambda to construct the function. Credentials and Region information will
    /// be set by the running Lambda environment.
    /// 
    /// This constuctor will also search for the environment variable overriding the default minimum confidence level
    /// for label detection.
    /// </summary>
    public Function()
    {
        this.S3Client = new AmazonS3Client();
        var environmentBucketName = System.Environment.GetEnvironmentVariable(THUMBNAIL_BUCKET_ENVIRONMENT_VARIABLE_NAME);
        if (!string.IsNullOrWhiteSpace(environmentBucketName))
        {
                this.ThumbnailBucketName = environmentBucketName;
        }
        else
        {
            Console.WriteLine($"Using default thumbnail bucket of {this.ThumbnailBucketName}");
        }
    }

    /// <summary>
    /// Constructor used for testing which will pass in the already configured service clients.
    /// </summary>
    /// <param name="s3Client"></param>

    public Function(IAmazonS3 s3Client, string thumbnailBucketName)
    {
        this.S3Client = s3Client;
        this.ThumbnailBucketName = thumbnailBucketName;
    }

    /// <summary>
    /// A function for responding to S3 create events. It will determine if the object is an image and use Amazon Rekognition
    /// to detect labels and add the labels as tags on the S3 object.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(object obj, ILambdaContext context)
    {
        var evresp = JsonConvert.DeserializeObject<EventNotification>(obj.ToString());

        if (!SupportedImageTypes.Contains(Path.GetExtension(evresp.Detail.Object.Key)))
        {
            Console.WriteLine($"Object {evresp.Detail.Bucket.Name}:{evresp.Detail.Object.Key} is not a supported image type");
            return;
        }

        Console.WriteLine($"Looking for labels in image {evresp.Detail.Bucket.Name}:{evresp.Detail.Object.Key}");

        var s3Bucket = evresp.Detail.Bucket.Name;
        var srcKey = evresp.Detail.Object.Key;


        using (var response = await S3Client.GetObjectAsync(s3Bucket, srcKey))
        {
            using (Stream responseStream = response.ResponseStream)
            {
                (Image Image, IImageFormat Format) image;
                image = await Image.LoadWithFormatAsync(responseStream);

                Console.WriteLine($"Start resizing");

                image.Image.Mutate(x => x.Resize(200, 200));
                Console.WriteLine($"Resized, Uploading to output bucket");
                using (MemoryStream m = new MemoryStream())
                {
                    await image.Image.SaveAsync(m, image.Format);
                    PutObjectRequest putRequest = new PutObjectRequest()
                    {
                        BucketName = this.ThumbnailBucketName,
                        Key = $"thumbnail-{srcKey}",
                        InputStream = m
                    };
                    await S3Client.PutObjectAsync(putRequest);
                    Console.WriteLine($"Uploaded");
                }
            }
        }

        return;
    }
}