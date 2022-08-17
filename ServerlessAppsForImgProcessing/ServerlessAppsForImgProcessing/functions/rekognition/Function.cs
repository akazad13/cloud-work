using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;


using Amazon.Rekognition;
using Amazon.Rekognition.Model;

using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ServerlessAppsForImgProcessing.functions.rekognition;

public class Function
{
    /// <summary>
    /// The default minimum confidence used for detecting labels.
    /// </summary>
    public const float DEFAULT_MIN_CONFIDENCE = 90f;
    public const string DEFAULT_DYNAMODB_TABLENAME = "lab4-test-stack-ImageLabels-1UH4ZKYRS2U4R";

    /// <summary>
    /// The name of the environment variable to set which will override the default minimum confidence level.
    /// </summary>
    public const string MIN_CONFIDENCE_ENVIRONMENT_VARIABLE_NAME = "MinConfidence";
    public const string DYNAMODB_TABLENAME_ENVIRONMENT_VARIABLE_NAME = "DynamoDBTableName";

    IAmazonS3 S3Client { get; }

    IAmazonRekognition RekognitionClient { get; }

    IAmazonDynamoDB DynamoDBClient { get; }

    float MinConfidence { get; set; } = DEFAULT_MIN_CONFIDENCE;
    string DynamoDBTableName { get; set; } = DEFAULT_DYNAMODB_TABLENAME;

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
        this.RekognitionClient = new AmazonRekognitionClient();
        this.DynamoDBClient = new AmazonDynamoDBClient();

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

        var environmentDynamoDBTable = System.Environment.GetEnvironmentVariable(DYNAMODB_TABLENAME_ENVIRONMENT_VARIABLE_NAME);
        if (!string.IsNullOrEmpty(environmentDynamoDBTable))
        {

            this.DynamoDBTableName = environmentDynamoDBTable;
        }
        else
        {
            Console.WriteLine($"Using default DynamoDB Table name of {this.DynamoDBTableName}");
        }
    }

    /// <summary>
    /// Constructor used for testing which will pass in the already configured service clients.
    /// </summary>
    /// <param name="s3Client"></param>
    /// <param name="rekognitionClient"></param>
    /// <param name="minConfidence"></param>
    public Function(IAmazonS3 s3Client, IAmazonRekognition rekognitionClient, IAmazonDynamoDB amazonDynamoDB, float minConfidence)
    {
        this.S3Client = s3Client;
        this.RekognitionClient = rekognitionClient;
        this.DynamoDBClient = amazonDynamoDB;
        this.MinConfidence = minConfidence;
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
            var detectResponses = await this.RekognitionClient.DetectLabelsAsync(new DetectLabelsRequest
            {
                MinConfidence = MinConfidence,
                Image = new Image
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object
                    {
                        Bucket = record.S3.Bucket.Name,
                        Name = record.S3.Object.Key
                    }
                }
            });

            var tags = new Dictionary<string, string>();
            foreach (var label in detectResponses.Labels)
            {
                tags.Add(label.Name, label.Confidence.ToString());
            }

            Console.WriteLine($"Table name = {this.DynamoDBTableName}");

            var request = new PutItemRequest
            {
                TableName = this.DynamoDBTableName,
                Item = new Dictionary<string, AttributeValue>()
                {
                    { "Image", new AttributeValue { S = record.S3.Object.Key } },
                    { "Labels", new AttributeValue { S =  JsonConvert.SerializeObject(tags) }
                    }
                }
            };
            var response = await DynamoDBClient.PutItemAsync(request);
            Console.WriteLine($"Inserted into the table");
        }
        return;
    }
}