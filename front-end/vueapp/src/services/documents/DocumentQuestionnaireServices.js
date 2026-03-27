import api from "@/services/api";

export default {
    async inputDocument(params) {
        return await api
            .post("/DocumentQuestionnarire/Input/", params)
            .then((response) => {
                return response;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    async applyQuestionnaire(params) {
        return await api
            .post("/DocumentQuestionnarire/InputQuestionnaire", params)
            .then((response) => {
                return response;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
};
