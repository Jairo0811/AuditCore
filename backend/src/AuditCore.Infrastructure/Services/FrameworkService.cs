using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Frameworks;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class FrameworkService : IFrameworkService
{
    private readonly IAuditCoreDbContext _dbContext;
    private readonly TenantGuard _tenantGuard;

    public FrameworkService(IAuditCoreDbContext dbContext, TenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<IReadOnlyCollection<FrameworkDto>> GetFrameworksAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.ControlFrameworks.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new FrameworkDto(x.Id, x.Name, x.Code, x.Version, x.Description, x.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<FrameworkDto> CreateFrameworkAsync(CreateFrameworkRequest request, CancellationToken cancellationToken = default)
    {
        EnsureGlobalConfigurationAccess();
        var code = NormalizeCode(request.Code);
        var version = NormalizeRequired(request.Version, nameof(request.Version));
        if (await _dbContext.ControlFrameworks.AnyAsync(x => x.Code == code && x.Version == version, cancellationToken))
            throw new InvalidOperationException("Ya existe esta versión del marco de control.");
        var entity = new ControlFramework(request.Name, code, version, request.Description);
        _dbContext.ControlFrameworks.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new FrameworkDto(entity.Id, entity.Name, entity.Code, entity.Version, entity.Description, entity.IsActive);
    }

    public async Task<FrameworkDto?> UpdateFrameworkAsync(Guid id, UpdateFrameworkRequest request, CancellationToken cancellationToken = default)
    {
        EnsureGlobalConfigurationAccess();
        var entity = await _dbContext.ControlFrameworks.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return null;
        var code = NormalizeCode(request.Code);
        var version = NormalizeRequired(request.Version, nameof(request.Version));
        if (await _dbContext.ControlFrameworks.AnyAsync(x => x.Id != id && x.Code == code && x.Version == version, cancellationToken))
            throw new InvalidOperationException("Ya existe esta versión del marco de control.");
        entity.Update(request.Name, code, version, request.Description, request.IsActive);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new FrameworkDto(entity.Id, entity.Name, entity.Code, entity.Version, entity.Description, entity.IsActive);
    }

    public async Task<IReadOnlyCollection<ControlDto>> GetControlsAsync(Guid? frameworkId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ControlDefinitions.AsNoTracking().AsQueryable();
        if (frameworkId.HasValue) query = query.Where(x => x.FrameworkId == frameworkId.Value);
        return await query.OrderBy(x => x.Domain).ThenBy(x => x.Code)
            .Select(x => new ControlDto(x.Id, x.FrameworkId, x.Code, x.Title, x.Domain, x.Description, x.Weight, x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<ControlDto> CreateControlAsync(CreateControlRequest request, CancellationToken cancellationToken = default)
    {
        EnsureGlobalConfigurationAccess();
        if (!await _dbContext.ControlFrameworks.AnyAsync(x => x.Id == request.FrameworkId, cancellationToken))
            throw new InvalidOperationException("El marco de control no existe.");
        var code = NormalizeCode(request.Code);
        if (await _dbContext.ControlDefinitions.AnyAsync(x => x.FrameworkId == request.FrameworkId && x.Code == code, cancellationToken))
            throw new InvalidOperationException("El código de control ya existe en el marco.");
        var entity = new ControlDefinition(request.FrameworkId, code, request.Title, request.Domain, request.Weight, request.Description);
        _dbContext.ControlDefinitions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ControlDto(entity.Id, entity.FrameworkId, entity.Code, entity.Title, entity.Domain, entity.Description, entity.Weight, entity.IsActive);
    }

    public async Task<ControlDto?> UpdateControlAsync(Guid id, UpdateControlRequest request, CancellationToken cancellationToken = default)
    {
        EnsureGlobalConfigurationAccess();
        var entity = await _dbContext.ControlDefinitions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return null;
        var code = NormalizeCode(request.Code);
        if (await _dbContext.ControlDefinitions.AnyAsync(x => x.Id != id && x.FrameworkId == entity.FrameworkId && x.Code == code, cancellationToken))
            throw new InvalidOperationException("El código de control ya existe en el marco.");
        entity.Update(code, request.Title, request.Domain, request.Weight, request.Description, request.IsActive);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ControlDto(entity.Id, entity.FrameworkId, entity.Code, entity.Title, entity.Domain, entity.Description, entity.Weight, entity.IsActive);
    }

    public async Task<IReadOnlyCollection<QuestionDto>> GetQuestionsAsync(Guid controlId, CancellationToken cancellationToken = default) =>
        await _dbContext.ControlQuestions.AsNoTracking().Where(x => x.ControlId == controlId).OrderBy(x => x.Order)
            .Select(x => new QuestionDto(x.Id, x.ControlId, x.Text, x.Weight, x.Order, x.IsRequired, x.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<QuestionDto> CreateQuestionAsync(CreateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        EnsureGlobalConfigurationAccess();
        if (!await _dbContext.ControlDefinitions.AnyAsync(x => x.Id == request.ControlId, cancellationToken))
            throw new InvalidOperationException("El control no existe.");
        if (await _dbContext.ControlQuestions.AnyAsync(x => x.ControlId == request.ControlId && x.Order == request.Order, cancellationToken))
            throw new InvalidOperationException("Ya existe una pregunta con este orden en el control.");
        var entity = new ControlQuestion(request.ControlId, request.Text, request.Weight, request.Order, request.IsRequired);
        _dbContext.ControlQuestions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new QuestionDto(entity.Id, entity.ControlId, entity.Text, entity.Weight, entity.Order, entity.IsRequired, entity.IsActive);
    }

    public async Task<QuestionDto?> UpdateQuestionAsync(Guid id, UpdateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        EnsureGlobalConfigurationAccess();
        var entity = await _dbContext.ControlQuestions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return null;
        if (await _dbContext.ControlQuestions.AnyAsync(x => x.Id != id && x.ControlId == entity.ControlId && x.Order == request.Order, cancellationToken))
            throw new InvalidOperationException("Ya existe una pregunta con este orden en el control.");
        entity.Update(request.Text, request.Weight, request.Order, request.IsRequired, request.IsActive);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new QuestionDto(entity.Id, entity.ControlId, entity.Text, entity.Weight, entity.Order, entity.IsRequired, entity.IsActive);
    }

    public async Task<IReadOnlyCollection<EvaluationDto>> GetEvaluationsAsync(Guid auditId, CancellationToken cancellationToken = default)
    {
        _tenantGuard.EnsureOrganization(await GetAuditOrganizationAsync(auditId, cancellationToken));
        return await _dbContext.ControlEvaluations.AsNoTracking().Where(x => x.AuditId == auditId).OrderBy(x => x.Control.Code)
            .Select(x => new EvaluationDto(x.Id, x.AuditId, x.ControlId, x.Control.Code, x.Score, x.Status, x.Notes, x.EvaluatedByUserId, x.EvaluatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<EvaluationDto> EvaluateAsync(Guid auditId, Guid controlId, EvaluateControlRequest request, CancellationToken cancellationToken = default)
    {
        var audit = await _dbContext.Audits.AsNoTracking().SingleOrDefaultAsync(x => x.Id == auditId, cancellationToken)
            ?? throw new InvalidOperationException("La auditoría no existe.");
        _tenantGuard.EnsureOrganization(audit.OrganizationId);
        if (!await _dbContext.ControlDefinitions.AnyAsync(x => x.Id == controlId && x.IsActive, cancellationToken))
            throw new InvalidOperationException("El control no existe o está inactivo.");
        if (!await _dbContext.Users.AnyAsync(x => x.Id == request.EvaluatedByUserId && x.OrganizationId == audit.OrganizationId && x.IsActive && !x.IsLocked, cancellationToken))
            throw new InvalidOperationException("El evaluador no es válido para esta organización.");

        var entity = await _dbContext.ControlEvaluations.SingleOrDefaultAsync(x => x.AuditId == auditId && x.ControlId == controlId, cancellationToken);
        if (entity is null)
        {
            entity = new ControlEvaluation(auditId, controlId);
            _dbContext.ControlEvaluations.Add(entity);
        }
        entity.Evaluate(request.Score, request.Status, request.Notes, request.EvaluatedByUserId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        var controlCode = await _dbContext.ControlDefinitions.Where(x => x.Id == controlId).Select(x => x.Code).SingleAsync(cancellationToken);
        return new EvaluationDto(entity.Id, entity.AuditId, entity.ControlId, controlCode, entity.Score, entity.Status, entity.Notes, entity.EvaluatedByUserId, entity.EvaluatedAtUtc);
    }

    public async Task<IReadOnlyCollection<AnswerDto>> GetAnswersAsync(Guid evaluationId, CancellationToken cancellationToken = default)
    {
        var organizationId = await _dbContext.ControlEvaluations.Where(x => x.Id == evaluationId)
            .Select(x => x.Audit.OrganizationId).SingleOrDefaultAsync(cancellationToken);
        if (organizationId == Guid.Empty) throw new InvalidOperationException("La evaluación no existe.");
        _tenantGuard.EnsureOrganization(organizationId);
        return await _dbContext.ControlAnswers.AsNoTracking().Where(x => x.EvaluationId == evaluationId)
            .OrderBy(x => x.Question.Order)
            .Select(x => new AnswerDto(x.Id, x.EvaluationId, x.QuestionId, x.Score, x.Notes))
            .ToListAsync(cancellationToken);
    }

    public async Task<AnswerDto> UpsertAnswerAsync(Guid evaluationId, Guid questionId, UpsertAnswerRequest request, CancellationToken cancellationToken = default)
    {
        var evaluation = await _dbContext.ControlEvaluations.SingleOrDefaultAsync(x => x.Id == evaluationId, cancellationToken)
            ?? throw new InvalidOperationException("La evaluación no existe.");
        _tenantGuard.EnsureOrganization(await GetAuditOrganizationAsync(evaluation.AuditId, cancellationToken));
        var questionBelongs = await _dbContext.ControlQuestions.AnyAsync(x => x.Id == questionId && x.ControlId == evaluation.ControlId && x.IsActive, cancellationToken);
        if (!questionBelongs) throw new InvalidOperationException("La pregunta no pertenece al control evaluado o está inactiva.");

        var entity = await _dbContext.ControlAnswers.SingleOrDefaultAsync(x => x.EvaluationId == evaluationId && x.QuestionId == questionId, cancellationToken);
        if (entity is null)
        {
            entity = new ControlAnswer(evaluationId, questionId, request.Score, request.Notes);
            _dbContext.ControlAnswers.Add(entity);
        }
        else
        {
            entity.Update(request.Score, request.Notes);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await RecalculateEvaluationAsync(evaluation, cancellationToken);
        return new AnswerDto(entity.Id, entity.EvaluationId, entity.QuestionId, entity.Score, entity.Notes);
    }

    private async Task RecalculateEvaluationAsync(ControlEvaluation evaluation, CancellationToken cancellationToken)
    {
        if (!evaluation.EvaluatedByUserId.HasValue) return;

        var questions = await _dbContext.ControlQuestions.AsNoTracking()
            .Where(x => x.ControlId == evaluation.ControlId && x.IsActive)
            .Select(x => new { x.Id, x.Weight, x.IsRequired })
            .ToListAsync(cancellationToken);

        if (questions.Count == 0) return;

        var answers = await _dbContext.ControlAnswers.AsNoTracking()
            .Where(x => x.EvaluationId == evaluation.Id)
            .Select(x => new { x.QuestionId, x.Score })
            .ToListAsync(cancellationToken);

        var answerMap = answers.ToDictionary(x => x.QuestionId, x => x.Score);
        var hasMissingRequired = questions
            .Where(x => x.IsRequired)
            .Any(x => !answerMap.TryGetValue(x.Id, out var score) || score is null);

        if (hasMissingRequired)
        {
            evaluation.Evaluate(null, ComplianceStatus.NotEvaluated, evaluation.Notes, evaluation.EvaluatedByUserId.Value);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var scoredQuestions = questions
            .Where(x => answerMap.TryGetValue(x.Id, out var score) && score.HasValue)
            .Select(x => new { x.Weight, Score = answerMap[x.Id]!.Value })
            .ToArray();

        if (scoredQuestions.Length == 0)
        {
            evaluation.Evaluate(null, ComplianceStatus.NotEvaluated, evaluation.Notes, evaluation.EvaluatedByUserId.Value);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var totalWeight = scoredQuestions.Sum(x => x.Weight);
        if (totalWeight <= 0) throw new InvalidOperationException("El peso total de las preguntas debe ser mayor que cero.");

        var weightedScore = scoredQuestions.Sum(x => x.Score * x.Weight) / totalWeight;
        var scoreValue = (int)Math.Round(weightedScore, MidpointRounding.AwayFromZero);
        var status = scoreValue switch
        {
            >= 80 => ComplianceStatus.Compliant,
            >= 50 => ComplianceStatus.PartiallyCompliant,
            _ => ComplianceStatus.NonCompliant
        };

        evaluation.Evaluate(scoreValue, status, evaluation.Notes, evaluation.EvaluatedByUserId.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Guid> GetAuditOrganizationAsync(Guid auditId, CancellationToken cancellationToken)
    {
        var organizationId = await _dbContext.Audits.Where(x => x.Id == auditId)
            .Select(x => x.OrganizationId).SingleOrDefaultAsync(cancellationToken);
        if (organizationId == Guid.Empty) throw new InvalidOperationException("La auditoría no existe.");
        return organizationId;
    }

    private void EnsureGlobalConfigurationAccess()
    {
        if (_tenantGuard.RestrictedOrganizationId.HasValue)
            throw new UnauthorizedAccessException("Solo un superadministrador puede modificar marcos globales.");
    }

    private static string NormalizeCode(string value) => NormalizeRequired(value, nameof(value)).ToUpperInvariant();

    private static string NormalizeRequired(string value, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        return value.Trim();
    }
}
