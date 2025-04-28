using l5.Core.Interfaces;
using l5.Core.Models;
using l5.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace l5.Application.Repositories
{
    public class BorrowedBookRepository: IBorrowedBookRepository
    {
        private readonly AppDbContext _context;

        public BorrowedBookRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddBorrowedBookAsync(BorrowedBook borrowedBook)
        {
            await _context.BorrowedBooks.AddAsync(borrowedBook);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasUserBorrowedBookAsync(int bookId, string username)
        {
            return await _context.BorrowedBooks.AnyAsync(bb => bb.Username == username && bb.BookId == bookId);
        }

        public async Task DeleteBorrowedBookAsync(BorrowedBook borrowedBook)
        {
            _context.BorrowedBooks.Remove(borrowedBook);
            await _context.SaveChangesAsync();
        }
    }
}
