using Amazon.Lambda.Core;

using Amazon.Rekognition;
using Amazon.Rekognition.Model;

using Newtonsoft.Json;
using ServerlessAppsForImgProcessing.Model;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ServerlessAppsForImgProcessing.functions.rekognition;

public class Function
{
    /// <summary>
    /// The default minimum confidence used for detecting labels.
    /// </summary>
    public const float DEFAULT_MIN_CONFIDENCE = 90f;

    /// <summary>
    /// The name of the environment variable to set which will override the default minimum confidence level.
    /// </summary>
    public const string MIN_CONFIDENCE_ENVIRONMENT_VARIABLE_NAME = "MinConfidence";

    IAmazonRekognition RekognitionClient { get; }

    float MinConfidence { get; set; } = DEFAULT_MIN_CONFIDENCE;

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
        this.RekognitionClient = new AmazonRekognitionClient();

        var environmentMinConfidence = System.Environment.GetEnvironmentVariable(MIN_CONFIDENCE_ENVIRONMENT_VARIABLE_NAME);
        if (!string.IsNullOrWhiteSpace(environmentMinConfidence))
        {
            float value;
            if (float.TryParse(environmentMinConfidence, out value))
            {
                this.MinConfidence = value;
            }
            else
            {
                Console.WriteLine($"Failed to parse value {environmentMinConfidence} for minimum confidence. Reverting back to default of {this.MinConfidence}");
            }
        }
        else
        {
            Console.WriteLine($"Using default minimum confidence of {this.MinConfidence}");
        }
    }

    /// <summary>
    /// Constructor used for testing which will pass in the already configured service clients.
    /// </summary>
    /// <param name="s3Client"></param>
    /// <param name="rekognitionClient"></param>
    /// <param name="minConfidence"></param>
    public Function(IAmazonRekognition rekognitionClient, float minConfidence)
    {
        this.RekognitionClient = rekognitionClient;
        this.MinConfidence = minConfidence;
    }

    /// <summary>
    /// A function for responding to S3 create events. It will determine if the object is an image and use Amazon Rekognition
    /// to detect labels and add the labels as tags on the S3 object.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<Response> FunctionHandler(object obj, ILambdaContext context)
    {
        var evresp = JsonConvert.DeserializeObject<EventNotification>(obj.ToString());

        if (!SupportedImageTypes.Contains(Path.GetExtension(evresp.Detail.Object.Key)))
        {
            Console.WriteLine($"Object {evresp.Detail.Bucket.Name}:{evresp.Detail.Object.Key} is not a supported image type");
            return null;
        }

        Console.WriteLine($"Looking for labels in image {evresp.Detail.Bucket.Name}:{evresp.Detail.Object.Key}");

        var detectResponses = await this.RekognitionClient.DetectLabelsAsync(new DetectLabelsRequest
        {
            MinConfidence = MinConfidence,
            Image = new Image
            {
                S3Object = new S3Object
                {
                    Bucket = evresp.Detail.Bucket.Name,
                    Name = evresp.Detail.Object.Key
                }
            }
        });

        var tags = new Dictionary<string, string>();
        foreach (var label in detectResponses.Labels)
        {
            tags.Add(label.Name, label.Confidence.ToString());
        }

        var response = new Response
        {
            Image = evresp.Detail.Object.Key,
            Labels = JsonConvert.SerializeObject(tags)
        };

        Console.WriteLine($"response:  {response}");

        return response;
    }
}

public class Response
{
    public string Image { get; set; }
    public string Labels { get; set; }
}

