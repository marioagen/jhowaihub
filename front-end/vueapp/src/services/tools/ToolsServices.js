import api from "@/services/api";

export default {
    getTools(params) {
        return api
            .get("/Tool/Paged/", { params: params })
            .then(({ data }) => {
                return {
                    content: data.items,
                    pagination: {
                        currentPage: data.currentPage,
                        totalPages: data.totalPages,
                        rowCount: data.totalCount,
                        totalItems: data.totalCount,
                    },
                };
            })
            .catch((error) => {
                return {
                    error: error
                }
            });
    },
    createTool(params) {
        return api
            .post("/Tool", params)
            .then(() => {
                return true;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    editTool(params) {
        return api
            .put("/Tool", params)
            .then(() => {
                return true;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    deleteTool(toolId) {
        return api
            .delete("/Tool/", { data: toolId })
            .then(() => {
                return true;
            })
            .catch((erro) => {
                return {
                    error: erro
                }
            });
    },
};
