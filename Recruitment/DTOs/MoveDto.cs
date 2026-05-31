namespace Recruitment.DTOs
{
    public class MoveDto
    {
        public int move_id { get; set; }
        public int candidate_id { get; set; }
        public int stage_id { get; set; }
        public string comments { get; set; }        
        public DateTime created_at { get; set; }

    }
}
