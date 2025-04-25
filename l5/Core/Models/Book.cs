using System.ComponentModel.DataAnnotations;

namespace l5.Core.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Author { get; set; }
        public int Year { get; set; }
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}

