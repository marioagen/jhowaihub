import api from "@/services/api";

export default {
    getWorkflows(params) {
        return api.get("/Workflow/List", { params: params })
            .then(({ data }) => {
                return {
                    content: data.content,
                    pagination: {
                        currentPage: data.currentPage,
                        totalPages: data.pageCount,
                        itemsPerPage: 10,
                        totalItems: data.rowCount,
                    }
                }
            })
            .catch((e) => {
                return {
                    error: e,
                }
            });
    },
    getWorkflowList(email) {
        return api.get(`/Workflow/Users/${email}`)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    getWorkflowCompleteList() {
        return api.get(`/Workflow`)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    getWorkflowByTeamId(teamId, filters) {
        return api.get(`/Workflow/Teams/${teamId}`, { params: filters })
            .then(({ data }) => {
                return data;
            })
            .catch((e) => {
                return {
                    error: e,
                }
            });
    },
    getWorkflowById(workflowId, filters) {
        return api.get(`/Workflow/${workflowId}`, { params: filters })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    createWorkflow(params) {
        return api.post("/Workflow", params)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    editWorkflow(params) {
        return api.put("/Workflow", params)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });

    },
    deleteWorkflowById(workflowId) {
        return api.delete(`/Workflow/${workflowId}`)
            .then(({ data } ) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    updateStepToolOutput(params) {
        return api.put("/Workflow/UpdateOutput", params)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
}
