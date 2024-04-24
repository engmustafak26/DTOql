using Demo.Domain;
using Demo.Infrastructure;
using Demo.Miselaneous;
using DTOql.Extensions;

namespace Demo.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string APIKEY = "XApiKey";
        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, DataContext dataContext, AppExecutionContext executionContext)
        {
            if (!context.Request.Headers.TryGetValue(APIKEY, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Api Key was not provided ");
                return;
            }

            if (extractedApiKey.ToString().IsEqual("global.admin"))
            {
                executionContext.UserType = UserType.WebsiteAdmin;

            }
            else
            {

                var user = dataContext.Users.FirstOrDefault(x => x.ApiKey == extractedApiKey.ToString());

                if (user is null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized client");
                    return;
                }

                executionContext.UserId = user.Id;
                executionContext.OrganizationId = user.OrganizationId;
                executionContext.UserType = user.IsOrganizationAdmin? UserType.OrganizationAdmin: UserType.NormalUser;
            }



            await _next(context);
        }
    }
}


