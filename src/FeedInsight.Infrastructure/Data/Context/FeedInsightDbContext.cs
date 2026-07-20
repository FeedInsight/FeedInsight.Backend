using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FeedInsight.Infrastructure.Data.Context
{
    public class FeedInsightDbContext: DbContext
    {
        public FeedInsightDbContext(DbContextOptions<FeedInsightDbContext> options): base(options) { }
    }
}
