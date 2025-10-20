<template>
    <div class="row mb-2">
        <div class="col">
            <button class="btn btn-primary btn-sm me-2" data-bs-toggle="collapse" data-bs-target="#toolsCollapse"
                aria-expanded="false" aria-controls="toolsCollapse" @click="showCollapse">
                <LucideIcon icon="Plus" :size="15" />
                {{ isActiveCollapse ? $t("flow.hideTools") : $t("flow.showTools") }}
            </button>
        </div>
    </div>
    <div class="collapse" id="toolsCollapse">
        <div class="mt-3 mb-3">
            <div class="card mb-3">
                <div class="card-body palette">
                    <div>
                        <button v-for="tool in toolsList" :key="tool.id"
                            class="btn btn-outline-primary btn-sm me-2 mt-2 palette-item" draggable="true"
                            @dragstart="onDragStart($event, { id: tool.id, name: tool.name, isEditableInput: tool.isEditableInput, toolType: tool.toolType })">
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
import { VueFlow } from '@vue-flow/core';
import { Background } from '@vue-flow/background'
import HubNode from '@/components/flow/HubNode.vue';
import SpecialEdge from '@/components/flow/SpecialEdge.vue';
import LogService from '@/services/log/logService';
import ToolsServices from '@/services/tools/ToolsServices';

export default {
    name: "VueFlowComponent",
    emits: ['openNodeConfig'],
    props: {
        stepId: {
            type: Number,
            required: false,
            default: null
        },
        stepOrder: {
            type: Number,
            required: false,
            default: null
        },
        isEdit: {
            type: Boolean,
            required: false,
            default: false
        }
    },
    data() {
        return {
            toolsList: [],
            nodes: [],
            edges: [],
            vueFlowInstance: null,
            isActiveCollapse: false,
        }
    },
    components: {
        VueFlow,
        Background,
        HubNode,
        SpecialEdge
    },
    methods: {
        getToolsList() {
            ToolsServices.getToolsList()
                .then((response) => {
                    this.toolsList = response;
                });
        },
        onPaneReady(instance) {
            this.vueFlowInstance = instance
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
                    isStartNode: true 
                },
                type: "hub"
            };
        },
        newFlow() {
            this.nodes = [this.createStartNode()];
            this.edges = [];
        },
        async getFlow() {
            try {
                let step = this.$store.state.tempWorkflow.list.find(item => {
                    if (this.isEdit && this.stepId == 0) {
                        if (item.order == this.stepOrder) {
                            return item.stepTools;
                        }
                    } else if(this.isEdit) {
                        if (item.id == this.stepId) {
                            return item.stepTools;
                        }
                    } else {
                        if (item.order == this.stepOrder) {
                            return item.stepTools;
                        }
                    }
                });

                let stepTools = step ? step.stepTools : [];
                const mappedNodes = stepTools.map(stepTool => ({
                    id: stepTool.id.toString(),
                    position: { x: stepTool.positionX, y: stepTool.positionY },
                    label: stepTool.tool.name,
                    toolId: stepTool.toolId,
                    data: { 
                        icon: "Activity", 
                        color: "blue", 
                        input: stepTool?.input || null, 
                        isEditableInput: stepTool.tool.isEditableInput,
                        toolType: stepTool.tool.toolType,
                    },
                    sourcePosition: "right",
                    targetPosition: "left",
                    type: "hub"
                }));
                console.log(mappedNodes, "mapped");
                const mappedEdges = stepTools.slice(0, -1).map((tool, index) => ({
                    id: `${tool.id}-${stepTools[index + 1].id}`,
                    source: tool.id.toString(),
                    target: stepTools[index + 1].id.toString(),
                    animated: false,
                    type: "special",
                }));

                const startNode = { ...this.createStartNode(), data: { ...this.createStartNode().data, isActive: true } };
                if (stepTools.length > 0) {
                    const firstTool = stepTools[0];
                    mappedEdges.unshift({
                        id: `start-${firstTool.id}`,
                        source: "start",
                        target: firstTool.id.toString(),
                        type: "special"
                    });
                }

                this.nodes = [startNode, ...mappedNodes];
                this.edges = mappedEdges;

            } catch (e) {
            console.log(e);
                LogService.showMessage("Erro ao carregar fluxo");
            }
        },
        deleteNode(nodeId) {
            this.nodes = this.nodes.filter(node => node.id !== nodeId);
            this.edges = this.edges.filter(edge => edge.source !== nodeId && edge.target !== nodeId);
        },
        updateNodeInput(nodeId, newInput) {
            console.log(newInput);
            const idx = this.nodes.findIndex(node => node.id === nodeId);
            if (idx !== -1) {
                this.nodes[idx] = {
                    ...this.nodes[idx],
                    data: {
                        ...this.nodes[idx].data,
                        input: newInput
                    }
                };
            }
        },
        deleteEdge(edgeId) {
            this.edges = this.edges.filter(edge => edge.id !== edgeId);
        },
        openNodeConfig(node) {
            this.$emit('openNodeConfig', node)
        },
        onConnect(params) {
            this.vueFlowInstance?.addEdges([{ ...params, type: 'special' }])
        },
        onDragOver(event) {
            event.preventDefault()
            event.dataTransfer.dropEffect = 'move'
        },
        onDragStart(event, nodeData) {
            event.dataTransfer.setData('application/node-data', JSON.stringify(nodeData))
            event.dataTransfer.effectAllowed = 'move'
        },
        onDrop(event) {
            event.preventDefault()
            const reactFlowBounds = event.currentTarget.getBoundingClientRect()
            const nodeData = JSON.parse(event.dataTransfer.getData('application/node-data'))
            const position = this.vueFlowInstance.project({
                x: event.clientX - reactFlowBounds.left,
                y: event.clientY - reactFlowBounds.top,
            })
            const newNode = {
                id: (this.nodes.length + 1).toString(),
                type: 'hub',
                position,
                label: nodeData.name,
                toolId: nodeData.id,
                data: { 
                    icon: 'Activity', 
                    color: '#000', 
                    isStartNode: false, 
                    isEditableInput: nodeData.isEditableInput,
                    toolType: nodeData.toolType,
                    input: nodeData.input || null
                }
            }
            this.vueFlowInstance?.addNodes([newNode])
        },
        buildFlowPayload() {
            console.log(this.nodes);
            return this.nodes
                .filter(node => node.id !== "start")
                .map((node, index) => ({
                    id: parseInt(node.id, 10),
                    toolId: node.toolId,
                    tool: { name: node.label, isEditableInput: node.data.isEditableInput, toolType: node.data.toolType },
                    positionX: parseFloat((node.position.x).toFixed(2)),
                    positionY: parseFloat((node.position.y).toFixed(2)),
                    order: index + 1,
                    status: "Active",
                    input: node.data.input|| null,
                    dependsOnStepToolId: index,
                }));
        },
        showCollapse() {
            this.isActiveCollapse = !this.isActiveCollapse;
        },
    },
    mounted() {
        this.getToolsList();
        this.getFlow();
    }
};
</script>

<style>
@import '@vue-flow/core/dist/style.css';
@import '@vue-flow/core/dist/theme-default.css';

.vue-flow-container {
    height: calc(100vh - 200px);
}
</style>