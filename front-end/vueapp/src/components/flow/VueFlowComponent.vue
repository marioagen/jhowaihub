<template>
    <div class="row mb-2">
        <div class="col">
            <button
                class="btn btn-primary btn-sm me-2"
                data-bs-toggle="collapse"
                data-bs-target="#toolsCollapse"
                aria-expanded="false"
                aria-controls="toolsCollapse"
                @click="showCollapse"
            >
                <LucideIcon
                    icon="Plus"
                    :size="15"
                />
                {{ isActiveCollapse ? $t("flow.hideTools") : $t("flow.showTools") }}
            </button>
        </div>
    </div>
    <div
        class="collapse"
        id="toolsCollapse"
    >
        <div class="mt-3 mb-3">
            <div class="card mb-3">
                <div class="card-body palette">
                    <div>
                        <button
                            v-for="tool in toolsList"
                            :key="tool.id"
                            class="btn btn-outline-primary btn-sm me-2 mt-2 palette-item"
                            draggable="true"
                            @dragstart="
                                onDragStart($event, {
                                    id: tool.id,
                                    name: tool.name,
                                    isEditableInput: tool.isEditableInput,
                                    toolType: tool.toolType,
                                })
                            "
                        >
                            {{ tool.name }}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="card vue-flow-container p-0">
        <VueFlow
            v-model:nodes="nodes"
            v-model:edges="edges"
            :style="{ width: '100%', height: '100%' }"
            @connect="onConnect"
            @pane-ready="onPaneReady"
            @drop="onDrop"
            @dragover="onDragOver"
        >
            <Background
                patternColor="#BCD5F2"
                gap="10"
                variant="dots"
                :size="1"
            />
            <template #node-hub="props">
                <HubNode
                    :node="props"
                    @deleteNode="deleteNode"
                    @openNodeConfig="openNodeConfig"
                />
            </template>
            <template #edge-special="props">
                <SpecialEdge
                    v-bind="props"
                    @deleteEdge="deleteEdge"
                    :data="props"
                />
            </template>
        </VueFlow>
    </div>
