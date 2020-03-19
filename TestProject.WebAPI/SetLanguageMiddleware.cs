using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TestProject.WebAPI
{
    public class SetLanguageMiddleware
    {
        private readonly RequestDelegate _next;

        public SetLanguageMiddleware(RequestDelegate next)
        {
            this._next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var headers = context.Request.Headers.ToList();
            var langHeader = headers.FirstOrDefault(x => x.Key == "Accept-Language");
            try
            {
                var newLangValue = "en";
                if (langHeader.Value.Count != 0)
                    newLangValue = langHeader.Value.First();
                var newCulture = new CultureInfo(newLangValue);
                CultureInfo.CurrentCulture = newCulture;
                CultureInfo.CurrentUICulture = newCulture;
            }
            catch (CultureNotFoundException) { }

            await _next.Invoke(context);

        }
    }
}
