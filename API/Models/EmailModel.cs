namespace B3C3GRP6.API.Models
{
    public class EmailModel
    {
        public string? ToEmails { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public List<KeyValuePair<string, string>>? PlaceHolders { get; set; }
    }
}
