using AceSmokeShop.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AceSmokeShop.Models
{
    public class UserOrders
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(256)")]
        public string CustOrderId { get; set; }

        [ForeignKey("AppUser")]
        [Column(TypeName = "nvarchar(450)")]
        public string UserId { get; set; }

        [ForeignKey("Addresses")]
        public int ShippingAddressId { get; set; }

        [Required]
        [Column(TypeName = "decimal(35,2)")]
        public double TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(35,2)")]
        public double SubTotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(35,2)")]
        public double Tax { get; set; }

        [Required]
        [Column(TypeName = "decimal(35,2)")]
        public double ShippingCharge { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(256)")]
        public string OrderStatus { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string PaymentId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(256)")]
        public DateTime CreateDate { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public DateTime DeliveryDate { get; set; }

        [Column(TypeName = "bit")]
        public bool IsVendor { get; set; }

        [Column(TypeName = "bit")]
        public bool IsPaid { get; set; }
        public virtual AppUser User { get; set; }
        public virtual Addresses ShippingAddress { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public virtual List<OrderShipStatus> OrderShipStatus { get; set; }
    }
}
