using Mango.Services.EmailAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.EmailAPI.Data
{
    public class AppDBContext : DbContext
    {


        public AppDBContext(DbContextOptions<AppDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<EmailLogger> EmailLoggers { get; set; }

    }
}
