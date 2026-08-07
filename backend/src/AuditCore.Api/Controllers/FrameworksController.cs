using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Frameworks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditCore.Api.Controllers;

[ApiController]
[Route("api/frameworks")]
[Authorize]
public sealed class FrameworksController : ControllerBase
{
    private readonly IFrameworkService _service;
    public FrameworksController(IFrameworkService service) => _service = service;

    [HttpGet]
    [Authorize(Policy = PermissionCodes.FrameworksView)]
    public async Task<ActionResult<IReadOnlyCollection<FrameworkDto>>> GetFrameworks(CancellationToken cancellationToken) =>
        Ok(await _service.GetFrameworksAsync(cancellationToken));

    [HttpPost]
    [Authorize(Policy = PermissionCodes.FrameworksManage)]
    public async Task<ActionResult<FrameworkDto>> CreateFramework(CreateFrameworkRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateFrameworkAsync(request, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.FrameworksManage)]
    public async Task<ActionResult<FrameworkDto>> UpdateFramework(Guid id, UpdateFrameworkRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateFrameworkAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("controls")]
    [Authorize(Policy = PermissionCodes.FrameworksView)]
    public async Task<ActionResult<IReadOnlyCollection<ControlDto>>> GetControls([FromQuery] Guid? frameworkId, CancellationToken cancellationToken) =>
        Ok(await _service.GetControlsAsync(frameworkId, cancellationToken));

    [HttpPost("controls")]
    [Authorize(Policy = PermissionCodes.FrameworksManage)]
    public async Task<ActionResult<ControlDto>> CreateControl(CreateControlRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateControlAsync(request, cancellationToken));

    [HttpPut("controls/{id:guid}")]
    [Authorize(Policy = PermissionCodes.FrameworksManage)]
    public async Task<ActionResult<ControlDto>> UpdateControl(Guid id, UpdateControlRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateControlAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("controls/{controlId:guid}/questions")]
    [Authorize(Policy = PermissionCodes.FrameworksView)]
    public async Task<ActionResult<IReadOnlyCollection<QuestionDto>>> GetQuestions(Guid controlId, CancellationToken cancellationToken) =>
        Ok(await _service.GetQuestionsAsync(controlId, cancellationToken));

    [HttpPost("questions")]
    [Authorize(Policy = PermissionCodes.FrameworksManage)]
    public async Task<ActionResult<QuestionDto>> CreateQuestion(CreateQuestionRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateQuestionAsync(request, cancellationToken));

    [HttpPut("questions/{id:guid}")]
    [Authorize(Policy = PermissionCodes.FrameworksManage)]
    public async Task<ActionResult<QuestionDto>> UpdateQuestion(Guid id, UpdateQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateQuestionAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("evaluations/{auditId:guid}")]
    [Authorize(Policy = PermissionCodes.AuditsView)]
    public async Task<ActionResult<IReadOnlyCollection<EvaluationDto>>> GetEvaluations(Guid auditId, CancellationToken cancellationToken) =>
        Ok(await _service.GetEvaluationsAsync(auditId, cancellationToken));

    [HttpPut("evaluations/{auditId:guid}/{controlId:guid}")]
    [Authorize(Policy = PermissionCodes.AuditsExecute)]
    public async Task<ActionResult<EvaluationDto>> Evaluate(Guid auditId, Guid controlId, EvaluateControlRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.EvaluateAsync(auditId, controlId, request, cancellationToken));

    [HttpGet("evaluations/{evaluationId:guid}/answers")]
    [Authorize(Policy = PermissionCodes.AuditsView)]
    public async Task<ActionResult<IReadOnlyCollection<AnswerDto>>> GetAnswers(Guid evaluationId, CancellationToken cancellationToken) =>
        Ok(await _service.GetAnswersAsync(evaluationId, cancellationToken));

    [HttpPut("evaluations/{evaluationId:guid}/answers/{questionId:guid}")]
    [Authorize(Policy = PermissionCodes.AuditsExecute)]
    public async Task<ActionResult<AnswerDto>> UpsertAnswer(Guid evaluationId, Guid questionId, UpsertAnswerRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.UpsertAnswerAsync(evaluationId, questionId, request, cancellationToken));
}
