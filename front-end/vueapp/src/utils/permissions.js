import { jwtDecode } from "jwt-decode";
import store from "@/store";

export const hasPermission = (module, action) => {
    if (store.state.userProfile.isAdmin) return true;

    const permissions = store.state.permissions;
    if (permissions.length === 0) return false;
    var isAllowed = permissions.some((p) => {
        const [key] = Object.keys(p);
        const value = p[key];
        return key === module && value === action;
    });
    return isAllowed;
};

export const getJWTPermissions = (token) => {
    if (!token) {
        return {
            permissions: [],
            isAdmin: false,
        };
    }

    try {
        const payload = jwtDecode(token);
        const permissions = payload.permission === "" ? [] : JSON.parse(payload.permissions);
        const isAdmin = payload.isAdmin === "true";
        return {
            permissions: permissions,
            isAdmin: isAdmin,
            payload: payload,
        };
    } catch (err) {
        return {
            permissions: [],
            isAdmin: false,
        };
    }
};
