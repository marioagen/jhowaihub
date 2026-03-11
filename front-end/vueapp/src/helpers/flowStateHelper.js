export default {
    updateFlowStateParams(nodeDataId, newParameters, newDependencies) {
        const flowStateJson = localStorage.getItem("flow_state_params");

        if (!flowStateJson) {
            return false;
        }

        try {
            const flowState = JSON.parse(flowStateJson);
            const nodeIndex = flowState.nodes.findIndex((n) => n.id === nodeDataId);

            if (nodeIndex === -1) {
                return false;
            }

            let node = flowState.nodes[nodeIndex];
            if (newParameters !== undefined) {
                node.data.parameters = newParameters;
            }
            if (newDependencies !== undefined) {
                node.data.dependencies = newDependencies;
                flowState.selectedDependencies = newDependencies;
            }

            flowState.selectedNode = undefined;

            localStorage.setItem("flow_state_params", JSON.stringify(flowState));
            return true;
        } catch (e) {
            console.error("Error updating flow state:", e);
            return false;
        }
    },

    /**
     * High-level: upserts the first parameter of a node with a given value,
     * optionally sets the node subtitle, updates dependencies, and persists.
     *
     * @param {string} nodeId       - ID of the node to update
     * @param {string} paramValue   - The value to set in parameters[0].value
     * @param {string|null} subtitle - Optional subtitle to set on the node (e.g. prompt name, template name)
     * @param {Array} dependencies  - Dependencies array to persist
     * @returns {boolean}           - true on success
     */
    commitNodeConfig(nodeId, paramValue, subtitle, dependencies) {
        const flowStateJson = localStorage.getItem("flow_state_params");

        if (!flowStateJson) {
            return false;
        }

        try {
            const flowState = JSON.parse(flowStateJson);
            const nodeIndex = flowState.nodes.findIndex((n) => n.id === nodeId);

            if (nodeIndex === -1) {
                return false;
            }

            const node = flowState.nodes[nodeIndex];
            if (!node.data.parameters) {
                node.data.parameters = [];
            }
            if (node.data.parameters.length === 0) {
                node.data.parameters.push({
                    stepToolId: 0,
                    value: paramValue ?? null,
                    requiredFile: false,
                    webhookId: null,
                });
            } else {
                node.data.parameters[0].value = paramValue ?? null;
            }

            if (subtitle !== undefined && subtitle !== null) {
                node.data.subtitle = subtitle;
            }

            if (dependencies !== undefined) {
                node.data.dependencies = dependencies;
                flowState.selectedDependencies = dependencies;
            }

            flowState.selectedNode = undefined;

            localStorage.setItem("flow_state_params", JSON.stringify(flowState));
            return true;
        } catch (e) {
            console.error("Error committing node config:", e);
            return false;
        }
    },
};

