namespace Recruitment.DTOs
{
    public class UploadDocumentDto
    {
        public int candidate_id { get; set; }
        public string? name { get; set; }
        public string? type { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        // El archivo físico ahora vive dentro del objeto que viene del formulario
        public IFormFile File { get; set; }
    }
}
