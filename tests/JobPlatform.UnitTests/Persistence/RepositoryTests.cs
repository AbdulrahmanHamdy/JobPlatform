using JobPlatform.Domain.Common;
using JobPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.UnitTests.Persistence;

// A simple concrete entity for testing
public class TestEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}

// A test-specific DbContext that adds TestEntity to the model
public class TestDbContext : ApplicationDbContext
{
    public DbSet<TestEntity> TestEntities => Set<TestEntity>();

    public TestDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<TestEntity>();
    }
}

public static class TestDbContextFactory
{
    public static TestDbContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var ctx = new TestDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}

public class RepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAddEntityToDatabase()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(AddAsync_ShouldAddEntityToDatabase));
        var repo = new Repository<TestEntity>(context);
        var entity = new TestEntity { Name = "Test Job" };

        // Act
        await repo.AddAsync(entity);
        await context.SaveChangesAsync();

        // Assert
        var saved = await context.TestEntities.FirstOrDefaultAsync(e => e.Id == entity.Id);
        Assert.NotNull(saved);
        Assert.Equal("Test Job", saved.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectEntity()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(GetByIdAsync_ShouldReturnCorrectEntity));
        var repo = new Repository<TestEntity>(context);
        var entity = new TestEntity { Name = "Find Me" };
        await repo.AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetByIdAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal("Find Me", result.Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedEntities()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(GetAllAsync_ShouldReturnAllNonDeletedEntities));
        var repo = new Repository<TestEntity>(context);
        await repo.AddAsync(new TestEntity { Name = "Alpha" });
        await repo.AddAsync(new TestEntity { Name = "Beta" });
        await context.SaveChangesAsync();

        // Act
        var all = await repo.GetAllAsync();

        // Assert
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task Remove_ShouldSoftDeleteEntity()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(Remove_ShouldSoftDeleteEntity));
        var repo = new Repository<TestEntity>(context);
        var entity = new TestEntity { Name = "Delete Me" };
        await repo.AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        repo.Remove(entity);
        await context.SaveChangesAsync();

        // Assert - global filter hides soft-deleted entities
        var result = await repo.GetByIdAsync(entity.Id);
        Assert.Null(result);

        // But still exists in raw DB
        var raw = await context.TestEntities
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == entity.Id);
        Assert.NotNull(raw);
        Assert.True(raw.IsDeleted);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnFilteredEntities()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(FindAsync_ShouldReturnFilteredEntities));
        var repo = new Repository<TestEntity>(context);
        await repo.AddAsync(new TestEntity { Name = "Alpha" });
        await repo.AddAsync(new TestEntity { Name = "Beta" });
        await repo.AddAsync(new TestEntity { Name = "Alpha Two" });
        await context.SaveChangesAsync();

        // Act
        var results = await repo.FindAsync(e => e.Name.StartsWith("Alpha"));

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.StartsWith("Alpha", r.Name));
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrueWhenEntityExists()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(ExistsAsync_ShouldReturnTrueWhenEntityExists));
        var repo = new Repository<TestEntity>(context);
        await repo.AddAsync(new TestEntity { Name = "Exists" });
        await context.SaveChangesAsync();

        // Act & Assert
        Assert.True(await repo.ExistsAsync(e => e.Name == "Exists"));
        Assert.False(await repo.ExistsAsync(e => e.Name == "DoesNotExist"));
    }
}

public class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_ShouldPersistEntities()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(SaveChangesAsync_ShouldPersistEntities));
        var uow = new UnitOfWork(context);
        var repo = new Repository<TestEntity>(context);

        // Act
        await repo.AddAsync(new TestEntity { Name = "Via UoW" });
        var count = await uow.SaveChangesAsync();

        // Assert
        Assert.Equal(1, count);
        Assert.Equal(1, await context.TestEntities.CountAsync());
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSetCreatedAt_OnAdd()
    {
        // Arrange
        using var context = TestDbContextFactory.Create(nameof(SaveChangesAsync_ShouldSetCreatedAt_OnAdd));
        var uow = new UnitOfWork(context);
        var repo = new Repository<TestEntity>(context);
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var entity = new TestEntity { Name = "Timestamp Test" };
        await repo.AddAsync(entity);
        await uow.SaveChangesAsync();

        // Assert
        var saved = await repo.GetByIdAsync(entity.Id);
        Assert.NotNull(saved);
        Assert.True(saved.CreatedAt >= before, "CreatedAt should be set on add.");
        Assert.Null(saved.UpdatedAt);
    }
}
