import api from "@/services/api";

export default {
    AnalyzeDocument(params) {
        return api.post("/Document/Analyze/", params)
            .then(() => {
                return true;
            })
            .catch((e) => {
                return {
                    error: e,
                }
            });
    },
    VerifyNormalize(id) {
        return api.get(`/Document/Status/${id}`)
            .then((response) => {
                return response.data;
            })
            .catch((e) => {
                return {
                    error: e,
                }
            });
    },
}