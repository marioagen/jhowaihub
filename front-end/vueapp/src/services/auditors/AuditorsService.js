import api from "@/services/api";

export default {
    getCardsAuditSummary(params = {}) {
        return api
            .get("/Auditor/Cards", { params })
            .then(({ data }) => data)
            .catch((error) => ({ error: error }));
    },
    getCardAuditDetails(cardId, workflowId, params = {}) {
        return api
            .get(`/Auditor/Cards/${cardId}/Workflows/${workflowId}`, { params })
            .then(({ data }) => data)
            .catch((error) => ({ error: error }));
    },
    getWorkflowAuditSummary() {
        return api
            .get("/Auditor/Workflows")
            .then(({ data }) => data)
            .catch((error) => ({ error: error }));
    },
    getWorkflowAuditDetails(id) {
        return api
            .get(`/Auditor/Workflow/${id}`)
            .then(({ data }) => data)
            .catch((error) => ({ error: error }));
    },
    getUserAuditSummary(params = {}) {
        return api
            .get("/Auditor/Users", { params })
            .then(({ data }) => data)
            .catch((error) => ({ error: error }));
    },
    getUserAuditDetails(userId, params = {}) {
        return api
            .get(`/Auditor/User/${userId}`, { params })
            .then(({ data }) => data)
            .catch((error) => ({ error: error }));
    },
};
