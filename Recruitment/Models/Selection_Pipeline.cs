using System.ComponentModel.DataAnnotations;

namespace Recruitment.Models
{
    public class Selection_Pipeline
    {
        [Key]
        public int pipeline_id { get; set; }
        public int company_id { get; set; }
        public string name { get; set; } = string.Empty;

        public DateTime created_at { get; set; }    
    }
}
