namespace l5.Application.DTOs
{
    public class BookResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int Quantity { get; set; }
        public int Year {  get; set; }
        public bool IsAvailable => Quantity > 0;
    }
}
