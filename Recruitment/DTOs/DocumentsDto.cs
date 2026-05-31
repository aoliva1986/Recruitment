using System.ComponentModel.DataAnnotations;

namespace Recruitment.DTOs
{
    public class DocumentsDto
    {                  
            public int id { get; set; }
            public int candidate_id { get; set; }
            public string name { get; set; }
            public string file_path { get; set; }
            public string type { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
        
    }
}
