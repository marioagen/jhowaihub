import api from "@/services/api";

export default {
    getMainDashboardData() {
        return api.get(`/Dashboard`)
            .then(({ data }) => {
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
    GetUsageUnits(filters) {
        return api.get(`/Dashboard/UsageUnits`, { params: filters })
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
    ProcessMetricsByTenant() {
        return api.put(`/Dashboard/ProcessMetricsByTenant`)
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    },
    GetTotalUsageCost(filters) {
        return api.get(`/UsageMonth/FindTotalUsageCost`, { params: filters })
            .then(({ data }) => {
                return data;
            })
            .catch((error) => {
                return {
                    error: error,
                }
            });
    }
}