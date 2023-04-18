using Microsoft.AspNetCore.Mvc;
using Supabase.Storage;
using Supabase.Tutorial.Contracts;
using Supabase.Tutorial.Models;
using FileOptions = Supabase.Storage.FileOptions;

namespace Supabase.Tutorial.Controllers;

[Route("newsletters")]
[ApiController]
public class NewslettersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateNewsletter(
        [FromForm] CreateNewsletterRequest request,
        Supabase.Client client)
    {
        var newsletter = new Newsletter
        {
            Name = request.Name,
            Description = request.Description,
            ReadTime = request.ReadTime,
            CreatedAt = DateTime.Now
        };

        var response = await client.From<Newsletter>().Insert(newsletter);

        var newNewsletter = response.Models.First();

        using var memoryStream = new MemoryStream();

        await request.CoverImage.CopyToAsync(memoryStream);

        var lastIndexOfDot = request.CoverImage.FileName.LastIndexOf('.');

        string extension = request.CoverImage.FileName.Substring(lastIndexOfDot + 1);

        await client.Storage.From("cover-images").Upload(
            memoryStream.ToArray(),
            $"newsletter-{newNewsletter.Id}.{extension}");

        return Ok(newNewsletter.Id);
    }
}
