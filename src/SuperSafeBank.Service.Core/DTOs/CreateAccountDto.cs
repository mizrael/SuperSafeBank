using System;
using System.ComponentModel.DataAnnotations;

namespace SuperSafeBank.Service.Core.DTOs
{
    public record CreateAccountDto
    {
        [Required]
        public string CurrencyCode { get; set; }

        [Required]
        public Guid CustomerId { get; set; } 
    }
}