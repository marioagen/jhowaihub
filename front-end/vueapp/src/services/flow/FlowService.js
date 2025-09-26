import api from "@/services/api";

export default {
    getStepToolsByStepId(stepId) {
            return api
                .get("/StepId/", stepId)
                .then(({ data }) => {
                    return {
                        permissions: data,
                    };
                })
                .catch(function (e) {
                    console.log(e);
                });
    },

    getStepToolDependenciesByStepId(stepId) {
        // Replace with real API call
        const stepToolDependencies = [
            { StepToolIdFrom: 1, StepToolIdTo: 2 },
            { StepToolIdFrom: 2, StepToolIdTo: 3 },
            { StepToolIdFrom: 3, StepToolIdTo: 4 }
        ];
        return Promise.resolve(stepToolDependencies);
    }
}