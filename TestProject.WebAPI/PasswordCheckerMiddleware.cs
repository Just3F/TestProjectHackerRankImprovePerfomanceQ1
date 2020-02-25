using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TestProject.WebAPI
{
    public class PasswordCheckerMiddleware
    {
        private readonly RequestDelegate _next;

        private int _counter = 0;

        public PasswordCheckerMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string passwordKeyConst = "passwordKey123456789";
            var responseHeaders = context.Request.Headers;
            responseHeaders.TryGetValue("passwordKey", out var passwordKey);
            if (passwordKey.Count > 0 && passwordKey[0] != null && passwordKey[0] == passwordKeyConst)
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
