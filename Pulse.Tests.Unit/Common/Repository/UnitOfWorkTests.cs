using System.Data.Common;
using Moq;
using Pulse.DAL.Common.Repository;

namespace Pulse.Tests.Unit.Common.Repository;

public class UnitOfWorkTests
{
    private readonly Mock<DbConnection> _connection = new();
    private readonly Mock<DbTransaction> _transaction = new();
    private readonly UnitOfWork _uow;

    public UnitOfWorkTests()
    {
        _transaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _transaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _transaction.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);
        _connection.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);
        _uow = new UnitOfWork(_connection.Object, _transaction.Object);
    }

    [Fact]
    public async Task CommitAsync_CommitsTransaction()
    {
        await _uow.CommitAsync();

        _transaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_WhenNotCommitted_RollsBackAsync()
    {
        await _uow.DisposeAsync();

        _transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_WhenCommitted_DoesNotRollBack()
    {
        await _uow.CommitAsync();
        await _uow.DisposeAsync();

        _transaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DisposeAsync_AlwaysDisposesTransactionAndConnection()
    {
        await _uow.DisposeAsync();

        _transaction.Verify(t => t.DisposeAsync(), Times.Once);
        _connection.Verify(c => c.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_AfterCommit_StillDisposesTransactionAndConnection()
    {
        await _uow.CommitAsync();
        await _uow.DisposeAsync();

        _transaction.Verify(t => t.DisposeAsync(), Times.Once);
        _connection.Verify(c => c.DisposeAsync(), Times.Once);
    }

    [Fact]
    public void Dispose_WhenNotCommitted_RollsBack()
    {
        _uow.Dispose();

        _transaction.Verify(t => t.Rollback(), Times.Once);
    }

    [Fact]
    public async Task Dispose_WhenCommitted_DoesNotRollBack()
    {
        await _uow.CommitAsync();
        _uow.Dispose();

        _transaction.Verify(t => t.Rollback(), Times.Never);
    }
}
