namespace l5.Application.DTOs
{
    public class UpdateUserDTO
    {
        public string Role { get; set; }    // Optional fields for update
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
