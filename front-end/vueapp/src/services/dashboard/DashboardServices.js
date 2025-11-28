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
    GetPlan(tenantName) {
        return api.get(`/Tenant/FindPlanByName/${tenantName}`)
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