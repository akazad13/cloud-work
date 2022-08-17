using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;

using Amazon.S3;
using Amazon.S3.Model;
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
    public const float DEFAULT_MIN_CONFIDENCE = 90f;

    IAmazonS3 S3Client { get; }


    HashSet<string> SupportedImageTypes { get; } = new HashSet<string> { ".png", ".jpg", ".jpeg" };

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
    }

    /// <summary>
    /// Constructor used for testing which will pass in the already configured service clients.
    /// </summary>
    /// <param name="s3Client"></param>

    public Function(IAmazonS3 s3Client)
    {
        this.S3Client = s3Client;
    }

    /// <summary>
    /// A function for responding to S3 create events. It will determine if the object is an image and use Amazon Rekognition
    /// to detect labels and add the labels as tags on the S3 object.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(S3Event input, ILambdaContext context)
    {
        foreach (var record in input.Records)
        {
            if (!SupportedImageTypes.Contains(Path.GetExtension(record.S3.Object.Key)))
            {
                Console.WriteLine($"Object {record.S3.Bucket.Name}:{record.S3.Object.Key} is not a supported image type");
                continue;
            }

            Console.WriteLine($"Looking for labels in image {record.S3.Bucket.Name}:{record.S3.Object.Key}");

            var s3Bucket = record.S3.Bucket.Name;
            var srcKey = record.S3.Object.Key;

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
                            BucketName = "lab4-image-resized",
                            Key = $"thumbnail-{srcKey}",
                            InputStream = m
                        };
                        await S3Client.PutObjectAsync(putRequest);
                        Console.WriteLine($"Uploaded");
                    }

                }
            }
        }
        return;
    }
}
