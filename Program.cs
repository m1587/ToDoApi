using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using TodoApi;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddEndpointsApiExplorer();
var connectionString = builder.Configuration.GetConnectionString("ToDoDB");
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
var app = builder.Build();
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI(
        options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        options.RoutePrefix = string.Empty; 
    }
    );
// }
app.UseCors("AllowAll");
app.MapGet("/items", async (ToDoDbContext db) =>
{
    return Results.Ok(await db.Items.ToListAsync());
});

app.MapPost("/items", async (Item newItem, ToDoDbContext db) =>
{
    db.Items.Add(newItem);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{newItem.Id}", newItem);
});

app.MapPut("/items/{id}", async (int id, Item updatedItem, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null)
        return Results.NotFound();

    item.Name = updatedItem.Name;
    item.IsComplete = updatedItem.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent(); 
});

app.MapDelete("/items/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null)
        return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapGet("/",() =>"Api is running");
 app.Run();

