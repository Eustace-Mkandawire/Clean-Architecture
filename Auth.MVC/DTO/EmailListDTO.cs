namespace Auth.MVC.DTO
{
    public class EmailListDTO
    {
        public List<string> Emails { get; set; } = [];
        public string? Message { get; set; }
        public string? Error { get; set; }
    }
    

}

