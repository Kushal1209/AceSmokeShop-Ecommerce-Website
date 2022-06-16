using AceSmokeShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AceSmokeShop.ViewModel
{
    public class OrderViewModel
    {
        public OrderViewModel()
        {
            userOrdersList = new List<UserOrders>();
            States = new List<State>();

            CancelOrderStatus = new List<string> { "Select Status", "Order Delayed", "Accidental Order", "Other" };

            OrderSelectId = "";

            OrderStatus = "Select OrderStatus";
            OrderStatusList = new List<string> { "Select OrderStatus", "Order Placed", "Order Packaging", "Order Dispatched", "Delivered", "Cancelled" };

            PaymentStatus = "Select PaymentStatus";
            PaymentStatusList = new List<string> { "Select PaymentStatus" , "PAID", "UNPAID"};

            ShippingStatus = "Select ShippingStatus";
            ShippingStatusList = new List<string> { "Select ShippingStatus", "Delivery", "Pick at Store" };

            search = "";
            UnPaidOrders = 0;
            UnPaidAmount = 0.0;
        }
        
        public List<UserOrders> userOrdersList { get; set; }
        public List<State> States { get; set; }
        public List<string> CancelOrderStatus { get; set; }
        public string OrderSelectId { get; set; }
        public List<string> OrderStatusList { get; set; }
        public string OrderStatus { get; set; }
        public List<string> PaymentStatusList { get; set; }
        public string PaymentStatus { get; set; }
        public List<string> ShippingStatusList { get; set; }
        public string ShippingStatus { get; set; }
        public string search { get; set; }
        public int UnPaidOrders { get; set; }
        public double UnPaidAmount { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateFrom { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd'/'MM'/'yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateTo { get; set; }
    }
}
