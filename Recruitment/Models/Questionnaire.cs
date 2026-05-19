using System.ComponentModel.DataAnnotations;

namespace Recruitment.Models
{
    public class Questionnaire
    {
        [Key]
        public int questionnaire_id { get; set; }
        public int company_id { get; set; }
        public string title { get; set; } = null!;
        public string description { get; set; } = null!;
        public DateTime created_at { get; set; }

    }
}
