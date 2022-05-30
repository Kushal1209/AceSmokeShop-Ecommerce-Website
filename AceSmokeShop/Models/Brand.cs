using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AceSmokeShop.Models
{
    public class Brand
    {
        [Key]
        [Column(TypeName = "int")]
        public int BrandID { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string BrandName { get; set; }
    }
}
