using Mango.Services.ShoppingCartAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Data
{
    public class AppDBContext : DbContext
    {
        // đại diện cho đối tượng cơ sở dữ liệu của ứng dụng và được sử dụng để tương tác với cơ sở dữ liệu, thường là một cơ sở dữ liệu SQL.
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        //để đại diện cho các bảng hoặc tập hợp dữ liệu trong cơ sở dữ liệu.
        public DbSet<CartHeader> CartHeaders { get; set; }
        public DbSet<CartDetails> CartDetails { get; set; }

    }
}
