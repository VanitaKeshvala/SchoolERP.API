using SchoolERP.API.Models;

namespace SchoolERP.API.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<UserPermissionViewModel>> GetUserPermissionsAsync(int userId);
        Task<List<UserPermissionViewModel>> GetUserPermissions(int userId, string? menuUrlPrefix = null, string? permissionName = null);
        List<MstUserTypeViewModel> GetAllUserTypes();
        MstUserTypeViewModel? GetUserTypeByID(int userTypeID);
        (bool success, string message) UpsertUserType(MstUserTypeUpsertRequest request, int userId, string ipAddress);
        (bool success, string message) ToggleUserTypeStatus(int userTypeID, bool isActive, int userId, string ipAddress);
        List<MstRoleViewModel> GetAllRoles();
        MstRoleViewModel? GetRoleByID(int roleID);
        Task<(bool success, string message, int roleId)> UpsertRole(MstRoleUpsertRequest request, int userId, string ipAddress);
        (bool success, string message) ToggleRoleStatus(int roleID, bool isActive, int userId, string ipAddress);
        List<RoleMenuPermissionViewModel> GetPermissionsMatrix(int roleId);
        (bool success, string message) SaveRolePermissions(MstRolePermissionSaveRequest request, int adminId, string ipAddress);
        (bool success, string message) DeleteRole(List<int> roleID, int userId, string ipAddress);
        List<MstPermissionViewModel> GetAllPermissions();
        MstPermissionViewModel? GetPermissionByID(int permissionID);
        (bool success, string message) UpsertPermission(MstPermissionUpsertRequest request, int userId, string ipAddress);
        (bool success, string message) TogglePermissionStatus(int permissionID, bool isActive, int userId, string ipAddress);
        (bool success, string message) DeletePermission(List<int> permissionID, int userId, string ipAddress);
    }
}
