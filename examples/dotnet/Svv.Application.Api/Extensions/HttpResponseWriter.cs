using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using Svv.Application.Api.Models;


namespace Svv.Application.Api.Extensions
{
    public static class HttpResponseWriter
    {
        private static readonly JsonSerializerSettings Settings;
        private static readonly string Version;
        static HttpResponseWriter()
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            Settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Formatting = Formatting.Indented
            };

            Settings.Converters.Add(new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()});
        }
        public static Task WriteHealthLivenessResponse(HttpContext httpContext,
            HealthReport result)
        {
            var health = new ReadinessHealthReport
            {
                Version = Version,
                Status = result.Status,
            };
            var json = JsonConvert.SerializeObject(health, Settings);

            return WriteStringResponse(httpContext, json);
        }

        public static Task WriteHealthReadinessResponse(HttpContext httpContext,
            HealthReport result)
        {
            var json = JsonConvert.SerializeObject(result, Settings);
            return WriteStringResponse(httpContext, json);
        }


        public static Task WriteExceptionResponse(HttpContext httpContext, object ex)
        {
            var json = JsonConvert.SerializeObject(ex, Settings);

            return WriteStringResponse(httpContext, json);
        }


        public static Task WriteStringResponse(HttpContext httpContext, string result)
        {
            httpContext.Response.ContentType = MediaTypeNames.Application.Json;

            return httpContext.Response.WriteAsync(result);
        }
    }
}
