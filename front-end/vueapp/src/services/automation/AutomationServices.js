import api from "@/services/api";

export default {
    getWorkflows(toolId) {
        return api
            .get(`/Automation/Workflows/N8n/${toolId}`)
            .then((response) => {
                return response.data;
            })
            .catch((e) => {
                const message = e?.response?.data?.message || "Erro desconhecido";
                return {
                    error: message,
                };
            });
    },
    getWorkflowWebhookInputs(params) {
        return api
            .get(`/Automation/Workflow/N8n/WebhookInputs`,{ params: params })
            .then((response) => {
                return response.data;
            })
            .catch((e) => {
                const message = e?.response?.data?.message || "Erro desconhecido";
                return {
                    error: message,
                };
            });
    },
}