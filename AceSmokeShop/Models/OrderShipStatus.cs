using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AceSmokeShop.Models
{
    public class OrderShipStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("UserOrders")]
        [Column(TypeName = "nvarchar(256)")]
        public string OrderId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(256)")]
        public string CustOrderId { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public DateTime CreateDate { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string Status { get; set; }
    }
}
