using AceSmokeShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AceSmokeShop.ViewModel
{
    public class AdminUserOrderViewModel
    {
        public AdminUserOrderViewModel()
        {
            userOrdersList = new List<UserOrders>();
            States = new List<State>();
            
            CancelOrderReason = new List<string> { "Select Status", "Order Delayed", "Accidental Order", "Other" };
            
            OrderStatusList = new List<string> { "Select Status", "Order Placed", "Order Packaging", "Order Dispatched", "Delivered", "Cancelled", "Return Requested", "Return Approved", "Return Shipped", "Return Delivered" };
            OrderStatus = "Select Status";

            SearchByUser = "Select User";
            SearchByUserList = new List<string> { "Select User", "USER", "VENDOR" };

            PaymentStatus = "Select Payment Status";
            PaymentStatusList = new List<string> { "Select Payment Status", "PAID", "UNPAID", "REFUNDED", "CARD PAYMENT", "IN-STORE PAYMENT" };

            adminOrderDetailsViewModel = new AdminOrderDetailsViewModel();

            SortByOrder = 1;
            SortByID = 0;
            search = "";

            TotalOrders = 0;
            FilteredOrders = 0;
            TotalUsers = 0;
            VendorOrder = 0;
            UserOrder = 0;
            UnPaidOrders = 0;
            UnPaidAmount = 0.0;
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
        public List<string> PaymentStatusList { get; set; }
        public string PaymentStatus { get; set; }
        public List<string> SearchByUserList { get; set; }
        public string SearchByUser { get; set; }
        public string search { get; set; }
        public int SortByOrder { get; set; }
        public int SortByID { get; set; }
        public List<int> RowPerPage { get; set; }
        public int TotalOrders { get; set; }

        public int FilteredOrders { get; set; }
        public int TotalUsers { get; set; }
        public int VendorOrder { get; set; }
        public int UserOrder { get; set; }
        public int CancelledOrders { get; set; }
        public int UnPaidOrders { get; set; }
        public double UnPaidAmount { get; set; }
        public int NewOrders { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int ItemsPerPage { get; set; }
        public string OrderSelectId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateFrom { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateTo { get; set; }
    }
}
