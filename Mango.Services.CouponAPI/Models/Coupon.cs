using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Services.CouponAPI.Models;

public partial class Coupon
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CouponId { get; set; }

    [Required]
    [MaxLength(30)]
    public string CouponCode { get; set; } = null!;

    [Required]
    public double DiscountAmount { get; set; }

    public int? MinAmount { get; set; }

}
