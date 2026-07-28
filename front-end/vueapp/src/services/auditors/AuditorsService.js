import api from "@/services/api";

export default {
    getActionTypes() {
        return api
            .get("/Auditor/ActionTypes")
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
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

    /// <summary>
    /// Returns a paged summary of audit events grouped by tool (Agents, Connectors, API Templates, Questionnaires).
    /// </summary>
    getToolsAuditSummary(params = {}) {
        return api
            .get("/Auditor/Tools", { params })
            .then(({ data }) => data)
            .catch((error) => ({ error }));
    },

    /// <summary>
    /// Returns the detailed event history for a specific tool.
    /// </summary>
    getToolsAuditDetail(toolId, params = {}) {
        return api
            .get(`/Auditor/Tool/${toolId}`, { params })
            .then(({ data }) => data)
            .catch((error) => ({ error }));
    },

    /// <summary>
    /// Returns system-wide audit events: logins, API calls, user management actions and workflow changes.
    /// </summary>
    getSystemAuditEvents(params = {}) {
        return api
            .get("/Auditor/System", { params })
            .then(({ data }) => data)
            .catch((error) => ({ error }));
    },
};
