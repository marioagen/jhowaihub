import store from "@/store";

export const hasPermission = (to) => {
    const required = to.meta?.permission;
    if (!required || required.length === 0) {
        return true;
    }

    const userPermissions = store.state.permissions || [];
    return required.every(permission => userPermissions.includes(permission));
};

export const getJWTPermissions = () => {
    return [
        "questions_view",
        "types_view",
        "documents_view",
    ]
};