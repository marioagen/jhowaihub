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
        lastSelectedWorkflow: null,
        tempWorkflow: {
            status: false,
            list: [],
            data: {
                name: "",
                teamId: "",
            },
        },
        uploadNotifications: [
            { id: "dummy-completed", fileName: "Annual-Report-2024.pdf", status: "completed" },
            { id: "dummy-in-progress", fileName: "Contract-draft.docx", status: "in_progress" },
        ],
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
        setLastSelectedWorkflow(state, workflow) {
            state.lastSelectedWorkflow = workflow;
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
        addUploadNotification(state, payload) {
            const { id, fileName, status = "in_progress" } = payload;
            const exists = state.uploadNotifications.some((n) => n.id === id);
            if (!exists) {
                state.uploadNotifications.unshift({ id, fileName, status });
            }
        },
        setUploadNotificationComplete(state, payload) {
            const notification = state.uploadNotifications.find((n) => n.id === payload.id);
            if (notification) {
                notification.status = "completed";
            }
        },
        removeUploadNotification(state, payload) {
            state.uploadNotifications = state.uploadNotifications.filter(
                (n) => n.id !== payload.id
            );
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
