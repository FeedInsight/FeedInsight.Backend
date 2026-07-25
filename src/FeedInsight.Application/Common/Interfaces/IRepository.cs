using Ardalis.Specification;
using FeedInsight.Domain.Common.Models;

namespace FeedInsight.Application.Common.Interfaces;

public interface IRepository<T> : IRepositoryBase<T> where T : Entity
{
}
