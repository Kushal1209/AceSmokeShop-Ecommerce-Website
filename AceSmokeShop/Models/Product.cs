using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AceSmokeShop.Models
{
    public class Product
    {
        [Key]
        [Column(TypeName = "int")]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Enter Barcode")]
        [Display(Name = "Barcode")]
        [Column(TypeName = "nvarchar(256)")]
        public string Barcode { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        [Column(TypeName = "nvarchar(256)")]
        public string ProductName { get; set; }

        [Required]
        [ForeignKey("Category")]
        [Display(Name = "Category")]
        [Column(TypeName = "int")]
        public int? CategoryID { get; set; }

        [Required]
        [ForeignKey("SubCategory")]
        [Display(Name = "Sub-Category")]
        [Column(TypeName = "int")]
        public int? SubCategoryID { get; set; }

        [Required]
        [Display(Name = "Stock")]
        [Column(TypeName = "int")]
        public int Stock { get; set; }

        [Display(Name = "Unit of Measure")]
        [Column(TypeName = "nvarchar(256)")]
        public string UnitOfMeasure { get; set; }

        [Required]
        [Display(Name = "Base Price")]
        [Column(TypeName = "decimal(35,2)")]
        public double BasePrice { get; set; }

        [Required]
        [Display(Name = "Sale Price")]
        [Column(TypeName = "decimal(35,2)")]
        public double SalePrice { get; set; }

        [Required]
        [Display(Name = "Description")]
        [Column(TypeName = "nvarchar(256)")]
        public string Description { get; set; }

        [Display(Name = "IsFeatured")]
        [Column(TypeName = "bit")]
        public bool IsFeatured { get; set; }

        [Display(Name = "IsPromoted")]
        [Column(TypeName = "bit")]
        public bool IsPromoted { get; set; }

        [Column(TypeName = "bit")]
        public bool IsRemoved { get; set; }

        public string PrimaryImage { get; set;}

        public string SecondaryImage1 { get; set; }

        public string SecondaryImage2 { get; set; }

        public virtual Category Category { get; set; }

        public virtual SubCategory SubCategory { get; set; }
    }
}
