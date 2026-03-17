import api from "@/services/api";

export default {
    getDocumentsAuditSummary(params = {}) {
        return api
            .get("/Auditor/Documents", { params })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getDocumentAuditDetails(documentId, workflowId, params = {}) {
        return api
            .get(`/Auditor/Documents/${documentId}/Workflows/${workflowId}`, { params })
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
