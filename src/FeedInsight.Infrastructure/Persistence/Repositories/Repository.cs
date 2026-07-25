using Ardalis.Specification.EntityFrameworkCore;
using FeedInsight.Application.Common.Interfaces;
using FeedInsight.Domain.Common.Models;
using FeedInsight.Infrastructure.Persistence.Context;

namespace FeedInsight.Infrastructure.Persistence.Repositories;

public class Repository<T> : RepositoryBase<T>, IRepository<T> where T : Entity
{
    public Repository(FeedInsightDbContext dbContext) : base(dbContext)
    {
    }
}
