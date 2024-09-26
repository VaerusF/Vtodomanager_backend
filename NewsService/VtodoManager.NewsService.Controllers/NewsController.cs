using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace VtodoManager.NewsService.Controllers;

[Route("api/v1/news")]
[ApiController]
public class NewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NewsController(IMediator mediator)
    {
        _mediator = mediator;
    }
}