import api from "@/services/api";
import { resolveErrorMessageKey } from "@/utils/errorMessage";

export default {
    getWorkflows(toolId) {
        return api
            .get(`/Automation/Workflows/N8n/${toolId}`)
            .then((response) => {
                return response.data;
            })
            .catch((e) => {
                return {
                    error: resolveErrorMessageKey(e),
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
                return {
                    error: resolveErrorMessageKey(e),
                };
            });
    },
}