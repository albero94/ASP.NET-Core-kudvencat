using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASP.NET_Core_MVC_kudvenkat.Models
{
    public class Employee
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage ="Name cannot exceed 50 characters")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Office Email")]
        [EmailAddress]
        public string Email { get; set; }
        public Dept Department { get; set; }
    }
}
