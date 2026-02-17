import api from "@/services/api";

export default {
    sendQuizz(params) {
        return api
            .post("/Document/inputQuestionnaire", params)
            .then(() => {
                return true;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    submitInput(params) {
        return api
            .post("/Document/input/", params)
            .then((response) => {
                return response;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getAnalyzeDocument(id) {
        return api
            .get("/Document/Analyze/" + id)
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
