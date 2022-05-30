using AceSmokeShop.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AceSmokeShop.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("UserOrders")]
        [Column(TypeName = "int")]
        public int OrderId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(256)")]
        public string CustOrderId { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required]
        [Column(TypeName = "decimal(35,2)")]
        public double Price { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public int Quantity { get; set; }

        public virtual Product Product { get; set; }

        public virtual UserOrders UserOrder { get; set; }
    }
}
