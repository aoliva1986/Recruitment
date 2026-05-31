namespace Recruitment.DTOs
{
    public class CandidateDto
    {        
        public int candidate_id { get; set; }
        public int company_id { get; set; }
        public int position_id { get; set; }
        public int current_stage_id { get; set; }
        public string full_name { get; set; } 
        public string email { get; set; } 
        public string phone { get; set; } 
        public string cv_url { get; set; } 
        public string origin { get; set; } 
        public string status { get; set; } 
        public DateTime created_at { get; set; }

    }
}
