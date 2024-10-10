namespace Shared.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync();
    }
}
