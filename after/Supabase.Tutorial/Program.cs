using Supabase;
using Supabase.Tutorial.Contracts;
using Supabase.Tutorial.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<Supabase.Client>(_ =>
    new Supabase.Client(
        builder.Configuration["SupabaseUrl"],
        builder.Configuration["SupabaseKey"],
        new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true
        }));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/newsletters/{id}", async (long id, Supabase.Client client) =>
{
    var response = await client
        .From<Newsletter>()
        .Where(n => n.Id == id)
        .Get();

    var newsletter = response.Models.FirstOrDefault();

    if (newsletter is null)
    {
        return Results.NotFound();
    }

    var newsletterResponse = new NewsletterResponse
    {
        Id = newsletter.Id,
        Name = newsletter.Name,
        Description = newsletter.Description,
        ReadTime = newsletter.ReadTime,
        CreatedAt = newsletter.CreatedAt,
        CoverImageUrl = client.Storage.From("cover-images")
            .GetPublicUrl($"newsletter-{id}.png")
    };

    return Results.Ok(newsletterResponse);
});

app.MapDelete("/newsletters/{id}", async (long id, Supabase.Client client) =>
{
    await client
        .From<Newsletter>()
        .Where(n => n.Id == id)
        .Delete();

    await client.Storage.From("cover-images")
        .Remove(new List<string> { $"newsletter-{id}.png" });

    return Results.NoContent();
});

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
