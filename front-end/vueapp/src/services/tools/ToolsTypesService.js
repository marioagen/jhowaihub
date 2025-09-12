import api from "@/services/api";

export default {
    getToolTypes() {
        return api
            .get("/ToolType")
            .then((response) => {
                return response.data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
}