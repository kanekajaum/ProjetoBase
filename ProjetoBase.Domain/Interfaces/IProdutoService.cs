using ProjetoBase.Domain.Entities;

namespace ProjetoBase.Domain.Interfaces
{
    public interface IProdutoService
    {
        Task<Produto?> GetByIdAsync(int id);
        Task<List<Produto>> GetAllAsync();
        Task AddAsync(Produto produto);
        Task UpdateAsync(Produto produto);
        Task DeleteAsync(int id);
    }
}