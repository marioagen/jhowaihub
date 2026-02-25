import api from "@/services/api";

export default {
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
    AnalyzeDocument(params) {
        return api
            .post("/DocumentMetadata/Analyze/", params)
            .then(() => {
                return true;
            })
            .catch((e) => {
                return {
                    error: e,
                };
            });
    },
};
