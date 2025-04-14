using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using l5.Data;
using l5.DTOs;
using Microsoft.EntityFrameworkCore;
using l5.Core.Models;

namespace l5.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public BooksController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet("get-books")]
        public async Task<IActionResult> GetBooks()
        {
            try
            {
                var books = await _context.Books
                    .Select(b => new Book
                    {
                        Title = b.Title,
                        Id = b.Id,
                        Author = b.Author,
                        Year = b.Year,
                        Quantity = b.Quantity// - _context.BorrowedBooks.Count(x => x.BookId == b.Id)
                        //Quantity = b.Quantity - _context.BorrowedBooks.Count(x => x.Book.Title == b.Title && x.Book.Author == b.Author)
                    }).ToListAsync();
                
                return Ok(books);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("get-borrowed-books")]
        public async Task<IActionResult> GetBorrowedBooks()
        {
            try
            {
                var username = User.Identity.Name; // Get the logged-in user's username

                // Fetch all books in the library
                var books = await _context.Books.ToListAsync();

                // Fetch the IDs of borrowed books for the current user
                var borrowedBookIds = await _context.BorrowedBooks
                    .Where(bb => bb.Username == username)
                    .Select(bb => bb.BookId)
                    .ToListAsync();

                // Combine the books and borrowed status into one object
                var booksWithStatus = books.Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Author,
                    b.Year,
                    b.Quantity,
                    IsBorrowed = borrowedBookIds.Contains(b.Id)
                }).ToList();

                return Ok(booksWithStatus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{title}")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBook(string title)
        {
            var books = await _context.Books.Where(b => b.Title.Contains(title)).ToListAsync();

            if (books == null || !books.Any()) return NotFound();

            var result = books.Select(book => new Book
            {
                Title = book.Title,
                Id = book.Id,
                Author = book.Author,
                Year = book.Year,
                Quantity = book.Quantity
            }).ToList();

            return Ok(result);
        }

        [HttpPost("add-book")]
        public async Task<ActionResult<BookDTO>> AddBook([FromBody] Book book) 
        {
            var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.Title == book.Title && b.Author == book.Author);
            if (existingBook != null) return BadRequest("The book already exists!!!");

            var result = await _context.Books.AddAsync(book);
            
            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error saving the book: {ex.Message}");
            }
        }

        [HttpPut("update-book/{oldTitle}/{oldAuthor}")]
        public async Task<IActionResult> UpdateBook(string oldTitle, string oldAuthor, [FromBody] BookDTO updatedBook)
        {
            var existingBook = await _context.Books.FirstOrDefaultAsync(b => b.Title == oldTitle && b.Author == oldAuthor);

            Console.WriteLine($"\n\nUpdated Book Title: {updatedBook.Title}\n\n");
            Console.WriteLine($"\n\nUpdated Book Author: {updatedBook.Author}\n\n");
            Console.WriteLine($"\n\nUpdated Book Year: {updatedBook.Year}\n\n");
            Console.WriteLine($"\n\nUpdated Book Quantity: {updatedBook.Quantity}\n\n");

            Console.WriteLine($"\n\nExisting Book Title: {existingBook.Title}\n\n");
            Console.WriteLine($"\n\nExisting Book Author: {existingBook.Author}\n\n");
            Console.WriteLine($"\n\nExisting Book Year: {existingBook.Year}\n\n");
            Console.WriteLine($"\n\nExisting Book Quantity: {existingBook.Quantity}\n\n");

            bool isUpdated = false;

            // Update Title if provided and if it's different
            if (!string.IsNullOrEmpty(updatedBook.Title) && updatedBook.Title != existingBook.Title)
            {
                existingBook.Title = updatedBook.Title;
                isUpdated = true;
            }

            // Update Author if provided and if it's different
            if (!string.IsNullOrEmpty(updatedBook.Author) && updatedBook.Author != existingBook.Author)
            {
                existingBook.Author = updatedBook.Author;
                isUpdated = true;
            }

            // Update Year if provided and if it's different
            if (updatedBook.Year != 0 && updatedBook.Year != existingBook.Year)
            {
                existingBook.Year = updatedBook.Year;
                isUpdated = true;
            }

            // Update Quantity - only increment or decrement allowed
            if (updatedBook.Quantity >= 0)
            {
                existingBook.Quantity = updatedBook.Quantity;
                isUpdated = true;
            }

            Console.WriteLine($"\n\nUpdate Quantity: {updatedBook.Quantity}\n\n");
            Console.WriteLine($"\n\nExisting Quantity: {existingBook.Quantity}\n\n");

            // If no changes were made, return 304 Not Modified
            if (!isUpdated)
                return StatusCode(304, "No changes were made to the book.");

            // Save changes
            try
            {
                await _context.SaveChangesAsync();
                return NoContent();  // Return 204 No Content to indicate successful update
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating the book: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBook([FromBody] List<string> titles)
        {
            var booksToDelete = _context.Books.Where(b => titles.Contains(b.Id.ToString())).ToList();
            //foreach(var book in booksToDelete) Console.WriteLine($"\n\nThe book details: {book.Title}");
            if (!booksToDelete.Any()) return NotFound("No books found with the provided titles.");

            foreach (var book in booksToDelete)
            {
                var borrowedBook = await _context.BorrowedBooks.FirstOrDefaultAsync(b => b.BookId == book.Id);
                if (borrowedBook != null) return BadRequest("Can't remove a borrowed book");
            }

            _context.Books.RemoveRange(booksToDelete);

            try
            {
                await _context.SaveChangesAsync();
                //foreach (var book in booksToDelete)   Console.WriteLine($"The deleted book id is {book.Id}");
                return NoContent();  // Return 204 No Content to indicate successful deletion
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting the books: {ex.Message}");
            }
        }

        
        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBooks([FromBody] List<string> titles)
        {
            var username = User.Identity.Name; // Get the username from JWT token

            // Fetch books
            var booksToBorrow = await _context.Books
                .Where(b => titles.Contains(b.Id.ToString()))
                .ToListAsync();
            foreach (var book in booksToBorrow)
            {
                if (book.Quantity == 0)
                {
                    return BadRequest("Some books are unavailable.");
                }
            }            

            // Check if user already borrowed any of these books
            var existingBorrowedBooks = await _context.BorrowedBooks
                .Where(bb => bb.Username == username && titles.Contains(bb.BookId.ToString()))
                .ToListAsync();

            if (existingBorrowedBooks.Any())
            {
                return BadRequest("You have already borrowed one or more of these books.");
            }

            // Borrow the books (create new BorrowedBook entries)
            var borrowedBooks = booksToBorrow.Select(b => new BorrowedBook
            {
                Username = username,
                BookId = b.Id, 
                BorrowedDate = DateTime.Now
                //Book = new Book
                //{
                //    Title = b.Title,
                //    Author = b.Author,
                //    Year = b.Year,
                //    Quantity = b.Quantity,
                //}
            }).ToList();

            _context.BorrowedBooks.AddRange(borrowedBooks);
            //Decrease the quantity of the books
            foreach (var book in booksToBorrow)
            {
                var bookInDb = await _context.Books.FirstOrDefaultAsync(b => b.Id == book.Id);
                bookInDb.Quantity--;
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Books borrowed successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error borrowing books: {ex.Message}");
            }
        }

        [HttpPost("return")]
        public async Task<IActionResult> ReturnBooks([FromBody] List<string> titles)
        {
            var username = User.Identity.Name; // Get the username from JWT token

            // Fetch books that are matching the borrowed ones to the user
            var booksToReturn = await _context.BorrowedBooks
                .Where(bb => bb.Username == username && titles.Contains(bb.BookId.ToString()))
                .ToListAsync();

            if (!booksToReturn.Any())
            {
                return BadRequest("No books found to return.");
            }

            // Remove the borrowed books from the BorrowedBooks table
            foreach (var borrowedBook in booksToReturn)
            {
                var bookInDb = await _context.Books.FirstOrDefaultAsync(b => b.Id == borrowedBook.BookId);

                if (bookInDb == null)
                {
                    // Handle case when the book is not found in the database
                    return BadRequest($"Book with ID {borrowedBook.BookId} not found.");
                }

                bookInDb.Quantity++;
                //if(borrowedBook.Username == username) 
                _context.BorrowedBooks.Remove(borrowedBook);
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Books returned successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error borrowing books: {ex.Message}");
            }
        }
    }
}
//var booksToReturn = await _context.BorrowedBooks
//    .Where(bb => bb.Username == username && titles.Contains(bb.BookId.ToString()))
//    .ToListAsync();
