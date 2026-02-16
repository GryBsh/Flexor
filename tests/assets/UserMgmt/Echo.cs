#:sdk Microsoft.NET.Sdk.Web

var app = WebApplication.CreateBuilder(args).Build();

app.Map("*", Echo);

app.Run();

static async void Echo(HttpContext context)
{
    context.Response.StatusCode = 200;
    foreach (var header in context.Request.Headers)
    {
        context.Response.Headers[header.Key] = header.Value;
    }
    
    await context.Response.WriteAsync(
        await new StreamReader(context.Request.Body).ReadToEndAsync()
    );
}