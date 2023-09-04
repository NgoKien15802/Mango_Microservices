using System;
using System.Collections.Generic;

namespace Mango.Services.AuthAPI.Models;

public partial class AppDBContext : DbContext
{
    public AppDBContext()
    {
    }

    public AppDBContext(DbContextOptions<MangoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Coupon> Coupons { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
