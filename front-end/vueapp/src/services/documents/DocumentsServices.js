import api from "@/services/api";
import store from "@/store";

export default {
    getDocuments(filters) {
        if (!store.state.userProfile.keyMongoAccess) {
            return Promise.resolve({
                content: [],
                pagination: {
                    currentPage: 1,
                    totalPages: 0,
                    itemsPerPage: 10,
                    totalItems: 0,
                }
            });
        }
        
        return api.get("/Document", { params: filters })
            .then(({ data }) => {
                return {
                    content: data.content,
                    pagination: {
                        currentPage: data.currentPage,
                        totalPages: data.pageCount,
                        itemsPerPage: 10,
                        totalItems: data.rowCount,
                    },
                }
            })
            .catch((e) => {
                return {
                    error: e,
                }
            });
    },
    deleteDocument(ids) {
        return api.delete("/Document/Delete", { data: ids })
                .then(() => {
                    return true;
                })
                .catch(function (e) {
                    return {
                        error: e,
                    }
                });
    },
    getDocumentAnalyze(docId) {
        return api
            .get(`/Document/Analyze/${docId}`)
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
            .post("/Document/Analyze/", params)
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
            .get(`/Document/Normalized/${docId}`)
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
            .get(`/Document/OcrText/${docId}`)
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
