import api from "@/services/api";

export default {
    getToollData() {
        return api
            .get("/ToolData")
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