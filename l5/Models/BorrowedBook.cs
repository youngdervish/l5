using System.ComponentModel.DataAnnotations;

namespace l5.Models
{
    public class BorrowedBook
    {
        [Key]
        public int Id { get; set; } // Unique ID for the borrowing record
        public string Username { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowedDate { get; set; }
        public Book Book { get; set; }
    }
}
