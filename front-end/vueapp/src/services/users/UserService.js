import api from "@/services/api";

export default {
    getUsers(params) {
        return api
            .get("/User/Paged/", { params: params })
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
    deleteUsersById(userId) {
        return api
            .delete("/User/DeactivateByIds", { data: [userId] })
            .then(() => {
                return true;
            })
            .catch(function (e) {
                console.log(e);
                return false;
            });
    },
    getUsersByTeamId(teamId){
        return api
            .get(`/User/Team/${teamId}`)
            .then(({data}) => {
                return data;
            })
            .catch(function (e) {
                console.log(e);
            });
    }
};
