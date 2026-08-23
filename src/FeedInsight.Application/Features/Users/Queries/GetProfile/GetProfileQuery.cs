using ErrorOr;
using FeedInsight.Application.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Users.Queries.GetProfile
{

    public record GetProfileQuery
        : IRequest<ErrorOr<ProfileDto>>;
}
