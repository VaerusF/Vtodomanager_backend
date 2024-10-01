using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VtodoManager.NewsService.UseCases.Handlers.News.Dto;
using VtodoManager.NewsService.UseCases.Handlers.News.Queries.GetOneNews;
using VtodoManager.NewsService.UseCases.Handlers.News.Queries.GetPagedNews;

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

    /// <summary>
    /// Get news by page count and count on page
    /// </summary>
    /// <param name="pagedNewsDto">Dto with page count and count on page</param>
    /// <response code="200">Returns list news dto</response>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<List<NewsDto>?> GetPagedNews([FromQuery] GetPagedNewsDto pagedNewsDto)
    {
        return await _mediator.Send(new GetPagedNewsRequest() { GetPagedNewsDto = pagedNewsDto});
    }
    
    /// <summary>
    /// Get single news
    /// </summary>
    /// <param name="newsId"></param>
    /// <response code="200">Returns news dto</response>
    /// <response code="404">News not found</response>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("{newsId:long}")]
    public async Task<NewsDto?> GetPagedNews(long newsId)
    {
        return await _mediator.Send(new GetOneNewsRequest() { NewsId = newsId});
    }
}