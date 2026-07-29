import api from "@/services/api";

export default {
    updateStepAndStatus(params) {
        return api
            .put(`/Card`, params)
            .then((result) => {
                return result.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    updateStatusOnly(params) {
        return api
            .put(`/Card/UpdateStatus`, params)
            .then((result) => {
                return result.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    assignUser(params) {
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
    assignRange(params) {
        return api
            .put(`/Card/AssignRange`, params)
            .then((result) => {
                return result.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    finalizeRange(params) {
        return api
            .put(`/Card/FinalizeRange`, params)
            .then((result) => {
                return result.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    unassignUser(cardId) {
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
    },
    async findCardHeaderInfo(id) {
        return await api
            .get(`/Card/HeaderInfo/${id}`)
            .then((response) => {
                return response.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    getCardsByBatch(documentBatchId, workflowId) {
        return api
            .get(`/Card/Batch/${documentBatchId}`, { params: { workflowId } })
            .then(({ data }) => {
                return data;
            })
            .catch((e) => {
                return {
                    error: e,
                };
            });
    },
    reprocessCard(cardId) {
        return api
            .put(`/Card/${cardId}/Reprocess`)
            .then((result) => {
                return result.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },

    /// <summary>
    /// Returns the flat ordered list of AI tool output rows for the given card.
    /// Intended to be converted to CSV on the frontend.
    /// </summary>
    findToolOutputsForExport(cardId) {
        return api
            .get(`/Card/${cardId}/ToolOutputsExport`)
            .then(({ data }) => data)
            .catch((error) => ({ error }));
    },
};
