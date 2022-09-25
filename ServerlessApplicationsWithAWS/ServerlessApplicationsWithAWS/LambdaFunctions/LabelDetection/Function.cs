
using Amazon.Lambda.Core;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Newtonsoft.Json;
using ServerlessApplicationsWithAWS.Models;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ServerlessApplicationsWithAWS.LambdaFunctions.LabelDetection
{
    public class Function
    {
        public const string MIN_CONFIDENCE_ENV_VAR = "MinConfidence";
        float MinConfidence { get; set; } = 90f;

        IAmazonRekognition RekognitionClient { get; }

        HashSet<string> ValidImageTypeToProcess { get; } = new HashSet<string> { ".png", ".jpg", ".jpeg" };

        public Function()
        {
            this.RekognitionClient = new AmazonRekognitionClient();

            var envMinConf = Environment.GetEnvironmentVariable(MIN_CONFIDENCE_ENV_VAR);
            if (!string.IsNullOrWhiteSpace(envMinConf))
            {
                if (float.TryParse(envMinConf, out float value))
                {
                    this.MinConfidence = value;
                }
                else
                {
                    Console.WriteLine($"Failed to parse value {envMinConf} for minimum confidence. Using the default confidence of {this.MinConfidence}");
                }
            }
            else
            {
                Console.WriteLine($"Using default minimum confidence of {this.MinConfidence}");
            }
        }


        public Function(IAmazonRekognition rekognitionClient, float minConfidence)
        {
            this.RekognitionClient = rekognitionClient;
            this.MinConfidence = minConfidence;
        }

        public async Task<LambdaResponse> FunctionHandler(object req, ILambdaContext context)
        {
            var data = JsonConvert.DeserializeObject<EventBridgeResponse>(req.ToString());

            string bucketName = data.Detail.Bucket.Name;
            string imageName = data.Detail.Object.Key;

            if (!ValidImageTypeToProcess.Contains(Path.GetExtension(imageName)))
            {
                Console.WriteLine($"Object {bucketName}:{imageName} is not a supported image type");
                return null;
            }

            var rekogResponses = await this.RekognitionClient.DetectLabelsAsync(new DetectLabelsRequest
            {
                MinConfidence = MinConfidence,
                Image = new Image
                {
                    S3Object = new S3Object
                    {
                        Bucket = bucketName,
                        Name = imageName
                    }
                }
            });

            var extractedTags = new Dictionary<string, string>();
            foreach (var label in rekogResponses.Labels)
            {
                extractedTags.Add(label.Name, label.Confidence.ToString());
            }

            var response = new LambdaResponse
            {
                Image = imageName,
                Labels = JsonConvert.SerializeObject(extractedTags)
            };

            Console.WriteLine($"Labels of image {imageName}:{response.Labels}");

            return response;
        }
    }
}
