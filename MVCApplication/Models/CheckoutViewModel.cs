using System.Collections.Generic;

namespace MVCApplication.Models
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> SelectedItems { get; set; } = new();
        public decimal Subtotal { get; set; }
        
        // Form Fields
        public string ShippingAddress { get; set; } = "";
        public string Note { get; set; } = "";
        public int? VoucherId { get; set; }
        public string PaymentMethod { get; set; } = "COD"; // Default COD
    }
}
