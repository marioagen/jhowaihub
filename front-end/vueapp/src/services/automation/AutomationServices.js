import api from "@/services/api";

export default {
    getWorkflows(toolId) {
        return api
            .get(`/Automation/Workflows/N8n/${toolId}`)
            .then((response) => {
                return response.data;
            })
            .catch(() => {
                return false; 
            });
    },
    getWorkflowWebhookInputs(params) {
        return api
            .get(`/Automation/Workflow/N8n/WebhookInputs`,{ params: params })
            .then((response) => {
                return response.data;
            })
            .catch(() => {
                return false; 
            });
    },
}