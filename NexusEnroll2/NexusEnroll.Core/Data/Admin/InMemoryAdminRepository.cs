using NexusEnroll.Core.Entities;

namespace NexusEnroll.Core.Data.Admin;

public class InMemoryAdminRepository : IAdminRepository
{
    private readonly List<Administrator> _admins = new();

    public Administrator GetAdmin(string adminId) =>
        _admins.FirstOrDefault(a => a.AdminId == adminId)!;

    public void AddAdmin(Administrator admin) =>
        _admins.Add(admin);
}