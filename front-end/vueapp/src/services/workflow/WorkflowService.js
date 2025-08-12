import api from "@/services/api";

export default {
    getWorkflowList() {
    },
    getWorkflowById() {
        return api.get("/Workflow")
            .then(({ data }) => {
                console.log(data)
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
            .then((data) => {
                console.log(data)
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });

    },
    deleteWorkflowById(workflowId) {
        return api.delete("/Workflow", workflowId)
            .then(( data ) => {
                console.log(data);
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
}