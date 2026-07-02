import store from "@/store";
import { isMockMode, MOCK_TENANT, MOCK_USER_EMAIL, MOCK_USER_NAME, MOCK_KEY_MONGO_ACCESS } from "@/mock/mockConfig.js";
import { buildLoginResponse, buildPermissions } from "@/mock/mockFixtures.js";

export function bootstrapMockSession() {
    if (!isMockMode()) {
        return;
    }

    const login = buildLoginResponse();
    const dataUser = {
        language: store.state.userProfile.language || "pt",
        image: "",
        name: MOCK_USER_NAME,
        login: MOCK_USER_EMAIL,
        tokenAzure: "",
        tokenApi: login.token,
        tenant: MOCK_TENANT,
        keyMongoAccess: MOCK_KEY_MONGO_ACCESS,
        isAdmin: true,
    };

    store.commit("updateUserProfile", { amount: dataUser });
    store.commit("updatePermissions", buildPermissions());
    store.commit("setTenantInitialized", true);
    window.localStorage.setItem("project", JSON.stringify({ isLogged: true }));
}

export function applyMockLoginFromCredentials(email) {
    bootstrapMockSession();
    if (email) {
        store.commit("updateUserProfile", {
            amount: {
                ...store.state.userProfile,
                login: email,
                name: email.split("@")[0],
            },
        });
    }
    return buildLoginResponse(email || MOCK_USER_EMAIL);
}
