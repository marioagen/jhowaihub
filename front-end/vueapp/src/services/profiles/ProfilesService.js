import api from "@/services/api";
import PaginationDivider from "@/utils/paginationDivider";
const divider = new PaginationDivider();

export default {
    getProfiles(params) {
        return api
            .get("/Profile/Paged/", { params: params })
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
    deleteProfileById(ids) {
        return api
            .delete("/Profile/DeleteByIds", { data: ids })
            .then(() => {
                return true;
            })
            .catch(function (e) {
                console.log(e);
                return false;
            });
    },
    addProfile(profile) {
        return api
            .post("/Profile", profile)
            .then(({ data, status }) => {
                return {
                    success: true,
                    status,
                    data,
                };
            })
            .catch((e) => {
                const status = e?.response?.status ?? 500;
                const message = e?.response?.data?.message || "Erro desconhecido";
                return {
                    success: false,
                    status,
                    error: message,
                };
            });
    },
    updateProfile(profile) {
        return api
            .put("/Profile", profile)
            .then(({ data, status }) => {
                return {
                    success: true,
                    status,
                    data,
                };
            })
            .catch((e) => {
                const status = e?.response?.status ?? 500;
                const message = e?.response?.data?.message || "Erro desconhecido";
                return {
                    success: false,
                    status,
                    error: message,
                };
            });
    },

};
