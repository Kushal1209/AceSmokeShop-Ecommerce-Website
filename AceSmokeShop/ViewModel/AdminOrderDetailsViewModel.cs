using AceSmokeShop.Models;
using System.Collections.Generic;

namespace AceSmokeShop.ViewModel
{
    public class AdminOrderDetailsViewModel
    {
        public AdminOrderDetailsViewModel()
        {
            ShippingAddress = new Addresses();
            ListOfOrderItem = new List<OrderItem>();
            userOrder = new UserOrders();
            States = new List<State>();
        }

        public UserOrders userOrder { get; set; }
        public Addresses ShippingAddress { get; set; }
        public List<OrderItem> ListOfOrderItem { get; set; }
        public List<State> States { get; set; }
    }
}
