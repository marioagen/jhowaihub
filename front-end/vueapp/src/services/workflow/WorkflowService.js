import api from "@/services/api";

export default {
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
    getWorkflowByTeamId(teamId) {
        return api.get(`/Workflow/Teams/${teamId}`)
            .then(({ data }) => {
                return data;
            })
            .catch((e) => {
                return {
                    error: e,
                }
            });
    },
    getWorkflowById(workflowId) {
        return api.get(`/Workflow/Team/${workflowId}`)
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
                    error: error.message,
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
        return api.delete(`/Workflow/${workflowId}`)
            .then(({ data } ) => {
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
