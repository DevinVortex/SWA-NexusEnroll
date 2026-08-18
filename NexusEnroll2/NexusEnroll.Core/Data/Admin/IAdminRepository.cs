using NexusEnroll.Core.Entities;

namespace NexusEnroll.Core.Data.Admin;

public interface IAdminRepository
{
    Administrator GetAdmin(string adminId);
    void AddAdmin(Administrator admin);
}