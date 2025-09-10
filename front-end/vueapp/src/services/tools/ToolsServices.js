import api from "@/services/api";

export default {
    getTools() {
        return api
            .get("/Tool/Paged/", { params: params })
            .then(({ data }) => {
                return true;
                // return {
                //     content: data.content,
                //     pagination: {
                //         currentPage: data.currentPage,
                //         totalPages: data.pageCount,
                //         rowCount: data.rowCount,
                //         totalItems: data.rowCount,
                //     },
                // };
            })
            .catch((error) => {
                return {
                    error: error
                }
            });
    },
    createTool() {
        return api
            .post("/Tool")
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
