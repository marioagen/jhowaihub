import api from "@/services/api";

export default {
    getTeams(params) {
        return api.get("/Team/Paged/", { params: params })
            .then(({ data }) => {
                return {
                    content: data.content,
                    pagination: {
                        currentPage: data.currentPage,
                        totalPages: data.pageCount,
                        rowCount: data.rowCount,
                        totalItems: data.rowCount,
                    },
                };
            })
            .catch(function (e) {
                console.log(e);
            });
    },
    getTeamList() {
        return api.get("/Team")
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    getTeamsByUser() {
        return api.get("/Team")
            .then(({ data }) => {
                return data;
            })
    },
    deleteTeamById(teamId) {
        return api
            .delete("/Team/DeleteByIds", { data: [teamId] })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
};
