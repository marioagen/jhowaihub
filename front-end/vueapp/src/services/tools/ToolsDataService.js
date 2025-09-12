import api from "@/services/api";

export default {
    getToollData() {
        return api
            .get("/ToolData")
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