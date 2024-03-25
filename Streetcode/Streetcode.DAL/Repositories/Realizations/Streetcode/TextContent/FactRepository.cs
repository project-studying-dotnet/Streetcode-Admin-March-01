using Microsoft.EntityFrameworkCore;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Persistence;
using Streetcode.DAL.Repositories.Interfaces.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Realizations.Base;

namespace Streetcode.DAL.Repositories.Realizations.Streetcode.TextContent;

public class FactRepository : RepositoryBase<Fact>, IFactRepository
{
    private readonly StreetcodeDbContext _context;
    public FactRepository(StreetcodeDbContext streetcodeDbContext)
        : base(streetcodeDbContext)
    {
        _context = streetcodeDbContext;
    }

    public Task<int> GetMaxNumberAsync()
    {
        return _context.Facts.MaxAsync(f => f.Number);
    }
}