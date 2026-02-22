using AccountAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Customer
{
    [Key]
    [ForeignKey("Account")]
    [Column(TypeName = "nvarchar(20)")]
    [MaxLength(20)]
    public string CustomerID { get; set; } = null!;   // = AccountID

    [Column(TypeName = "nvarchar(100)")]
    [MaxLength(100)]
    public string? FullName { get; set; }

    [Column(TypeName = "date")]
    public DateTime? DateOfBirth { get; set; }

    [Column(TypeName = "nvarchar(10)")]
    [MaxLength(10)]
    public string? Gender { get; set; }

    [Column(TypeName = "nvarchar(20)")]
    [MaxLength(20)]
    public string? Phone { get; set; }

    [Column(TypeName = "nvarchar(255)")]
    [MaxLength(255)]
    public string? Address { get; set; }

    [Column(TypeName = "nvarchar(20)")]
    [MaxLength(20)]
    public string? Status { get; set; }

    public Account Account { get; set; } = null!;
}