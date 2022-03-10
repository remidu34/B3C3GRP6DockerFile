using System.ComponentModel.DataAnnotations;

namespace B3C3GRP6.Data.Models
{
    public partial class Compte
    {
        public Compte()
        {

        }
        [Key]
        public int IdCompte { get; set; }
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? BrowserName { get; set; }
        public string? IpPublic { get; set; }
        public string IncrementDelay { get; set; } = null!;
    }
}
