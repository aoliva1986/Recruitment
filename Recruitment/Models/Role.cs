using System.ComponentModel.DataAnnotations;

namespace Recruitment.Models
{
    public class Role
    {
        [Key]
        public int role_id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string description { get; set; }
    }
}
