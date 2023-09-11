using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Messaging;
using Mango.Services.EmailAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
////ntkien - config
// add DB context
builder.Services.AddDbContext<AppDBContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// gán cho hàm tạo EmailService() để tương tác vs csdl
var optionBuilder = new DbContextOptionsBuilder<AppDBContext>();
optionBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddSingleton(new EmailService(optionBuilder.Options));

//Hosted Service là một dịch vụ chạy trong nền, được quản lý bởi khung ứng dụng (framework) và được khởi động cùng với ứng dụng. 
// nên RabbitMQAuthConsumer implement từ BackgroundService
builder.Services.AddHostedService<RabbitMQAuthConsumer>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// mặc định khi run thì nó sẽ kiểm tra có migration nào đang pending ko? nếu có thì nó cập nhật trong db
ApplyMigration();

app.MapControllers();

app.Run();

void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<AppDBContext>();

        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}
