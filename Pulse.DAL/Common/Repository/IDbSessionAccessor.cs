namespace Pulse.DAL.Common.Repository;

public interface IDbSessionAccessor
{
    IDbSession? Session { get; set; }
}
