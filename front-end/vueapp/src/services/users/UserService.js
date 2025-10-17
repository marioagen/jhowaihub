import api from "@/services/api";
import logService from '@/services/log/logService.js';

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
                logService.showMessage(e);
            });
    },
    deleteUsersById(userId) {
        return api
            .delete("/User/DeactivateByIds", { data: [userId] })
            .then(() => {
                return true;
            })
            .catch(function (e) {
                logService.showMessage(e);
                return false;
            });
    },
    getUsersByTeamId(teamId){
        return api
            .get(`/User/Team/${teamId}`)
            .then(({ data }) => {
                return data;
            })
            .catch(function (e) {
                logService.showMessage(e);
            });
    },
    getUserById(userId) {
        return api.post('/User/FindById', userId)
            .then(({ data }) => {
                return data;
            })
            .catch(function (e) {
                logService.showMessage(e);
            });
    }
};
