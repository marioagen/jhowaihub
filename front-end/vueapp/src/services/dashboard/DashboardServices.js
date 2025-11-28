import api from "@/services/api";

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
    GetUsedModels() {
        return api.get(`/UsageMonth/FindUsedModels`)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    GetByUsageType(filters) {
        return api.get(`/UsageMonth`, { params: filters })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    GetTokensByModel(filters) {
        return api.get(`/UsageMonth/FindByModel`, { params: filters })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    GetUsageUnits() {
        return api.get(`/Dashboard/UsageUnits`)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
}