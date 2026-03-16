import api from "@/services/api";

export default {
    getCardsAuditSummary(params = {}) {
        return api
            .get("/Auditor/Cards", { params })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getCardAuditDetails(cardId, workflowId, params = {}) {
        return api
            .get(`/Auditor/Cards/${cardId}/Workflows/${workflowId}`, { params })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getWorkflowAuditSummary(params = {}) {
        return api
            .get("/Auditor/Workflows", { params })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getWorkflowAuditDetails(id, params = {}) {
        return api
            .get(`/Auditor/Workflow/${id}`, { params })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getUserAuditSummary(params = {}) {
        return api
            .get("/Auditor/Users", { params })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getUserAuditDetails(userId, params = {}) {
        return api
            .get(`/Auditor/User/${userId}`, { params })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
};
