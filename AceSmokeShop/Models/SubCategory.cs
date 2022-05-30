using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AceSmokeShop.Models
{
    public class SubCategory
    {
        [Key]
        [Column(TypeName = "int")]
        public int SubCategoryID { get; set; }

        [ForeignKey("Category")]
        public int CategoryID { get; set; }

        [Display(Name = "Sub-Category")]
        [Column(TypeName = "nvarchar(256)")]
        public string SubCategoryName { get; set; }

        public virtual Category Category { get; set; }
    }
}
