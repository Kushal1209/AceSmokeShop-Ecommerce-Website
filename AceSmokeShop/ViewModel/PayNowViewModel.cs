using AceSmokeShop.Models;
using System.Collections.Generic;

namespace AceSmokeShop.ViewModel
{
    public class PayNowViewModel
    {
        public PayNowViewModel()
        {
            userOrder = new UserOrders();
            PaymentCards = new PaymentCardViewModel();
            CardList = new List<CardInfo>();
        }

        public UserOrders userOrder { get; set; }

        public PaymentCardViewModel PaymentCards { get; set; }
        public List<CardInfo> CardList { get; set; }
    }
}
