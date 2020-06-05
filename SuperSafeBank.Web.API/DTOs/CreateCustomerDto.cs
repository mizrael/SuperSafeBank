using System.ComponentModel.DataAnnotations;

namespace SuperSafeBank.Web.API.DTOs
{
    public class CreateCustomerDto
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }
    }
}