<template>
    <main :key="changeLanguage">
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div class="card mb-3">
                    <div class="card-body palette">
                             <div
                        class="palette-item"
                        draggable="true"
                        @dragstart="onDragStart($event, {id:0,name:'processo'})"
                    >
                        Processo
                    </div>
                    <div
                        class="palette-item"
                        draggable="true"
                        @dragstart="onDragStart($event,  {id:0,name:'decisao'})"
                    >
                        Decisão
                    </div>
                    </div>
                </div>
                <div class="card mb-3">
                    <div class="card-body vue-flow-container p-0">
                        <VueFlowComponent 
                            :stepId="1" 
                            :isEditMode="true" 
                            @openNodeConfig="openNodeConfig" ref="vueflowComponent"
                        />
                    </div>
                </div>
                <div class="offcanvas offcanvas-end" tabindex="-1" id="offcanvasRight" aria-labelledby="offcanvasRightLabel" ref="sidebar">
                    <div class="offcanvas-header">
                        <h5 id="offcanvasRightLabel">Configurar I/O: {{nodeFlow.label}}</h5>
                        <button type="button" class="btn-close text-reset" data-bs-dismiss="offcanvas" aria-label="Close" @click="closeSidebar"></button>
                    </div>
                    <div class="offcanvas-body">
                        <div class="mb-3">
                            <h6>Inputs</h6><hr>
                            <div class="background-div">
                                <div v-if="nodeFlow.data.input.type == 'string'">
                                    <textarea class="form-control" id="exampleFormControlTextarea1" rows="3" v-model="nodeFlow.data.input.value"></textarea>
                                </div>
                            </div>
                            <div class="mt-4">
                                <button type="button" class="btn btn-primary" @click="updateNode">{{$t("labelSave")}}</button>
                            </div>
                        </div> 
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
    import VueFlowComponent from '@/components/flow/VueFlowComponent.vue';
    export default {
        name: "FlowPage",
        components: {
            VueFlowComponent
        },
        props: {
            stepId: {
                type: Number,
                required: true
            }
        },
        data() {
            return {
                nodeFlow: {
                    data: {
                        input: {
                            type: "",
                            value: "",
                        }
                    },
                    label: ""
                },
            }
        },
        methods: {
            openNodeConfig(node) {
                this.nodeFlow = node;
                console.log(this.nodeFlow);
                const sidebar = new bootstrap.Offcanvas(this.$refs.sidebar);
                sidebar.show();
            },
            closeSidebar() {
                sidebar.hide();
            },
            updateNode() {
                this.$refs.vueflowComponent.updateNode(this.nodeFlow);
            }
        },
    }
</script>

<style>
/* import the necessary styles for Vue Flow to work */
@import '@vue-flow/core/dist/style.css';

/* import the default theme, this is optional but generally recommended */
@import '@vue-flow/core/dist/theme-default.css';

.vue-flow-container{
    height: calc(100vh - 200px);
}
    .background-div {
        background-color: rgb(249 250 251);
        border-width: 1px;
        border-radius: 0.375rem;
        padding: 15px;
    }

    .font-medium {
        font-weight: 500;
    }
</style>