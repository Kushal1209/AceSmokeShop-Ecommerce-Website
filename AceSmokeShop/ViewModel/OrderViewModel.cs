using AceSmokeShop.Models;
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
        }
        
        public List<UserOrders> userOrdersList { get; set; }
        public List<State> States { get; set; }

        public List<string> CancelOrderStatus { get; set; }

        public string OrderSelectId { get; set; }
    }
}
