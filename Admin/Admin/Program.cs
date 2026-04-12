    using Admin.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCors(options => options.AddPolicy("AllowAll",
    p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/api/pois", async (HttpContext http, int? langId, Admin.Data.ApplicationDbContext db) =>
{
    var baseUrl = $"{http.Request.Scheme}://{http.Request.Host}";

    var data = await (
        from site in db.TourismSites

        join trans in db.SiteTranslations
            on new { site.SiteID, LangID = langId ?? 0 }
            equals new { trans.SiteID, trans.LangID }
            into transGroup

        from trans in transGroup.DefaultIfEmpty()

        select new
        {
            site.SiteID,
            site.TourismName,
            site.Address,
            site.Latitude,
            site.Longitude,

            QuickInfo = trans != null ? trans.QuickInfo : null,

            Images = db.SiteImages
    .Where(img => img.SiteID == site.SiteID)
    .Select(img => new
    {
        ImageURL = baseUrl + img.ImageURL,
        Type = img.ImageType
    })
    .ToList()
        }
    ).ToListAsync();

    return Results.Ok(data);

}).RequireCors("AllowAll");

app.Run();
