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
                        <button
                            v-for="tool in toolsList"
                            :key="tool.id"
                            class="btn btn-outline-primary btn-sm me-2 mt-2 palette-item"
                            draggable="true"
                            @dragstart="onDragStart($event, { id: tool.id, name: tool.name })"
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
            @nodes-change="onNodesChange"
        >
            <Background patternColor="#BCD5F2" gap="10" variant="dots" size="1" />
            <template #node-hub="props">
                <HubNode :node="props" @deleteNode="deleteNode" @openNodeConfig="openNodeConfig" />
            </template>
            <template #edge-special="props">
                <SpecialEdge v-bind="props" @deleteEdge="deleteEdge" :data="props" />
            </template>
        </VueFlow>
    </div>
</template>

<script>
import { VueFlow } from '@vue-flow/core'
import { Background } from '@vue-flow/background'
import HubNode from '@/components/flow/HubNode.vue';
import FlowService from '@/services/flow/FlowService';
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
        isEditMode: {
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
                data: { icon: "CirclePlay", color: "green", isStartNode: true },
                type: "hub"
            };
        },
        newFlow() {
            this.nodes = [this.createStartNode()];
            this.edges = [];
        },
        async getFlow() {
            try {
                const stepTools = await FlowService.getStepToolsByStepId(this.stepId);
                const dependencies = await FlowService.getStepToolDependenciesByStepId(this.stepId);

                const mappedNodes = stepTools.map(tool => ({
                    id: tool.id.toString(),
                    position: { x: tool.positionX, y: tool.positionY },
                    label: tool.label,
                    data: { icon: "MessageCircle", color: "blue", input: tool.input || null },
                    sourcePosition: "right",
                    targetPosition: "left",
                    type: "hub"
                }));

                const mappedEdges = dependencies.map(dep => ({
                    id: `${dep.StepToolIdFrom}-${dep.StepToolIdTo}`,
                    source: dep.StepToolIdFrom.toString(),
                    target: dep.StepToolIdTo.toString(),
                    animated: true,
                    type: "special"
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
                LogService.showMessage("Erro ao carregar fluxo");
            }
        },
        deleteNode(nodeId) {
            this.nodes = this.nodes.filter(node => node.id !== nodeId);
            this.edges = this.edges.filter(edge => edge.source !== nodeId && edge.target !== nodeId);
        },
        updateNode(nodeFlow) {
            const idx = this.nodes.findIndex(node => node.id === nodeFlow.id);
            this.nodes[idx] = nodeFlow;
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
                data: { icon: 'Activity', color: '#000', isStartNode: false }
            }
            this.vueFlowInstance?.addNodes([newNode])
        },
        buildFlowPayload() {
            const stepTools = this.nodes
                .filter(node => node.id !== "start")
                .map((node, index) => ({
                    Id: parseInt(node.id, 10),
                    ToolId: node.data.toolId || null,
                    Label: node.label,
                    PositionX: node.position.x,
                    PositionY: node.position.y,
                    Order: index + 1,
                    Status: "Active",
                    Input: node.data.input || null
                }));

            const stepToolDependencies = this.edges
                .filter(edge => edge.source !== "start")
                .map(edge => ({
                    StepToolIdFrom: parseInt(edge.source, 10),
                    StepToolIdTo: parseInt(edge.target, 10)
                }));

            return {
                StepTools: stepTools,
                StepToolDependencies: stepToolDependencies
            };
        },
        async saveFlow() {
            try {
                const payload = this.buildFlowPayload();
                await FlowService.saveFlow(this.stepId, payload);
               
            } catch (e) {
                
            }
        },
        showCollapse() {
            this.isActiveCollapse = !this.isActiveCollapse;
        },
    },
    mounted() {
        this.getToolsList();
        if (!this.isEditMode) {
            this.newFlow();
        } else {
            this.getFlow();
        }
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