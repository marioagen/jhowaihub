import api from "@/services/api";

export default {
    getQuestions(params) {
        return api.get("/Question/Paged", { params: params })
            .then(({ data }) => {
                return {
                    content: data.content,
                    pagination: {
                        currentPage: data.currentPage,
                        totalPages: data.pageCount,
                        rowCount: data.rowCount,
                        totalItems: data.rowCount,
                    }
                }
            })
            .catch((e) => {
                return {
                    error: e,
                }
            });
    },
    createQuestion(description) {
        return api.post(`/Question?description=${description}`)
            .then(() => {
                return true;
            })
            .catch(function (e) {
                let errorMessage = "";
                if (e.response.status == 409) {
                    errorMessage = "labelQuestionAlreadyExists";
                } else {
                    errorMessage = "labelQuestionError";
                }

                return {
                    error: errorMessage,
                }
            });
    },
    editQuestion(params) {
        return api.put("/Question", params)
            .then(() => {
                return true;
            })
            .catch((e) => {
                let errorMessage = "";
                if (e.response.status == 409) {
                    errorMessage = "labelQuestionAlreadyExists";
                } else {
                    errorMessage = e;
                }
                return {
                    error: errorMessage,
                }
            });
    },
    deleteQuestionById(ids) {
        return api.delete("/Question/DeleteByIds", { data: ids })
            .then(() => {
                return true;
            })
            .catch(function (e) {
                return {
                    error: e,
                }
            });
    }
}