import store from "@/store";

export const hasPermission = (permissionKey) => {
    const isAdmin = store.state.userData.isAdmin;
    if(isAdmin) return true;

    const [module, permission] = permissionKey.split(":");

    const userPermissions = store.state.permissions;
    const modulePermissions = userPermissions.find((p) => p.module === module);

    if (!modulePermissions) return false;

    return modulePermissions.actions.includes(permission);
};