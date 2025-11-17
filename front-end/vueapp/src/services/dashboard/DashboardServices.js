import api from "@/services/api";
// import store from "@/store";

export default {
    getMainDashboardData() {
        return api.get(`/Dashboard`)
            .then(({ data }) => {
                console.log(data);
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    getTokensData(filters) {
        return api.get(`/Dashboard/Tokens`, { params: filters })
            .then(({ data }) => {
                console.log(data);
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    getPagesData(filters) {
        return api.get(`/Dashboard/Ocr`, { params: filters })
            .then(({ data }) => {
                console.log(data);
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    getWorkflowsData(filters) {
        return api.get(`/Dashboard/Workflows`, { params: filters })
            .then(({ data }) => {
                console.log(data);
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    getWorkflowsAutomaticData(filters) {
        return api.get(`/Dashboard/Workflows/Automatic`, { params: filters })
            .then(({ data }) => {
                console.log(data);
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
}