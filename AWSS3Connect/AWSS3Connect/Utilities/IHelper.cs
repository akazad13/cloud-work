using AWSS3Connect.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWSS3Connect.Utilities
{
    public interface IHelper
    {
        Task<bool> FileUploadonAmazonS3(int bookid, string data);
        Task<string[]> ReadObjectDataAsync(int bookId);
        Task<bool> IsObjectExist(int bookId);
        Task<bool> DeleteAWSS3Object(int bookId);
        Task<List<Book>> ReadAllObjectDataAsync();
    }
}