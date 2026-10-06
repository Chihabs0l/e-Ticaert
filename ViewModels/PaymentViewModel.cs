using System.ComponentModel.DataAnnotations;

namespace ShoppingApp.ViewModels;

/// <summary>Mock payment gateway form. NOT a real payment integration.</summary>
public class PaymentViewModel
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }

    [Required, StringLength(100)]
    public string CardHolder { get; set; } = string.Empty;

    [Required, CreditCard]
    public string CardNumber { get; set; } = string.Empty;

    [Required, RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Format MM/YY")]
    public string Expiry { get; set; } = string.Empty;

    [Required, StringLength(4, MinimumLength = 3)]
    public string Cvv { get; set; } = string.Empty;
}
