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
            console.log(payload)
            console.log(stepId)
            console.log(flowData)
            console.log(stepOrder)
            state.tempWorkflow.list = state.tempWorkflow.list.map(item => {
                if (stepId !== undefined) {
                    return item.id == stepId ? { ...item, stepTools: flowData } : item;
                } 
                if (stepOrder !== undefined) {
                    return item.stepOrder == stepOrder ? { ...item, stepTools: flowData } : item;
                }
                return item;
            });
            console.log(state.tempWorkflow.list);
        },
        cleanTempWorkflow(state) {
            state.tempWorkflow = {
                status: false,
                list: [],
                data: {},
            };
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
