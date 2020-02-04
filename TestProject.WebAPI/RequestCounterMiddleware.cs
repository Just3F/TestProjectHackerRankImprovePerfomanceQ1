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
            _counter++;
            context.Response.Headers.Add("requestCounter", _counter.ToString());
            await _next.Invoke(context);
        }
    }
}
