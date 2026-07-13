namespace Pulse.DAL.Common.Repository;

public class DbSessionAccessor : IDbSessionAccessor
{
    public IDbSession? Session { get; set; }
}
