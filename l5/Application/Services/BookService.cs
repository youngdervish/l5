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
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public BookService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        Task<IEnumerable<BookResponseDTO>> GetAllBooksAsync()
        {

        }
        Task<BookResponseDTO?> GetBookByIdAsync(int id);
        Task<IEnumerable<BookDTO>> SearchBooksByTitleAsync(string title);
        Task<BookResponseDTO> AddBookAsync(BookDTO bookDTO);
        Task<BookResponseDTO?> UpdateBookAsync(int id, BookDTO bookDTO);
        Task<bool> DeleteBookAsync(int id);
        Task<string> BorrowBookAsync(int bookId, string username);
    }
}
