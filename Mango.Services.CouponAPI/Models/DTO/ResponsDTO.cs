namespace Mango.Services.CouponAPI.Models.DTO
{
    public class ResponsDTO
    {
        public object? Result { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "";
    }
}
