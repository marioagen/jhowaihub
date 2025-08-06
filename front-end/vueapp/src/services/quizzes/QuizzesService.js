import api from "@/services/api";

export default {
    getQuizzes(params) {
        return api.get("/Questionnaire/Paged", { params: params })
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
    getQuizzById(id) {
        return api.get(`/Questionnaire/${id}`)
            .then(({ data }) => {
                return data;
            })
            .catch((e) => {
                return {
                    error: e,
                }
            });
    },
    createQuizz(params) {
        return api.post("/Questionnaire", params)
            .then(() => {
                return true;
            })
            .catch(function (e) {
                const message = e?.response?.data?.message || "Erro desconhecido";
                return {
                    error: message,
                };
            });
    },
    editQuizz(params) {
        return api.put("/Questionnaire", params)
            .then(() => {
                return true;
            })
            .catch((e) => {
                const message = e?.response?.data?.message || "Erro desconhecido";
                return {
                    error: message,
                };
            });
    },
    deleteQuizzById(ids) {
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