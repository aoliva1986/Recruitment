using System.ComponentModel.DataAnnotations;

namespace Recruitment.Models
{
    public class Company
    {
        [Key]
        public int company_id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string tax_id { get; set; }
        [Required]
        public string website { get; set; }

    }
}
