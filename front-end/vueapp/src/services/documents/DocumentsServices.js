import api from "@/services/api";

export default {
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
