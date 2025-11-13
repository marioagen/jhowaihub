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
    assignUser(params){
        return api
            .put(`/Card/AssignUser`, params)
            .then((result) => {
                return result.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    unassignUser(cardId){
        return api
            .put(`/Card/UnassignUser/${cardId}`)
            .then((result) => {
                return result.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    async findByIdAnalyzeWithSteps(id) {
        return await api
            .get(`/Card/AnalyzeSteps/${id}`)
            .then((response) => {
                return response;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    }    
};
