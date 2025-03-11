using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);
// פתרון בעית ה cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("tododb"),
    new MySqlServerVersion(new Version(8, 0, 0))));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowAllOrigins");

// הפעל  Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger/index.html"));


//שולף את כל המשימות
app.MapGet("/getItems", async (ToDoDbContext context) =>
{
    var items = await context.Items.ToListAsync();
    return items;
});

//מוסיף משימה חדשה
app.MapPost("/addItem", async (ToDoDbContext context, [FromBody] string name) =>
{
    Item item = new Item
    {
        Name = name,
        IsComplete = false
    };
    context.Items.Add(item);

    await context.SaveChangesAsync();

    return Results.Created($"/items/{item.Id}", item);
});

// מעדכן את סטטוס המשימה
app.MapPut("/updateItem", async (ToDoDbContext context, int id, bool? IsComplete) =>
{
    var item = await context.Items.FindAsync(id);
    if (item is null)
    {
        return Results.NotFound();
    }

    if (item.Id == id)
    {
        if (IsComplete != null)
        {
            item.IsComplete = IsComplete;
        }
    }

    await context.SaveChangesAsync();
    return Results.NoContent();

});
// מוחק משימה
app.MapDelete("/deleteItem", async (ToDoDbContext context, int id) =>
{
    var item = await context.Items.FindAsync(id);
    if (item is null)
    {
        return Results.NotFound();
    }
    context.Items.Remove(item);
    await context.SaveChangesAsync();
    return Results.NoContent();
});


app.Run();
