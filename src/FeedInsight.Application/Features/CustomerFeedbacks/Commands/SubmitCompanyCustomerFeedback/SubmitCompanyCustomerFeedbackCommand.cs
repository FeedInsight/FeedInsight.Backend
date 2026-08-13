using ErrorOr;
using FeedInsight.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.CustomerFeedbacks.Commands.SubmitCompanyCustomerFeedback
{
    public record SubmitCompanyCustomerFeedbackCommand(
   string RawContent,
   string? MetadataJson
) : IRequest<ErrorOr<Guid>>;
}
