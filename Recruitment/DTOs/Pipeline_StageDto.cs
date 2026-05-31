using System.ComponentModel.DataAnnotations;

namespace Recruitment.DTOs
{
    public class Pipeline_StageDto
    {        
        public int stage_id { get; set; }
        public int pipeline_id { get; set; }
        public string name { get; set; }
        public int order { get; set; }
    }
}
