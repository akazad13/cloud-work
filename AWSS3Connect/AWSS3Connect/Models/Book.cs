using System.ComponentModel.DataAnnotations;

namespace AWSS3Connect.Models
{
    public class Book
    {
        [Required]
        public int BookId { get; set; }
        [Required]
        public string BookName { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
