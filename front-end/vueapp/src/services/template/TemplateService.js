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
};
