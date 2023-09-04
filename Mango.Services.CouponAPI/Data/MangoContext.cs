using System;
using System.Collections.Generic;
using Mango.Services.CouponAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.CouponAPI.Data;

public partial class MangoContext : DbContext
{
    public MangoContext()
    {
    }

    public MangoContext(DbContextOptions<MangoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Coupon> Coupons { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
