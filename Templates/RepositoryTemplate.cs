namespace BuildQuickPkg.Templates;

/// <summary>
/// Produces a generic, entity-agnostic <c>IRepository&lt;T&gt;</c>/<c>Repository&lt;T&gt;</c> plus
/// <c>IUnitOfWork</c>/<c>UnitOfWork</c>, written by <c>BuildQuickPkg add repository</c> into the
/// same project that already owns the generated <c>DbContext</c>. Entity-agnostic on purpose: this
/// package doesn't know what entities the user has, so <c>Repository&lt;T&gt;</c> works against any
/// <c>DbSet&lt;T&gt;</c>, and <c>UnitOfWork</c> hands out one cached <c>Repository&lt;T&gt;</c> per
/// entity type instead of requiring DI to resolve an open generic.
/// </summary>
internal static class RepositoryTemplate
{
    public static string IRepository(string repositoriesNamespace) => $$"""
        namespace {{repositoriesNamespace}};

        public interface IRepository<T> where T : class
        {
            Task<T?> GetByIdAsync(params object[] keyValues);
            Task<IReadOnlyList<T>> GetAllAsync();
            Task AddAsync(T entity);
            void Update(T entity);
            void Remove(T entity);
        }
        """;

    public static string Repository(string repositoriesNamespace) => $$"""
        using Microsoft.EntityFrameworkCore;

        namespace {{repositoriesNamespace}};

        public class Repository<T> : IRepository<T> where T : class
        {
            private readonly DbContext _context;
            private readonly DbSet<T> _dbSet;

            public Repository(DbContext context)
            {
                _context = context;
                _dbSet = context.Set<T>();
            }

            public async Task<T?> GetByIdAsync(params object[] keyValues) => await _dbSet.FindAsync(keyValues);

            public async Task<IReadOnlyList<T>> GetAllAsync() => await _dbSet.ToListAsync();

            public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

            public void Update(T entity) => _dbSet.Update(entity);

            public void Remove(T entity) => _dbSet.Remove(entity);
        }
        """;

    public static string IUnitOfWork(string repositoriesNamespace) => $$"""
        namespace {{repositoriesNamespace}};

        public interface IUnitOfWork : IDisposable
        {
            IRepository<T> Repository<T>() where T : class;
            Task<int> SaveChangesAsync();
        }
        """;

    public static string UnitOfWork(string repositoriesNamespace, string dbContextName, string dbContextNamespace) => $$"""
        using {{dbContextNamespace}};

        namespace {{repositoriesNamespace}};

        public class UnitOfWork : IUnitOfWork
        {
            private readonly {{dbContextName}} _context;
            private readonly Dictionary<Type, object> _repositories = new();

            public UnitOfWork({{dbContextName}} context)
            {
                _context = context;
            }

            public IRepository<T> Repository<T>() where T : class
            {
                var type = typeof(T);
                if (!_repositories.TryGetValue(type, out var repository))
                {
                    repository = new Repository<T>(_context);
                    _repositories[type] = repository;
                }

                return (IRepository<T>)repository;
            }

            public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();

            public void Dispose() => _context.Dispose();
        }
        """;
}
