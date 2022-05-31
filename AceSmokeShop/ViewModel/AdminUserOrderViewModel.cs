using AceSmokeShop.Models;
using System.Collections.Generic;

namespace AceSmokeShop.ViewModel
{
    public class AdminUserOrderViewModel
    {
        public AdminUserOrderViewModel()
        {
            userOrdersList = new List<UserOrders>();
            States = new List<State>();
            CancelOrderReason = new List<string> { "Select Status", "Order Delayed", "Accidental Order", "Other" };
            OrderStatusList = new List<string> { "Select Status", "Order Placed", "Order Packaging", "Order Dispatched", "Delivered" };
            OrderStatus = "Select Status";

            adminOrderDetailsViewModel = new AdminOrderDetailsViewModel();

            SortByOrder = 1;
            SortByID = 0;
            search = "";

            TotalOrders = 0;
            RowPerPage = new List<int> { 10, 20, 50, 100 };
            CurrentPage = 1;
            TotalPages = 0;
            ItemsPerPage = 10;
        }

        public AdminOrderDetailsViewModel adminOrderDetailsViewModel { get; set; }
        public List<UserOrders> userOrdersList { get; set; }
        public List<State> States { get; set; }
        public List<string> CancelOrderReason { get; set; }
        public List<string> OrderStatusList { get; set; }
        public string OrderStatus { get; set; }
        public string search { get; set; }
        public int SortByOrder { get; set; }
        public int SortByID { get; set; }
        public List<int> RowPerPage { get; set; }
        public int TotalOrders { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int ItemsPerPage { get; set; }
        public string OrderSelectId { get; set; }
    }
}
