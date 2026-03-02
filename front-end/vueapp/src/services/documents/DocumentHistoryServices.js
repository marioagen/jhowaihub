import api from "@/services/api";

export default {
    async getDocumentHistory(id) {
        return await api
            .get(`/DocumentHistory/${id}`)
            .then((response) => {
                return response;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    async getDocumentQuestionsHistory(id, filters) {
        return await api
            .get(`/DocumentHistory/${id}/batch`, {
                params: filters,
            })
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
