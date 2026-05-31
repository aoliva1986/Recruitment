using System.ComponentModel.DataAnnotations;

namespace Recruitment.Models
{
    public class Job_Position
    {
        [Key]
        public int position_id { get; set; }

        public int company_id { get; set; }
        public string internal_id { get; set; }
        public string title { get; set; }
        public string department { get; set; }
        public string location { get; set; }
        public bool is_remote { get; set; }
        public decimal min_salary { get; set; }
        public decimal max_salary { get; set; }
        public string status { get; set; }
        public int pipeline_id { get; set; }
        public int questionnaire_id { get; set; }
        public int scorecard_template_id { get; set; }
        public DateTime created_at { get; set; }

    }
}
