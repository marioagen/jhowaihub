import api from "@/services/api";
import store from "@/store";

export default {
    getDocuments(filters) {
        /*if (!store.state.userProfile.keyMongoAccess) {
            return Promise.resolve({
                content: [],
                pagination: {
                    currentPage: 1,
                    totalPages: 0,
                    itemsPerPage: 10,
                    totalItems: 0,
                }
            });
        }*/

        return api
            .get("/Document", { params: filters })
            .then(({ data }) => {
                return {
                    content: data.content,
                    pagination: {
                        currentPage: data.currentPage,
                        totalPages: data.pageCount,
                        itemsPerPage: 10,
                        totalItems: data.rowCount,
                    },
                };
            })
            .catch((e) => {
                return {
                    error: e,
                };
            });
    },
    deleteDocument(ids) {
        return api
            .delete("/Document/Delete", { data: ids })
            .then(() => {
                return true;
            })
            .catch(function (e) {
                return {
                    error: e,
                };
            });
    },
    getDocumentAnalyze(docId) {
        return api
            .get(`/DocumentMetadata/Analyze/${docId}`)
            .then((result) => {
                return result.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    findDocument(docId) {
        return api
            .get(`/Document/FindDocument/${docId}`, {
                responseType: "blob",
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
    normalizeDocument(params) {
        return api
            .post("/DocumentMetadata/Analyze/", params)
            .then((response) => {
                return response;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getNormalizedDocument(docId) {
        return api
            .get(`/DocumentMetadata/Normalized/${docId}`)
            .then((result) => {
                return result.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getOcrText(docId) {
        return api
            .get(`/DocumentMetadata/OcrText/${docId}`)
            .then((result) => {
                return result.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
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
    async findByIdAnalyze(id) {
        return await api
            .get(`/DocumentMetadata/Analyze/${id}`)
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
    checkPagesLength() {
        return api
            .get("/Document/CheckExceededPages")
            .then(({ data }) => {
                return data;
            })
            .catch((e) => {
                return false;
            });
    },
};
