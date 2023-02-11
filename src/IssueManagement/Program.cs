using IssueManagement;
using IssueManagement.Core;
using IssueManagement.Features.CreateIssue;
using Marten;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Mvc.Razor;
using Serilog;
using Serilog.Extensions.Logging;
using Weasel.Core;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);
builder.Host.AddLogging();
var logger = new SerilogLoggerFactory(Log.Logger)
  .CreateLogger<Program>();

// Add services to the container.
builder.Services.AddControllersWithViews()
  .AddRazorRuntimeCompilation();

builder.Services.Configure<RazorViewEngineOptions>(
  o => o.ViewLocationExpanders.Add(new FeatureFolderLocationExpander())
);

builder.Services.AddMarten(
  options =>
  {
    options.Connection(builder.Configuration.GetConnectionString("EventStore"));
    options.AutoCreateSchemaObjects = AutoCreate.All;

    options.Projections.SelfAggregate<Issue>(ProjectionLifecycle.Inline);
  }
);
builder.Host.UseWolverine();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
  name: "default",
  pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
