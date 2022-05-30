using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Models;
using System.Collections.Generic;

namespace AceSmokeShop.ViewModel
{
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            UserEmail = "";
            UserRole = "USER";
            IsActive = true;
            UserRoleList = new List<string> { "USER", "ADMIN", "VENDOR"};
        }

        public string UserEmail { get; set; }

        public List<string> UserRoleList { get; set; }

        public string UserRole { get; set; }

        public bool IsActive { get; set; }
    }
}
