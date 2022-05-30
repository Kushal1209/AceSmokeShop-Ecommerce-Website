using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AceSmokeShop.Models
{
    public class State
    {
        [Key]
        public int StateID { get; set; }

        [Display(Name = "State")]
        [Column(TypeName = "nvarchar(256)")]
        public string StateName { get; set; }
    }
}
