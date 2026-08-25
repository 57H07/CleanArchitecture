using CleanArchitecture.Application.DependencyInjection;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.DependencyInjection;
using CleanArchitecture.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Ensure database is created and migrated => To Remove and use EF Migrations 
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
