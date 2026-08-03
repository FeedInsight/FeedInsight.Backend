using FeedInsight.Domain.Tenants.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Tenants.Commands.ToggleTenantStatus
{
    public record ToggleTenantStatusRequest(
     TenantStatus Status,
     string? Reason
 );
}
