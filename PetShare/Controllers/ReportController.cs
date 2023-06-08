using Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetShare.Models;
using PetShare.Models.Reports;
using PetShare.Services.Interfaces.Pagination;
using PetShare.Services.Interfaces.Reports;

namespace PetShare.Controllers;

[ApiController]
[Route("reports")]
public class ReportController : ControllerBase
{
    private readonly IReportCommand _command;
    private readonly IPaginationService _paginator;
    private readonly IReportQuery _query;

    public ReportController(IReportQuery query, IReportCommand command, IPaginationService paginator)
    {
        _query = query;
        _command = command;
        _paginator = paginator;
    }

    /// <summary>
    ///     Gets all unanswered reports (with status 'new')
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ReportPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReportPageResponse>> GetAll([FromQuery] PaginationQueryRequest request)
    {
        var reports = await _query.GetNewReportsAsync(HttpContext.RequestAborted);
        var page = _paginator.GetPage(reports, request);
        if (page is null)
            return BadRequest("Wrong pagination parameters");

        return new ReportPageResponse
        {
            Reports = page.Items.Select(report => report.ToResponse()).ToList(),
            PageNumber = page.PageNumber,
            Count = page.TotalCount
        };
    }

    /// <summary>
    ///     Creates new report
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Post(ReportRequest request)
    {
        var report = Report.FromRequest(request);
        var result = await _command.AddAsync(report);
        return result.HasValue
            ? Created(new Uri(report.Id.ToString(), UriKind.Relative), report)
            : result.State.ToActionResult();
    }

    /// <summary>
    ///     Updates report state
    /// </summary>
    [HttpPut]
    [Route("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReportResponse>> Put(Guid id, ReportUpdateRequest request)
    {
        var state = Enum.Parse<ReportState>(request.State, true);
        var result = await _command.UpdateStateAsync(id, state);
        return result.HasValue ? result.Value.ToResponse() : result.State.ToActionResult();
    }
}
