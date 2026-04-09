import api from "@/services/api";

export default {
    getPromptList(paramsReq) {
        return api.get('/Prompt/Paged/', { params: paramsReq })
            .then(function (response) {
                return response
            }).catch((e) => {
                return {
                    error: e,
                }
            });
    },
    createPrompt(createDto) {
        return api.post('/Prompt', createDto)
            .then(({ data }) => {
                return data;
            })
            .catch((e) => {
                return {
                    error: e,
                }
            });
    },
    getPromptByUserId(paramsReq) {
        return api.get('/Prompt/PagedByUser/', { params: paramsReq })
            .then(function (response) {
                return response
            }).catch((e) => {
                return {
                    error: e,
                }
            });
    },
    updatePrompt(updateDto) {
        return api.put('/Prompt', updateDto)
            .then(({ data }) => {
                return data;
            })
            .catch((e) => {
                return {
                    error: e,
                }
            });
    },
    getPromptById(id) {
        return api.get(`/Prompt/${id}`)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    deletePrompts(ids) {
        return api.delete('/Prompt/DeleteByIds', { data: ids })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    validateOwner(id) {
        return api.get(`/Prompt/${id}/validate-ownership`)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    getPrompts(email) {
        return api.get(`/Prompt`, email)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    findPromptTemplates(query, orderBy) {
        return api.get('/Prompt/Templates', {
            params: { query, orderBy }
        })
            .then(({ data }) => data)
            .catch((error) => ({ error }));
    },
    importPrompts(templateIds) {
        return api.post('/Prompt/Import', templateIds)
            .then(({ data }) => data)
            .catch((error) => ({ error }));
    },
    refinePrompt(promptText) {
        return api.post('/Prompt/RefinePrompt', promptText)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    testPrompt({ promptText, contextText }) {
        return api.post("/v1/prompts/test", {
            promptText: promptText ?? "",
            contextText: contextText ?? "",
        }).then(({ data }) => data);
    },
}

