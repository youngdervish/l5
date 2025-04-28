using l5.Core.Models;

namespace l5.Core.Interfaces
{
    public interface IBorrowedBookRepository
    {
        Task AddBorrowedBookAsync(BorrowedBook borrowedBook);
        Task<bool> HasUserBorrowedBookAsync(int bookId, string username);
        Task DeleteBorrowedBookAsync(BorrowedBook borrowedBook);
    }
}
