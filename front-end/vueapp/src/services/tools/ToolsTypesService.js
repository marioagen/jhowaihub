import api from "@/services/api";

export default {
    getToolTypes() {
        return api
            .get("/ToolType")
            .then(() => {
                return true;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
}