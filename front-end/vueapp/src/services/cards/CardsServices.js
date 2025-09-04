import api from "@/services/api";

export default {
    updateStepAndStatus(params) {
        return api
            .put(`/Card`,params)
            .then((result) => {
                return result.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
};
