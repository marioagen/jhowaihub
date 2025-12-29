import api from "@/services/api";

export default {
    getTemplates(params) {
        return api
            .get("/ApiTemplate", { params: params })
            .then(({ data }) => {
                return data;
            })
            .catch((e) => {
                return {
                    error: e,
                };
            });
    },
};
