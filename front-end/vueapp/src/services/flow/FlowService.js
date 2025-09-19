import api from "@/services/api";

export default {
    getStepToolsByStepId(stepId) {
        // Replace with real API call
        const stepTools = [
            { id: 1, toolId: 201, label: "OCR", positionX: 100, positionY: 100 },
            { id: 2, toolId: 202, label: "Validação", positionX: 200, positionY: 100 },
            { id: 3, toolId: 203, label: "Exportação", positionX: 300, positionY: 200 },
            { id: 4, toolId: 203, label: "Importação", positionX: 400, positionY: 300, input: {type: 'string', value:"sadlakdladk"} }
        ];
        return Promise.resolve(stepTools);
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