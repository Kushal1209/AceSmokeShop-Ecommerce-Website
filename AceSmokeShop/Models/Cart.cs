

using AceSmokeShop.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AceSmokeShop.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("AppUser")]
        [Column(TypeName = "nvarchar(450)")]
        public string UserId { get; set; }

        [ForeignKey("Product")]
        [Column(TypeName = "int")]
        public int ProductId { get; set; }

        [Column(TypeName = "int")]
        public int Quantity { get; set; }

        [Column(TypeName = "bit")]
        public bool IsRemoved { get; set; }


        public virtual AppUser User { get; set; }
        public virtual Product Product { get; set; }

    }
}
