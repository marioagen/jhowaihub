import api from "@/services/api";

export default {
    getWorkflowList() {
        return api
            .get("/Workflow")
            .then(({ data }) => {
                return data;
            })
            .catch(function (e) {
                console.log(e);
            });
    },
    getWorkbyTeamId(id) {
        return api.get(`/Workflow/${id}`)
            .then(({ data }) => {
                return data;
            })
            .catch((e) => {
                return {
                    error: e,
                }
            });
    },
};
