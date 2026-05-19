namespace Recruitment.DTOs
{
    public class CompanyDto
    {
        public int company_id { get; set; }
        public string name { get; set; } = string.Empty;
        public string tax_id { get; set; } = string.Empty;
        public string website { get; set; } = string.Empty;
    }
}
