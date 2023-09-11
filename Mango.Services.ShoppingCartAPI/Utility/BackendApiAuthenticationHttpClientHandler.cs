using Mango.Services.ShoppingCartAPI.Data;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace Mango.Services.ShoppingCartAPI.Utility
{
    public class BackendApiAuthenticationHttpClientHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _accessor;

        public BackendApiAuthenticationHttpClientHandler(IHttpContextAccessor accessor)
        {
            //  cho phép truy cập thông tin liên quan đến yêu cầu HTTP hiện tại, bao gồm các token xác thực.
            _accessor = accessor;
        }

        //được gọi khi gửi một yêu cầu HTTP bằng HttpClient. Phương thức này chịu trách nhiệm bổ sung thông tin xác thực vào yêu cầu trước khi nó được gửi đi.
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //lấy token xác thực từ HttpContext hiện tại
            var token = await _accessor.HttpContext.GetTokenAsync("access_token");

            //thêm thông tin xác thực vào yêu cầu HTTP thông qua HTTP header
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // gửi yêu cầu HTTP đã được cập nhật đến dịch vụ web hoặc API.
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
