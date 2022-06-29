using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AceSmokeShop.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the AppUser class
    public class AppUser : IdentityUser
    {
        public AppUser(){

            UserRole = "USER";
        }

        [PersonalData]
        [Column(TypeName = "nvarchar(256)")]
        public string Fullname { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(256)")]
        public string Contact { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(256)")]
        public DateTime Dob { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(256)")]
        public string CustomerId { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(256)")]
        public string UserRole { get; set; }

        [PersonalData]
        public int StateID { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(256)")]
        public DateTime CreateDate { get; set; }

        [PersonalData]
        [Column(TypeName = "bit")]
        public bool IsAccounting { get; set; }

        [PersonalData]
        [Column(TypeName = "nvarchar(256)")]
        public string Stores { get; set; }

        public static explicit operator AppUser(Task<object> v)
        {
            throw new NotImplementedException();
        }
    }
}
