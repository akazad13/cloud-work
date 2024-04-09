using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using AWSS3Connect.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AWSS3Connect.Utilities
{
    public class Helper : IHelper
    {
        private readonly ConfigModel _configModel;
        public Helper(IOptions<ConfigModel> configModel)
        {
            _configModel = configModel.Value;
        }
        public async Task<bool> FileUploadonAmazonS3(int bookid, string data)
        {
            AmazonS3Client s3Client = new AmazonS3Client(_configModel.AmazonPublicKey, _configModel.AmazonSecretKey, RegionEndpoint.USEast2);
            var putRequest = new PutObjectRequest
            {
                BucketName = _configModel.AWSBucketName,
                Key = bookid + ".json",
                ContentBody = data,
                ContentType = "Application/json"
            };
            try
            {
                var response2 = await s3Client.PutObjectAsync(putRequest);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<string[]> ReadObjectDataAsync(int bookId)
        {
            string[] ret = new string[2];
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _configModel.AWSBucketName,
                    Key = bookId + ".json"
                };

                AmazonS3Client s3Client = new AmazonS3Client(_configModel.AmazonPublicKey, _configModel.AmazonSecretKey, RegionEndpoint.USEast2);

                using GetObjectResponse response = await s3Client.GetObjectAsync(request);
                using Stream responseStream = response.ResponseStream;
                using StreamReader reader = new StreamReader(responseStream);
                ret[0] = reader.ReadToEnd();
                ret[1] = "Success";
            }
            catch (AmazonS3Exception e)
            {
                ret[0] = e.Message;
                ret[1] = "Failed";
            }
            catch (Exception e)
            {
                ret[0] = e.Message;
                ret[1] = "Failed";
            }

            return ret;
        }

        public async Task<List<Book>> ReadAllObjectDataAsync()
        {
            AmazonS3Client s3Client = new AmazonS3Client(_configModel.AmazonPublicKey, _configModel.AmazonSecretKey, RegionEndpoint.USEast2);

            var response = await s3Client.ListObjectsAsync(_configModel.AWSBucketName);
            List<Book> books = new List<Book>();
            foreach (S3Object entry in response.S3Objects)
            {
                var bookId = entry.Key?.Replace(".json", "");
                var ret = await ReadObjectDataAsync(int.Parse(bookId));
                if (ret[1] == "Success")
                {
                    books.Add(JsonConvert.DeserializeObject<Book>(ret[0]));
                }
                
            }

            return books;

        }

        public async Task<bool> IsObjectExist(int bookId)
        {
            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _configModel.AWSBucketName,
                    Key = bookId + ".json"
                };

                AmazonS3Client s3Client = new AmazonS3Client(_configModel.AmazonPublicKey, _configModel.AmazonSecretKey, RegionEndpoint.USEast2);

                using GetObjectResponse response = await s3Client.GetObjectAsync(request);
                using Stream responseStream = response.ResponseStream;
                using StreamReader reader = new StreamReader(responseStream);

                var res = reader.ReadToEnd();

                if (res != null) return true;
                return false;
            }
            catch (AmazonS3Exception ex)
            {
                return false;
            }

            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAWSS3Object(int bookId)
        {
            try
            {
                AmazonS3Client s3Client = new AmazonS3Client(_configModel.AmazonPublicKey, _configModel.AmazonSecretKey, RegionEndpoint.USEast2);

                var response = await s3Client.DeleteObjectAsync(_configModel.AWSBucketName, bookId + ".json");
                return true;
            }
            catch (AmazonS3Exception e)
            {
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}
