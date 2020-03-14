using System.ComponentModel.DataAnnotations;

namespace SuperSafeBank.Web.API.DTOs
{
    public class CreateAccountDto
    {
        [Required]
        public string CurrencyCode { get; set; }
    }
}