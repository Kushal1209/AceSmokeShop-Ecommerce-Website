using AceSmokeShop.Areas.Identity.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AceSmokeShop.Models
{
    public class Transactions
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(256)")]
        public string PaymentMethod { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(256)")]
        public string Status { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string OrderId { get; set; }

        [Required]
        [ForeignKey("AppUser")]
        [Column(TypeName = "nvarchar(450)")]
        public string UserId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(256)")]
        public string TransactionType { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(256)")]
        public string UserRole { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(256)")]
        public DateTime CreateDate { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string PaymentIntentId { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string RefundId { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string VoidId { get; set; }

        [Required]
        [Column(TypeName = "decimal(35, 2)")]
        public double Amount { get; set; }

        public virtual AppUser User { get; set; }
    }
}
