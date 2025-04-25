using l5.Application.DTOs;

namespace l5.Core.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookResponseDTO>> GetAllBooksAsync();
        Task<BookResponseDTO?> GetBookByIdAsync(int id);
        Task<IEnumerable<BookDTO>> SearchBooksByTitleAsync(string title);
        Task<BookResponseDTO> AddBookAsync(BookDTO bookDTO);
        Task<BookResponseDTO?> UpdateBookAsync(int id, BookDTO bookDTO);
        Task<bool> DeleteBookAsync(int id);
        Task<string> BorrowBookAsync(int bookId, string username);
    }
}
