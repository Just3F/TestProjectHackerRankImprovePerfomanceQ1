using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TestProject.WebAPI
{
    public class RequestCounterMiddleware
    {
        private readonly RequestDelegate _next;

        private int _counter = 0;

        public RequestCounterMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string secretKeyConst = "passwordKey123456789";
            var responseHeaders = context.Response.Headers;
            responseHeaders.TryGetValue("secretKey", out var passwordKey);
            if (passwordKey.Count > 0 && passwordKey[0] != null && passwordKey[0] == secretKeyConst)
            {
                await _next.Invoke(context);

            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
