import api from "@/services/api";

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
    getProfilesList() {
        return api
            .get("/Profile")
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
    deleteProfileById(ids) {
        return api
            .delete("/Profile", { data: ids })
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
                const data = e?.response?.data ?? {};
                const message = data.detail || data.message || "Erro desconhecido";
                return {
                    success: false,
                    status,
                    error: message,
                    errorCode: data.errorCode,
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
                const data = e?.response?.data ?? {};
                const message = data.detail || data.message || "Erro desconhecido";
                return {
                    success: false,
                    status,
                    error: message,
                    errorCode: data.errorCode,
                };
            });
    },
    getProfileById(profileId) {
        return api
            .get(`/Profile/${profileId}`)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                };
            });
    },
};
