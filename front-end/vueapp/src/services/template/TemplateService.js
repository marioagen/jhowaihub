import api from "@/services/api";

export default {
    getTemplates(params) {
        return api
            .get("/ApiTemplate/Paged", { params: params })
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

    getTemplateById(id) {
        return api
            .get(`/ApiTemplate/${id}`)
            .then(({ data }) => {
                return data;
            })
            .catch((e) => {
                throw e;
            });
    },

    createTemplate(template) {
        return api
            .post("/ApiTemplate", template)
            .then(({ data }) => {
                return data;
            })
            .catch((e) => {
                throw e;
            });
    },

    updateTemplate(template) {
        return api
            .put(`/ApiTemplate`, template)
            .then(({ data }) => {
                return data;
            })
            .catch((e) => {
                throw e;
            });
    },

    deleteTemplate(id) {
        return api
            .delete(`/ApiTemplate/${id}`)
            .then(({ data }) => {
                return data;
            })
            .catch((e) => {
                throw e;
            });
    },
};
