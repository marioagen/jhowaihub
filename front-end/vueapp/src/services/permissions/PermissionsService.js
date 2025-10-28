import api from "@/services/api";
import logService from '@/services/log/logService.js';

export default {
    getPermissions() {
        return api
            .get("/Permission/FindAll/")
            .then(({ data }) => {
                return {
                    permissions: data,
                };
            })
            .catch(function (e) {
                logService.showMessage(e);
            });
    },
    getWorkflowPermissions() {
        return api
            .get("/Permission/Workflow/")
            .then(({ data }) => {
                return {
                    permissions: data,
                };
            })
            .catch(function (e) {
                logService.showMessage(e);
            });
    },
};
