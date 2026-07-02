import { isMockMode } from "@/mock/mockConfig.js";
import { resolveMockResponse } from "@/mock/mockApiRouter.js";

export function installMockApi(api) {
    if (!isMockMode()) {
        return;
    }

    api.interceptors.request.use((config) => {
        config.adapter = () => Promise.resolve(resolveMockResponse(config));
        return config;
    });

    console.info("[MockMode] API simulada ativa — nenhuma chamada real ao backend.");
}
