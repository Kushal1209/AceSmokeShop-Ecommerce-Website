using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AceSmokeShop.Models
{
    public class Category
    {
        [Key]
        [Column(TypeName = "int")]

        public int CategoryID { get; set; }

        [Display(Name = "Category")]
        [Column(TypeName = "nvarchar(256)")]
        public string CategoryName { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string Desc { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string Image { get; set; }
    }
}
