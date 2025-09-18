<template>
    <VueFlow v-model:nodes="nodes" v-model:edges="edges" :style="{ width: '100%', height: '100%' }" @connect="onConnect"
        @pane-ready="onPaneReady" @drop="onDrop" @dragover="onDragOver" @nodes-change="onNodesChange">
        <Background patternColor="#BCD5F2" gap="10" variant="dots" size="1" />
        <template #node-hub="props">
            <HubNode :node="props" @deleteNode="deleteNode" @openNodeConfig="openNodeConfig" />
        </template>
        <template #edge-special="props">
            <SpecialEdge v-bind="props" @deleteEdge="deleteEdge" :data="props" />
        </template>
    </VueFlow>
</template>

<script>
import { VueFlow } from '@vue-flow/core'
import { Background } from '@vue-flow/background'
import HubNode from '@/components/flow/HubNode.vue';
import FlowService from '@/services/flow/FlowService';
import SpecialEdge from '@/components/flow/SpecialEdge.vue';
import LogService from '@/services/log/logService';

export default {
    name: "VueFlowComponent",
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
            nodes: [],
            edges: [],
            vueFlowInstance: null,
        }
    },
    components: {
        VueFlow,
        Background,
        HubNode,
        SpecialEdge
    },
    methods: {
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

                // StepTools -> Nodes
                const mappedNodes = stepTools.map(tool => ({
                    id: tool.Id.toString(),
                    position: { x: tool.PositionX, y: tool.PositionY },
                    label: tool.Label,
                    data: { icon: "MessageCircle", color: "blue", input: tool.input || null },
                    sourcePosition: "right",
                    targetPosition: "left",
                    type: "hub"
                }));

                // StepToolDependencies -> Edges
                const mappedEdges = dependencies.map(dep => ({
                    id: `${dep.StepToolIdFrom}-${dep.StepToolIdTo}`,
                    source: dep.StepToolIdFrom.toString(),
                    target: dep.StepToolIdTo.toString(),
                    animated: true,
                    type: "special"
                }));

                // Edge inicial (Início -> primeiro StepTool)
                if (stepTools.length > 0) {
                    const firstTool = stepTools[0];
                    mappedEdges.unshift({
                        id: `start-${firstTool.Id}`,
                        source: "start",
                        target: firstTool.Id.toString(),
                        type: "special"
                    });
                }

                this.nodes = [this.createStartNode(), ...mappedNodes];
                this.edges = mappedEdges;

            } catch (e) {
                LogService.showMessage("Erro ao carregar fluxo");
            }
        },
        deleteNode(nodeId) {
            this.nodes = this.nodes.filter(node => node.id !== nodeId);
            this.edges = this.edges.filter(edge => edge.source !== nodeId && edge.target !== nodeId);
        },
        deleteEdge(edgeId) {
            this.edges = this.edges.filter(edge => edge.id !== edgeId);
        },
        openNodeConfig(nodeId) {
            const node = this.nodes.find(n => n.id === nodeId);
            if (node) {
                alert(`Open config for node: ${node.label}`);
            }
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
            // Mapeia nodes (ignorando o nó "start")
            const stepTools = this.nodes
                .filter(node => node.id !== "start")
                .map((node, index) => ({
                    Id: parseInt(node.id, 10),
                    ToolId: node.data.toolId || null,
                    Label: node.label,
                    PositionX: node.position.x,
                    PositionY: node.position.y,
                    Order: index + 1,
                    Status: "Active"
                }));

            // Mapeia edges (ignorando edges que saem do "start")
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
                LogService.showMessage("Fluxo salvo com sucesso!");
            } catch (e) {
                LogService.showMessage("Erro ao salvar fluxo");
            }
        }
    },
    mounted() {
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