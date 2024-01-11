namespace SimpleMVCWebWithAuth.Data
{
    public interface IUserRolesService
    {
        Task EnsureAdminUserRole();
    }
}