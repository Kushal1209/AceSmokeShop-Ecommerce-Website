
using AceSmokeShop.Models;
using System.Collections.Generic;

namespace AceSmokeShop.ViewModel
{
    public class CheckoutViewModel
    {
        public CheckoutViewModel()
        {
            Address = new AddressViewModel();
            PaymentMethods = new Stripe.StripeList<Stripe.PaymentMethod>();
            Subtotal = 0;
            Qty = 1;
            Tax = 0;
            Shipping = 0;
            Grandtotal = 0;
            productId = "";
            BuyNowProduct = new Product();

            CartList = new List<Cart>();
        }
        public AddressViewModel Address { get; set; }
        public Stripe.StripeList<Stripe.PaymentMethod> PaymentMethods { get; set; }
        public double Subtotal { get; set; }
        public double Tax { get; set; }
        public double Shipping { get; set; }
        public double Grandtotal { get; set; }
        public string productId { get; set; }
        public Product BuyNowProduct { get; set; }
        public int Qty { get; set; }

        public List<Cart> CartList { get; set; }
    }
}
