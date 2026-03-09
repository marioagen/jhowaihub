import api from "@/services/api";

export default {
    getAuditorDocuments(params) {
        return api
            .get("/Auditor/Documents", { params: params })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getAuditorWorkflows(params) {
        return api
            .get("/Auditor/Workflows", { params: params })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getAuditorUsers(params) {
        return api
            .get("/Auditor/Users", { params: params })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getAuditorDocument(id) {
        return api
            .get(`/Auditor/Document/${id}`)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getAuditorWorkflow(id) {
        return api
            .get(`/Auditor/Workflow/${id}`)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getAuditorUser(id) {
        return api
            .get(`/Auditor/User/${id}`)
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
