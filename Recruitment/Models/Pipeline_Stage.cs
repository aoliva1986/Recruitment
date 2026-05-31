using System.ComponentModel.DataAnnotations;

namespace Recruitment.Models
{
    public class Pipeline_Stage
    {
        [Key]
        public int stage_id { get; set; }
        public int pipeline_id { get; set; }
        public string name { get; set; }
        public int order { get; set; }

    }
}