</template>
<script>
    import { VueFlow } from "@vue-flow/core";
    import { Background } from "@vue-flow/background";
    import HubNode from "@/components/flow/HubNode.vue";
    import SpecialEdge from "@/components/flow/SpecialEdge.vue";
    import LogService from "@/services/log/logService";
    import ToolsServices from "@/services/tools/ToolsServices";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import PromptService from "@/services/prompts/PromptsService";
    import ToolType from "@/constants/ToolType";

    export default {
        name: "VueFlowComponent",
        emits: ["openNodeConfig", "nodeDeleted", "flowChanged"],
        props: {
            stepId: {
                type: Number,
                required: false,
                default: null,
            },
            step: {
                type: Object,
                required: false,
                default: null,
            },
            stepOrder: {
                type: Number,
                required: false,
                default: null,
            },
            isEdit: {
                type: Boolean,
                required: false,
                default: false,
            },
            hasStepTools: {
                type: Boolean,
                required: false,
                default: false,
            },
        },
        data() {
            return {
                toolsList: [],
                nodes: [],
                edges: [],
                vueFlowInstance: null,
                isActiveCollapse: false,
            };
        },
        components: {
            VueFlow,
            Background,
            HubNode,
            SpecialEdge,
        },
        methods: {
            emitFlowChanged() {
                this.$emit("flowChanged");
            },
            getToolsList() {
                ToolsServices.getToolsList().then((response) => {
                    this.toolsList = response;
                });
            },
            onPaneReady(instance) {
                this.vueFlowInstance = instance;
            },
            createStartNode() {
                return {
                    id: "start",
                    position: { x: 50, y: 50 },
                    sourcePosition: "right",
                    label: this.$t("flow.start"),
                    data: {
                        icon: "CirclePlay",
                        color: "green",
                        isStartNode: true,
                    },
                    type: "hub",
                };
            },
            reloadFlow() {
                this.getFlow();
            },
            newFlow() {
                this.nodes = [this.createStartNode()];
                this.edges = [];
            },
            async getFlow() {
                try {
                    let stepTools = this.step ? this.step.stepTools : [];
                    const mappedNodes = stepTools.map((stepTool) => ({
                        id: stepTool.id.toString(),
                        position: {
                            x: stepTool.positionX,
                            y: stepTool.positionY,
                        },
                        label: stepTool.tool.name,
                        toolId: stepTool.toolId,
                        data: {
                            order: stepTool.order,
                            icon: "Activity",
                            color: "blue",
                            parameters: stepTool.parameters,
                            isEditableInput: stepTool.tool.isEditableInput,
                            toolType: stepTool.tool.toolType,
                            toolId: stepTool.toolId,
                            stepToolId: stepTool.id,
                            dependencies: stepTool.dependencies,
                            subtitle:
                                stepTool.parameters && stepTool.parameters.length > 0
                                    ? stepTool.parameters[0].promptName
                                    : "",
                        },
                        sourcePosition: "right",
                        targetPosition: "left",
                        type: "hub",
                    }));
                    const mappedEdges = stepTools.slice(0, -1).map((tool, index) => ({
                        id: `${tool.id}-${stepTools[index + 1].id}`,
                        source: tool.id.toString(),
                        target: stepTools[index + 1].id.toString(),
                        animated: false,
                        type: "special",
                    }));

                    const startNode = {
                        ...this.createStartNode(),
                        data: {
                            ...this.createStartNode().data,
                            isActive: true,
                        },
                    };
                    if (stepTools.length > 0) {
                        const firstTool = stepTools[0];
                        mappedEdges.unshift({
                            id: `start-${firstTool.id}`,
                            source: "start",
                            target: firstTool.id.toString(),
                            type: "special",
                        });
                    }

                    this.nodes = [startNode, ...mappedNodes];
                    this.edges = mappedEdges;

                    await this.enrichNodesWithSubtitles(this.nodes);
                } catch (e) {
                    LogService.showMessage("Erro ao carregar fluxo");
                }
            },
            deleteNode(nodeId) {
                this.removeNodeDependency(nodeId);
                this.nodes = this.nodes.filter((node) => node.id !== nodeId);
                this.edges = this.edges.filter(
                    (edge) => edge.source !== nodeId && edge.target !== nodeId
                );
                this.$emit("nodeDeleted", nodeId);
                this.emitFlowChanged();
            },
            removeNodeDependency(nodeId) {
                const idx = this.nodes.findIndex((node) => node.id === nodeId);
                if (idx !== -1) {
                    const node = this.nodes[idx];
                    this.nodes.forEach((n) => {
                        if (n.order > node.data.order) {
                            n.data.dependencies = (n.data.dependencies || []).filter(
                                (d) =>
                                    !(
                                        d.stepOrder === this.step.order &&
                                        d.stepToolOrder === node.data.order
                                    )
                            );
                        }
                    });

                    this.$store.state.tempWorkflow.list.forEach((step) => {
                        if (step.order > this.step.order) {
                            step.stepTools.forEach((stepTool) => {
                                stepTool.dependencies = (stepTool.dependencies || []).filter(
                                    (d) =>
                                        !(
                                            d.stepOrder === this.step.order &&
                                            d.stepToolOrder === node.data.order
                                        )
                                );
                            });
                        }
                    });
                }
            },
            updateNodeInput(nodeId, parameters, dependencies) {
                const idx = this.nodes.findIndex((node) => node.id === nodeId);
                if (idx !== -1) {
                    this.nodes[idx] = {
                        ...this.nodes[idx],
                        data: {
                            ...this.nodes[idx].data,
                            parameters: parameters,
                            dependencies: dependencies,
                        },
                    };
                }
            },
            deleteEdge(edgeId) {
                this.edges = this.edges.filter((edge) => edge.id !== edgeId);
                this.emitFlowChanged();
            },
            openNodeConfig(node) {
                const idx = this.nodes.findIndex((n) => n.id === node.id);
                this.$emit("openNodeConfig", this.nodes, this.nodes[idx]);
            },
            onConnect(params) {
                this.vueFlowInstance?.addEdges([{ ...params, type: "special" }]);
                this.emitFlowChanged();
            },
            onDragOver(event) {
                event.preventDefault();
                event.dataTransfer.dropEffect = "move";
            },
            onDragStart(event, nodeData) {
                event.dataTransfer.setData("application/node-data", JSON.stringify(nodeData));
                event.dataTransfer.effectAllowed = "move";
            },
            onDrop(event) {
                event.preventDefault();
                const reactFlowBounds = event.currentTarget.getBoundingClientRect();
                const nodeData = JSON.parse(event.dataTransfer.getData("application/node-data"));
                const position = this.vueFlowInstance.project({
                    x: event.clientX - reactFlowBounds.left,
                    y: event.clientY - reactFlowBounds.top,
                });
                const newNode = {
                    id: (this.nodes.length + 1).toString(),
                    type: "hub",
                    position,
                    label: nodeData.name,
                    toolId: nodeData.id,
                    data: {
                        order: this.nodes.length + 1,
                        icon: nodeData.toolType === "Quiz" ? "ClipboardList" : "Activity",
                        color: "#000",
                        isStartNode: false,
                        isEditableInput: nodeData.isEditableInput,
                        toolType: nodeData.toolType,
                        parameters: [],
                        toolId: nodeData.id,
                        stepToolId: null,
                        dependencies: [],
                        subtitle: "",
                    },
                };
                this.vueFlowInstance?.addNodes([newNode]);
                this.emitFlowChanged();
            },
            getNodesOrderedByEdges() {
                const edges = this.edges || [];
                const outgoings = {};
                edges.forEach((e) => {
                    const s = String(e.source);
                    if (!outgoings[s]) outgoings[s] = [];
                    outgoings[s].push(String(e.target));
                });
                const visited = new Set();
                const order = [];
                const queue = ["start"];
                while (queue.length) {
                    const id = queue.shift();
                    if (visited.has(id)) continue;
                    visited.add(id);
                    if (id !== "start") order.push(id);
                    (outgoings[id] || []).forEach((target) => queue.push(target));
                }
                const nodeMap = {};
                this.nodes.forEach((n) => {
                    nodeMap[String(n.id)] = n;
                });
                return order.map((id) => nodeMap[id]).filter(Boolean);
            },
            buildFlowPayload() {
                const orderedNodes = this.getNodesOrderedByEdges();
                return orderedNodes.map((node, index) => ({
                    id: parseInt(node.id, 10),
                    toolId: node.toolId,
                    tool: {
                        name: node.label,
                        isEditableInput: node.data.isEditableInput,
                        toolType: node.data.toolType,
                    },
                    positionX: parseFloat(node.position.x.toFixed(2)),
                    positionY: parseFloat(node.position.y.toFixed(2)),
                    order: index + 1,
                    status: "Active",
                    parameters: node.data.parameters,
                    dependsOnStepToolId:
                        node.data.stepToolId && node.data.stepToolId > 0
                            ? node.data.stepToolId
                            : null,
                    dependencies: (node.data.dependencies || []).map((d) => ({
                        stepOrder: d.stepOrder ?? null,
                        stepToolOrder: d.stepToolOrder ?? null,
                    })),
                }));
            },
            showCollapse() {
                this.isActiveCollapse = !this.isActiveCollapse;
            },
            async enrichNodesWithSubtitles(nodes) {
                const promptNodes = nodes.filter(
                    (n) => n.data?.toolType === ToolType.Prompt && n.data?.parameters?.length > 0
                );

                if (promptNodes.length > 0) {
                    const prompts = await PromptService.getPrompts();
                    promptNodes.forEach((node) => {
                        const promptId = node.data.parameters[0].value;
                        if (promptId) {
                            const prompt = prompts.find(
                                (p) => p.id.toString() === promptId.toString()
                            );
                            if (prompt) {
                                node.data.subtitle = prompt.name;
                            }
                        }
                    });
                }
            },
        },
        async mounted() {
            this.getToolsList();
            this.getFlow();
        },
        expose: ["updateNodeInput", "buildFlowPayload", "reloadFlow", "getNodesOrderedByEdges"],
    };
</script>
<style>
    @import "@vue-flow/core/dist/style.css";
    @import "@vue-flow/core/dist/theme-default.css";

    .vue-flow-container {
        height: calc(100vh - 200px);
    }
</style>
<style scoped>
    .btn-outline-quiz {
        color: #7c4dff;
        border: 1px solid #7c4dff;
        background: transparent;
    }

    .btn-outline-quiz:hover {
        color: #6a3ee6;
        border-color: #6a3ee6;
        background: #f3eeff;
    }

    .btn-outline-quiz:active {
        color: #5a32cc;
        border-color: #5a32cc;
        background: #e8e0ff;
    }

    .btn-outline-quiz:disabled {
        color: #b8a7ff;
        border-color: #d6ccff;
        background: #faf8ff;
    }

    .vue-flow__node-default {
        background: var(--color-card-content) !important;
        color: var(--color-body-content) !important;
    }
</style>
