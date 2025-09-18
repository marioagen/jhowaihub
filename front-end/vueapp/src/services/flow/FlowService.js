import api from "@/services/api";

export default {
    getStepToolsByStepId(stepId) {
        // TODO: Replace with real API call
        const stepTools = [
            { Id: 1, ToolId: 201, Label: "OCR", PositionX: 100, PositionY: 100 },
            { Id: 2, ToolId: 202, Label: "Validação", PositionX: 200, PositionY: 100 },
            { Id: 3, ToolId: 203, Label: "Exportação", PositionX: 300, PositionY: 200 },
            { Id: 3, ToolId: 203, Label: "Exportação", PositionX: 400, PositionY: 300 }
        ];
        return Promise.resolve(stepTools);
    },

    getStepToolDependenciesByStepId(stepId) {
        // TODO: Replace with real API call
        const stepToolDependencies = [
            { StepToolIdFrom: 1, StepToolIdTo: 2 },
            { StepToolIdFrom: 2, StepToolIdTo: 3 }
        ];
        return Promise.resolve(stepToolDependencies);
    }
}