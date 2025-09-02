import api from "@/services/api";

export default {
    getDocuments(filters) {
        return api.get("/Document", { params: filters })
            .then(({ data }) => {
                return {
                    content: data.content,
                    pagination: {
                        currentPage: data.currentPage,
                        pageCount: data.pageCount,
                        rowCount: data.rowCount,
                        listPage: data.rowCount,
                    }
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
};
