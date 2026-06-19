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
    getAllUsers() {
        return api
            .get("/User")
            .then(({ data }) => {
                return data;
            })
            .catch(function (e) {
                logService.showMessage(e);
                return {
                    error: e,
                }
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
    getUsersByTeamId(teamId) {
        return api
            .get(`/User/Team/${teamId}`)
            .then(({ data }) => {
                return data;
            })
            .catch(function (e) {
                logService.showMessage(e);
            });
    },
    getUsersByTeamIds(teamIds) {
        return api
            .post('/User/Team/Query', { teamIds })
            .then(({ data }) => {
                return data;
            })
            .catch(function (e) {
                logService.showMessage(e);
                throw e;
            });
    },
    getUserByEmail(email) {
        return api.get(`/User/${email}`)
            .then(({ data }) => {
                return {
                    ...data,
                    password: "",
                    confirmedPassword: "",
                };
            })
            .catch((e) => {
                logService.showMessage(e);
                return {
                    error: e,
                }
            });
    },
};
