import api from "@/services/api";

export default {
    getStatus() {
        return api.get("/Status")
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
}