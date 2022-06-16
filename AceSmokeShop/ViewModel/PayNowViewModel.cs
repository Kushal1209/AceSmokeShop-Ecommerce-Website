using AceSmokeShop.Models;

namespace AceSmokeShop.ViewModel
{
    public class PayNowViewModel
    {
        public PayNowViewModel()
        {
            userOrder = new UserOrders();
            PaymentCards = new PaymentCardViewModel();
            PaymentMethods = new Stripe.StripeList<Stripe.PaymentMethod>();
        }

        public UserOrders userOrder { get; set; }

        public PaymentCardViewModel PaymentCards { get; set; }
        public Stripe.StripeList<Stripe.PaymentMethod> PaymentMethods { get; set; }
    }
}
