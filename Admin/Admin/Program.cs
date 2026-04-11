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

app.MapGet("/api/pois", async (Admin.Data.ApplicationDbContext db) => {
    // Lấy dữ liệu từ SQL và trả về dạng JSON
    return Results.Ok(await db.TourismSites.ToListAsync());
}).RequireCors("AllowAll");

app.MapGet("/api/pois/{langId}", async (int langId, Admin.Data.ApplicationDbContext db) => {
    var data = await (from site in db.TourismSites
                      join trans in db.SiteTranslations
                        on site.SiteID equals trans.SiteID
                      join lang in db.Languages
                        on trans.LangID equals lang.LangID
                      where trans.LangID == langId // Lọc theo LangID từ MAUI gửi lên
                      select new
                      {
                          site.SiteID,
                          site.TourismName,
                          site.Address,
                          site.Latitude,
                          site.Longitude,
                          site.Version,
                          QuickInfo = trans.QuickInfo // Lấy mô tả theo ngôn ngữ
                      }).ToListAsync();

    return Results.Ok(data);
}).RequireCors("AllowAll");

app.Run();
