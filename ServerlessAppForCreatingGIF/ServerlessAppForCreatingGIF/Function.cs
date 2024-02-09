using Amazon.Lambda.Core;

using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using ServerlessAppForCreatingGIF.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ServerlessAppForCreatingGIF;

public class Function
{
    /// <summary>
    /// The default minimum confidence used for detecting labels.
    /// </summary>
    public const string DEFAULT_INPUT_BUCKET = "input-images-3451";
    public const string DEFAULT_OUTPUT_BUCKET = "output-images-3451";

    public const string INPUT_BUCKET_ENVIRONMENT_VARIABLE_NAME = "InputBucketName";
    public const string OUTPUT_BUCKET_ENVIRONMENT_VARIABLE_NAME = "OutputBucketName";

    IAmazonS3 S3Client { get; }


    HashSet<string> SupportedImageTypes { get; } = new HashSet<string> { ".png", ".jpg", ".jpeg" };

    string InnputBucketName { get; set; } = DEFAULT_INPUT_BUCKET;
    string OutputBucketName { get; set; } = DEFAULT_OUTPUT_BUCKET;

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
        var environmentInputBucketName = System.Environment.GetEnvironmentVariable(INPUT_BUCKET_ENVIRONMENT_VARIABLE_NAME);
        if (!string.IsNullOrWhiteSpace(environmentInputBucketName))
        {
            this.InnputBucketName = environmentInputBucketName;
        }
        else
        {
            Console.WriteLine($"Using default input bucket of {this.InnputBucketName}");
        }

        var environmentOutputBucketName = System.Environment.GetEnvironmentVariable(OUTPUT_BUCKET_ENVIRONMENT_VARIABLE_NAME);
        if (!string.IsNullOrWhiteSpace(environmentOutputBucketName))
        {
            this.OutputBucketName = environmentOutputBucketName;
        }
        else
        {
            Console.WriteLine($"Using default output bucket of {this.OutputBucketName}");
        }
    }

    /// <summary>
    /// Constructor used for testing which will pass in the already configured service clients.
    /// </summary>
    /// <param name="s3Client"></param>

    public Function(IAmazonS3 s3Client, string inputBucketName, string outputBucketName)
    {
        this.S3Client = s3Client;
        this.InnputBucketName = inputBucketName;
        this.OutputBucketName = outputBucketName;
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
        try
        {
            if(obj == null)
            {
                Console.WriteLine($"Nothing!");
                return;
            }
            var req = JsonConvert.DeserializeObject<Request>(obj.ToString());

            if(req.queryStringParameters == null)
            {
                Console.WriteLine($"No folder provided!");
                return;
            }

            Console.WriteLine($"obj: {req?.queryStringParameters["foldername"]}");

            string? foldername = req?.queryStringParameters["foldername"];
            if (string.IsNullOrEmpty(foldername))
            {
                Console.WriteLine($"No folder provided!");
                return;
            }
            Console.WriteLine($"Converting the images in {this.InnputBucketName}/{foldername}");

            var s3Bucket = this.InnputBucketName;

            var response = (await S3Client.GetAllObjectKeysAsync(s3Bucket, $"{foldername}/", null))?.Skip(1)?.ToList();

            if (response == null)
            {
                Console.WriteLine($"No image in the folder!");
                return;
            }

            Console.WriteLine($"Keys {response.Count()}");

            // Image dimensions of the gif.
            const int width = 150;
            const int height = 150;

            // Delay between frames in (1/100) of a second.
            const int frameDelay = 20;

            // Create empty image.
            using Image gif = new Image<Rgba32>(width, height);

            // Set animation loop repeat count to 5.
            var gifMetaData = gif.Metadata.GetGifMetadata();
            gifMetaData.RepeatCount = 5;

            // Set the delay until the next image is displayed.
            GifFrameMetadata metadata = gif.Frames.RootFrame.Metadata.GetGifMetadata();
            metadata.FrameDelay = frameDelay;

            for (int i = 0; i < response.Count; i++)
            {
                //Console.WriteLine($"Fetching Object {response[i]}");
                if (!SupportedImageTypes.Contains(Path.GetExtension(response[i])?.ToLower()))
                {
                    Console.WriteLine($"Object {response[i]} is not a supported image type");
                }
                else
                {
                    using (var resp = await S3Client.GetObjectAsync(s3Bucket, response[i]))
                    {
                        using (Stream responseStream = resp.ResponseStream)
                        {
                            var image = await Image.LoadAsync(responseStream);

                            image.Mutate(x => x.Resize(width, height));
                            metadata = image.Frames.RootFrame.Metadata.GetGifMetadata();
                            metadata.FrameDelay = frameDelay;

                            gif.Frames.AddFrame(image.Frames.RootFrame);

                            image.Dispose();
                        }
                    }
                }
            }

            using (MemoryStream m = new())
            {
                await gif.SaveAsGifAsync(m);
                gif.Dispose();
                PutObjectRequest putRequest = new()
                {
                    BucketName = this.OutputBucketName,
                    Key = $"{foldername}_{DateTime.Now.Ticks}.gif",
                    InputStream = m
                };
                await S3Client.PutObjectAsync(putRequest);
                Console.WriteLine($"Uploaded successfully");
            }
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"exception: {ex}");
            return;
        }
    }
}