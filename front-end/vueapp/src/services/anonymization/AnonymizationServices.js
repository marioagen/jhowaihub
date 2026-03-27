import api from "@/services/api";

export default {
    processAnonymization(params) {
        return api
            .post(`/Anonymization`, params)
            .then((result) => {
                return result;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
};
