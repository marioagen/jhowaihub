import api from "@/services/api";

export default {
    async rejectAnalysis(params) {
        return await api
            .post("/DocumentAnalysisRejection", params)
            .then((response) => {
                return response;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    async findRejections(cardId) {
        return await api
            .get(`/DocumentAnalysisRejection?cardId=${cardId}`)
            .then((response) => {
                return response.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    async findWorkflowPreviousSteps(workflowId, cardId) {
        return await api
            .get(`/DocumentAnalysisRejection/WorkflowPreviousSteps/${workflowId}?cardId=${cardId}`)
            .then((response) => {
                return response.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
};
