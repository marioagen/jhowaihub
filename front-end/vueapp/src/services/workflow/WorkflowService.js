import api from "@/services/api";

export default {
    getWorkflowList(email) {
        return api
            .get(`/Workflow/Users/${email}`)
            .then(({ data }) => {
                return data;
            })
            .catch(function (e) {
                console.log(e);
            });
    },
    getWorflowkbyTeamId(teamId) {
        return api.get(`/Workflow/Teams/${teamId}`)
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
