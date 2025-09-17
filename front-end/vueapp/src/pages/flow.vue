<template>
    <main :key="changeLanguage">
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div class="card mb-3">
                    <div class="card-body">
                        <!-- controles aqui -->
                    </div>
                </div>
                <div class="card mb-3">
                    <div class="card-body vue-flow-container p-0">
                        <VueFlow :nodes="nodes" :edges="edges" :style="{ width: '100%', height: '100%' }" @connect="onConnect">
                            <Background  patternColor="#BCD5F2" gap="10" variant="dots" size="1"/>
                            <template #node-hub="props">
                                <HubNode :node="props" @deleteNode="deleteNode" @openNodeConfig="openNodeConfig"/>
                            </template>
                            <template #edge-special="props">
                                <SpecialEdge v-bind="props" @deleteEdge="deleteEdge" :data="props"/>
                            </template>
                        </VueFlow>
                    </div>
                </div>               
            </div>
        </div>
    </main>
</template>

<script>
import { VueFlow, useVueFlow } from '@vue-flow/core'
import { Background } from '@vue-flow/background'
import HubNode from '@/components/flow/HubNode.vue';
import FlowService from '@/services/flow/FlowService';
import SpecialEdge from '../components/flow/SpecialEdge.vue';

const { addEdges } = useVueFlow()

    export default {
        name: "FlowPage",
        props: {
            flowId: {
                type: Number,
                required: false
            }
        },
        data() {
            return{
                nodes:[],
                edges:[],
            }
        },
        components: {
            VueFlow,
            Background,
            HubNode,
            SpecialEdge
        },
        methods: {
            getFlow(){
                FlowService.getFlowById(this.flowId).then(response => {
                    this.nodes = response.nodes;
                    this.edges = response.edges;
                }).catch(e => {
                    console.log(e);
                });
            },
            deleteNode(nodeId) {
                this.nodes = this.nodes.filter(node => node.id !== nodeId);
                this.edges = this.edges.filter(edge => edge.source !== nodeId && edge.target !== nodeId);
            },
            deleteEdge(edgeId) {
                console.log("Deleting edge with ID:", edgeId);
                this.edges = this.edges.filter(edge => edge.id !== edgeId);
            },
            openNodeConfig(nodeId) {
                const node = this.nodes.find(n => n.id === nodeId);
                if (node) {
                    alert(`Open config for node: ${node.label}`);
                }
            },
            onConnect(params) {
                this.edges.value = addEdges({ ...params, type: 'step' }, this.edges.value)
            }
        },
        mounted(){
            this.getFlow();
        }
    };
</script>

<style>
/* import the necessary styles for Vue Flow to work */
@import '@vue-flow/core/dist/style.css';

/* import the default theme, this is optional but generally recommended */
@import '@vue-flow/core/dist/theme-default.css';

.vue-flow-container{
    height: calc(100vh - 200px);
}
</style>