using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Application.Features.Users.Queries.GetProfile
{
    public record ProfileDto(
      string FirstName,
      string LastName,
      string CompanyName);
}
