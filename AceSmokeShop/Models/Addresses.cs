using AceSmokeShop.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AceSmokeShop.Models
{
    public class Addresses
    {
        public Addresses()
        {
            State = new State();
        }

        [Key]
        [Column(TypeName = "int")]
        public int Id { get; set; }

        [Required]
        [ForeignKey("AppUser")]
        [Column(TypeName = "nvarchar(450)")]
        public string UserId { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(256)")]
        public string AddressLineA { get; set; }

        [Column(TypeName = "NVARCHAR(256)")]
        public string AddressLineB { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR(256)")]
        public string City { get; set; }

        [Required]
        [ForeignKey("State")]
        [Column(TypeName = "int")]
        public int StateID { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public int Zipcode { get; set; }

        [Column(TypeName = "bit")]
        public bool IsRemoved { get; set; }

        [Column(TypeName = "bit")]
        public bool IsShipping { get; set; }

        [Column(TypeName = "bit")]
        public bool IsBilling { get; set; }

        public virtual State State { get; set; }
        public virtual AppUser User { get; set; }
    }
}
