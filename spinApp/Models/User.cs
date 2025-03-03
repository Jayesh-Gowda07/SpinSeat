using System.ComponentModel.DataAnnotations;
using System;

namespace spinApp.Models
{
    public class User
    {
        public Guid Id { get; set; }
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
    }

}
