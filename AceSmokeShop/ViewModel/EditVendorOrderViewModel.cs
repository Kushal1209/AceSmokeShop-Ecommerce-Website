using AceSmokeShop.Models;

namespace AceSmokeShop.ViewModel
{
    public class EditVendorOrderViewModel
    {
        public EditVendorOrderViewModel()
        {
            Qty = 0;
            CustOrderId = "";
            OrderId = 0;
            items = 0;
        }
        public int Qty { get; set; }
        
        public string CustOrderId { get; set; }
        
        public int OrderId { get; set; }

        public int items { get; set; }

    }
}
