using System;

namespace FeedInsight.Application.Common.Models;

public record ExtractedTaskPayload(Guid TenantId, Guid? CategoryId, string Text);
