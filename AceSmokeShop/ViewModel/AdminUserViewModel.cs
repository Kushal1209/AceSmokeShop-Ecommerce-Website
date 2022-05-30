using AceSmokeShop.Areas.Identity.Data;
using AceSmokeShop.Models;
using System.Collections.Generic;

namespace AceSmokeShop.ViewModel
{
    public class AdminUserViewModel
    {
        public AdminUserViewModel()
        {
            UserList = new List<AppUser>();

            StateID = 0;
            StateSelect = new State();
            StateList = new List<State>();

            EditUser = new EditUserViewModel();

            TotalUsers = 0;
            AdminUsers = 0;
            VendorUsers = 0;
            ActiveUsers = 0;
            BlockedUsers = 0;

            RowPerPage = new List<int> { 5, 10, 20, 50, 100 };

            UserRole = "Select-Role";
            UserRoleList = new List<string> { "Select-Role", "USER", "VENDOR", "ADMIN"};

            Search = "";
            SortByOrder = 1;
            SortByID = 0;

            CurrentPage = 1;
            TotalPages = 0;
            ItemsPerPage = 5;


        }



        public List<AppUser> UserList { get; set; }

        public EditUserViewModel EditUser { get; set; }

        public string UserRole { get; set; }
        public List<string> UserRoleList { get; set; }

        public List<State> StateList { get; set; }
        public int StateID { get; set; }
        public State StateSelect { get; set; }

        public List<int> RowPerPage { get; set; }

        public string Search { get; set; }
        public int SortByOrder { get; set; }
        public int SortByID { get; set; }


        public int TotalUsers;
        public int AdminUsers;
        public int VendorUsers;
        public int ActiveUsers;
        public int BlockedUsers;

        public int CurrentPage;
        public int TotalPages;
        public int ItemsPerPage;

    }
}
