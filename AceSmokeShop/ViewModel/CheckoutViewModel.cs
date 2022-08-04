
using AceSmokeShop.Models;
using AuthorizeNet.Api.Contracts.V1;
using System.Collections.Generic;

namespace AceSmokeShop.ViewModel
{
    public class CheckoutViewModel
    {
        public CheckoutViewModel()
        {
            Address = new AddressViewModel();
            Subtotal = 0;
            Qty = 1;
            Tax = 0;
            Shipping = 0;
            Grandtotal = 0;
            productId = "";
            BuyNowProduct = new Product();

            CardList = new List<CardInfo>();

            CartList = new List<Cart>();
        }
        public AddressViewModel Address { get; set; }
        //public Stripe.StripeList<Stripe.PaymentMethod> 

        public List<CardInfo> CardList { get; set; }

        public double Subtotal { get; set; }
        public double Tax { get; set; }
        public double Shipping { get; set; }
        public double Grandtotal { get; set; }
        public string productId { get; set; }
        public Product BuyNowProduct { get; set; }
        public int Qty { get; set; }

        public List<Cart> CartList { get; set; }
    }

    public class CardInfo{

        public string cardNumber { get; set; }
        public string paymentProfileId { get; set; }
        public string cardType { get; set; }

    }
}
