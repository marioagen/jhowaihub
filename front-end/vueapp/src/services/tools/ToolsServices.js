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
                        itemsPerPage: params.pageSize,
                    },
                };
            })
            .catch((error) => {
                return {
                    error: error
                }
            });
    },
    getToolsList() {
        return api.get("/Tool")
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    createTool(params) {
        return api
            .post("/Tool", params)
            .then(() => {
                return true;
            })
            .catch((e) => {
                console.log(e);
                const message = e?.response?.data?.errorCode
                return {
                    error: message,
                };
            });
    },
    editTool(params) {
        return api
            .put("/Tool", params)
            .then(() => {
                return true;
            })
            .catch((e) => {
                const message = e?.response?.data?.errorCode
                return {
                    error: message,
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
    validateConnector(params) {
        return api
            .post("/Tool/ValidateConnector", params)
            .then((response) => {
                return response.data;
            })
            .catch(() => {
                return false; 
            });
    }
};
