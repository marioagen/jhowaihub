import { jwtDecode } from "jwt-decode";
import store from "@/store";

export const hasPermission = (module, action) => {
    if (store.state.userProfile.isAdmin) return true;

    const permissions = store.state.permissions;
    if (!permissions || permissions.length === 0) return false;

    return permissions.some((p) => {
        const [key] = Object.keys(p);
        const value = p[key];
        return key === module && value === action;
    });
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
        const raw = payload.permissions ?? payload.permission;
        const permissions =
            !raw || raw === "" ? [] : typeof raw === "string" ? JSON.parse(raw) : raw;

        console.log(permissions);

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
