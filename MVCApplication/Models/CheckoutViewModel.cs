using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MVCApplication.Models
{
    public class CheckoutViewModel
    {
        [ValidateNever]
        public List<CartItemViewModel> SelectedItems { get; set; } = new();
        
        [ValidateNever]
        public decimal Subtotal { get; set; }
        
        // Form Fields
        public string ShippingAddress { get; set; } = "";
        public string? Note { get; set; }
        public int? VoucherId { get; set; }
        public string PaymentMethod { get; set; } = "COD"; // Default COD
    }
}
