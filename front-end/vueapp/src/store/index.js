import Vuex from "vuex";
import createPersistedState from "vuex-persistedstate";
import Cookies from "js-cookie";

export default new Vuex.Store({
    state: {
        theme: null,
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
        lastSelectedWorkflow: null,
        userPreferences: {
            analyzeLeftColumnPercent: 50,
        },
        tempWorkflow: {
            status: false,
            list: [],
            data: {
                name: "",
                teamId: "",
            },
        },
        uploadNotifications: [],
    },
    mutations: {
        setTheme(state, themeName) {
            state.theme = themeName;
        },
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
        setLastSelectedWorkflow(state, workflow) {
            state.lastSelectedWorkflow = workflow;
        },
        setUserPreference(state, { key, value }) {
            if (!state.userPreferences) {
                state.userPreferences = {};
            }
            state.userPreferences[key] = value;
        },
        setTempWorkflow(state, payload) {
            state.tempWorkflow.status = true;
            state.tempWorkflow.list = payload.list;
            state.tempWorkflow.data = payload.data;
        },
        setFlowByStep(state, payload) {
            const { stepId, flowData, stepOrder } = payload;
            state.tempWorkflow.list = state.tempWorkflow.list.map((item) => {
                if (stepOrder !== undefined && (stepId === undefined || stepId == 0)) {
                    return item.order == stepOrder ? { ...item, stepTools: flowData } : item;
                }
                if (stepId !== undefined && stepId != 0) {
                    return item.id == stepId ? { ...item, stepTools: flowData } : item;
                }
                return item;
            });
        },
        cleanTempWorkflow(state) {
            state.tempWorkflow = {
                status: false,
                list: [],
                data: {},
            };
        },
        clearInProgressUploadNotifications(state, payload) {
            const namesFiles = payload.namesFiles || [];
            if (namesFiles.length === 0) return;
            state.uploadNotifications = state.uploadNotifications.filter(
                (n) => !(n.status === "in_progress" && namesFiles.includes(n.fileName))
            );
        },
        addUploadNotification(state, payload) {
            const { id, fileName, status = "in_progress", success = true } = payload;
            const exists = state.uploadNotifications.some((n) => n.id === id);
            if (!exists) {
                state.uploadNotifications.unshift({ id, fileName, status, success });
            }
        },
        setUploadNotificationComplete(state, payload) {
            const notification = state.uploadNotifications.find((n) => n.id === payload.id);
            if (notification) {
                notification.status = "completed";
                notification.success = payload.success !== false;
            }
        },
        removeUploadNotification(state, payload) {
            state.uploadNotifications = state.uploadNotifications.filter(
                (n) => n.id !== payload.id
            );
        },
        clearUploadNotifications(state) {
            state.uploadNotifications = [];
        },
        addAnonimyzationNotification(state, payload) {
            const {
                id,
                title = "Documento anonimizado",
                fileName,
                status = "completed",
                success = true,
                link,
            } = payload;
            const exists = state.uploadNotifications.some((n) => n.id === id);
            if (!exists) {
                state.uploadNotifications.unshift({ id, fileName, status, success, link, title });
            }
        },
    },
    plugins: [
        createPersistedState({
            storage: {
                getItem: (key) => Cookies.get(key),
                setItem: (key, value) => Cookies.set(key, value, { expires: 3, secure: true }),
                removeItem: (key) => Cookies.remove(key),
            },
            reducer(state) {
                const { theme, ...rest } = state;
                return rest;
            },
        }),
    ],
});
