using AutoMapper;
using l5.Application.DTOs;
using l5.Core.Interfaces;
using l5.Core.Models;
using l5.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace l5.Application.Services
{
    public class BookService : IBookService
    {
        //private readonly AppDbContext _context;
        public readonly IBorrowedBookRepository _borrowedBookRepository;
        public readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public BookService(IBorrowedBookRepository borrowedBookRepository, IBookRepository bookRepository, IMapper mapper)//AppDbContext context
        {
            //_context = context;
            _borrowedBookRepository = borrowedBookRepository;
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BookResponseDTO>> GetAllBooksAsync()
        {
            var books = await _bookRepository.GetAllBooksAsync();
            return books.Select(book => _mapper.Map<BookResponseDTO>(book));
        }
        public async Task<BookResponseDTO?> GetBookByIdAsync(int id)
        {
            var book = await _bookRepository.GetBookByIdAsync(id);
            return book != null ? _mapper.Map<BookResponseDTO>(book) : null;
        }
        public async Task<IEnumerable<BookResponseDTO>> SearchBooksByTitleAsync(string title)
        {
            var books = await _bookRepository.SearchBooksByNameAsync(title);
            return books.Select(book => _mapper.Map<BookResponseDTO>(book));
        }
        public async Task<BookResponseDTO> AddBookAsync(BookDTO bookDTO)
        {
            var book = _mapper.Map<Book>(bookDTO);
            await _bookRepository.AddBookAsync(book);
            return _mapper.Map<BookResponseDTO>(book);
        }
        public async Task<BookResponseDTO?> UpdateBookAsync(int id, BookDTO bookDTO)
        {
            var existingBook = await _bookRepository.GetBookByIdAsync(id);
            if (existingBook == null) return null;

            _mapper.Map(bookDTO, existingBook); 
            await _bookRepository.UpdateBookAsync(existingBook);
            return _mapper.Map<BookResponseDTO>(existingBook);
        }
        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _bookRepository.GetBookByIdAsync(id);
            if (book == null) return false;

            await _bookRepository.DeleteBookAsync(book.Id);
            return true;
        }
        public async Task<string> BorrowBookAsync(int bookId, string username)
        //public async Task<bool> BorrowBookAsync(int bookId, string username)
        {
            var book = await _bookRepository.GetBookByIdAsync(bookId);
            if (book == null || book.Quantity <= 0) return null;

            var existingBorrow = await _borrowedBookRepository.HasUserBorrowedBookAsync(bookId, username);
            if (existingBorrow != null) return null;  // User already borrowed this book

            book.Quantity -= 1;
            await _bookRepository.UpdateBookAsync(book);

            var borrowedBook = new BorrowedBook
            {
                BookId = bookId,
                Username = username,
                BorrowedDate = DateTime.UtcNow
            };

            await _borrowedBookRepository.AddBorrowedBookAsync(borrowedBook);
            //return _mapper.Map<BorrowedBookDTO>(borrowedBook);
            return "Book was borrowed successfully";
        }

        public async Task<bool> ReturnBorrowedBookAsync(int bookId, string username)
        {
            var borrowRecord = await _borrowedBookRepository.HasUserBorrowedBookAsync(bookId, username);
            if (borrowRecord == null) return false;

            await _borrowedBookRepository.DeleteBorrowedBookAsync(bookId);
            return true;
        }
    }
}
