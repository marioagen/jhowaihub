import Vuex from "vuex";
import createPersistedState from "vuex-persistedstate";
import Cookies from "js-cookie";

export default new Vuex.Store({
    state: {
        tenantInitialized: false,
        userProfile: {
            language: "pt",
            image: "",
            name: "",
            login: "",
            tokenAzure: "",
            tokenApi: "",
            tenant: "",
            keyMongoAccess: "",
            isAdmin: false,
        },
        permissions: [],
    },
    mutations: {
        updateUserProfile(state, payload) {
            state.userProfile = payload.amount;
        },
        updatePermissions(state, payload) {
            state.permissions = payload;
        },
        updateUserProfileLanguage(state, payload) {
            state.userProfile.language = payload.amount;
        },
        updateUserProfileImage(state, payload) {
            state.userProfile.image = payload.amount;
        },
        updateUserProfileTenant(state, payload) {
            state.userProfile.tenant = payload.amount;
        },
        updateUserProfileKeyMongo(state, payload) {
            state.userProfile.keyMongoAccess = payload.amount;
        },
        setTenantInitialized(state, value) {
            state.tenantInitialized = value;
        },
    },
    plugins: [
        createPersistedState({
            storage: {
                getItem: (key) => Cookies.get(key),
                setItem: (key, value) => Cookies.set(key, value, { expires: 3, secure: true }),
                removeItem: (key) => Cookies.remove(key),
            },
        }),
    ],
});
