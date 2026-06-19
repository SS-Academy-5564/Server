
namespace Pulse.DAL.Common.Constants;

public static class SeededIds
{
    public static class Roles
    {
        public static readonly Guid User = Guid.Parse("A1000000-0000-0000-0000-000000000001");
        public static readonly Guid Viewer = Guid.Parse("A1000000-0000-0000-0000-000000000002");
    }

    public static class Organizations
    {
        public static readonly Guid Default = Guid.Parse("B1000000-0000-0000-0000-000000000001");
    }
}
