using Mango.Services.AuthAPI;
using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.RabbitMQSender;
using Mango.Services.AuthAPI.Services;
using Mango.Services.AuthAPI.Services.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var modelBuilder = WebApplication.CreateBuilder(args);

// Add services to the container.
////ntkien - config
// add DB context
modelBuilder.Services.AddDbContext<AppDBContext>(option =>
{
    option.UseSqlServer(modelBuilder.Configuration.GetConnectionString("DefaultConnection"));
});

// config jwt
//Sau khi cài đặt này được áp dụng, các cài đặt JWT trong tệp cấu hình của bạn sẽ được đọc và cấu hình cho lớp JwtOption.
//Điều này cho phép ứng dụng của bạn sử dụng các thông tin này để tạo và xác thực JWT tokens dựa trên cài đặt đã được định nghĩa.
modelBuilder.Services.Configure<JwtOption>(modelBuilder.Configuration.GetSection("ApiSettings:JwtOptions"));


// add dịch vụ identity
//AddEntityFrameworkStores: Đoạn mã này chỉ định rằng bạn đang sử dụng Entity Framework (EF) để lưu trữ dữ liệu liên quan đến người dùng và vai trò. 
//Phương thức này thêm các token provider mặc định vào Identity. Các token provider là các phần mềm hỗ trợ cho việc quản lý xác thực và phục hồi mật khẩu trong Identity.
/*modelBuilder.Services.AddIdentityCore<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders();*/

// defining our IdentityCore Service
modelBuilder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    // password configuration
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    // for email confirmation
    options.SignIn.RequireConfirmedEmail = true;
})
    .AddRoles<IdentityRole>() // be able to add roles
    .AddRoleManager<RoleManager<IdentityRole>>() // be able to make use of RoleManager
    .AddEntityFrameworkStores<AppDBContext>() // providing our context
    .AddSignInManager<SignInManager<ApplicationUser>>() // make use of Signin manager
    .AddUserManager<UserManager<ApplicationUser>>() // make use of UserManager to create users
    .AddDefaultTokenProviders(); // be able to create tokens for email confirmation

// be able to authenticate users using JWT
modelBuilder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // validate the token based on the key we have provided inside appsettings.development.json JWT:Key
            ValidateIssuerSigningKey = true,
            // the issuer singning key based on JWT:Key
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(modelBuilder.Configuration["ApiSettings:JwtOptions:Secret"])),
            // the issuer which in here is the api project url we are using
            ValidIssuer = modelBuilder.Configuration["ApiSettings:JwtOptions:Issuer"],
            // validate the issuer (who ever is issuing the JWT)
            ValidateIssuer = true,
            // don't validate audience (angular side)
            ValidateAudience = false
        };
    });
/*    // Thêm middleware xử lý cookies
    .AddCookie("Cookies", options =>
    {
        // Cấu hình tên của cookie chứa token
        options.Cookie.Name = SD.TokenCookie;
        // Các cấu hình khác (HttpOnly, Expire, SameSite...) nếu cần
    });*/




/*modelBuilder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        // Đọc thông tin Authentication:Google từ appsettings.json
        IConfigurationSection googleAuthNSection = modelBuilder.Configuration.GetSection("Authentication:Google");

        // Thiết lập ClientID và ClientSecret để truy cập API google
        googleOptions.ClientId = googleAuthNSection["ClientId"];
        googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];
        // Cấu hình Url callback lại từ Google (không thiết lập thì mặc định là /signin-google)
        googleOptions.CallbackPath = "/dang-nhap-tu-google";

    });*/

modelBuilder.Services.AddCors();


modelBuilder.Services.AddControllers();


// DI
modelBuilder.Services.AddScoped<IAuthService, AuthService>();
modelBuilder.Services.AddScoped<ITokenProvider, TokenProvider>();
modelBuilder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
modelBuilder.Services.AddScoped<EmailService>();
modelBuilder.Services.AddScoped<IRabbitMQAuthMessageSender, RabbitMQAuthMessageSender>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
modelBuilder.Services.AddEndpointsApiExplorer();
modelBuilder.Services.AddSwaggerGen();


var app = modelBuilder.Build();

// Configure the HTTP request pipeline.
app.UseCors(opt =>
{
    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(modelBuilder.Configuration["ApiSettings:JwtOptions:ClientUrl"]);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// mặc định khi run thì nó sẽ kiểm tra có migration nào đang pending ko? nếu có thì nó cập nhật trong db
ApplyMigration();

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