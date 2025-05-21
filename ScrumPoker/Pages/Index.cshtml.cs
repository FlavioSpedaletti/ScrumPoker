using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ScrumPoker.Pages;

public class IndexModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? RoomId { get; set; }

    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        RoomId = RouteData.Values["roomId"]?.ToString();
    }
}
