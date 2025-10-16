<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="row align-items-center">
                <div class="col-auto">
                    <div class="row">
                        <div class="col-2">
                            <button class="btn btn-outline-primary btn-table btn-sm table-btn" @click="redirectToIndex">
                                <LucideIcon icon="ArrowLeft" />
                            </button>
                        </div>
                        <div class="col-10">
                            <div>
                                <h5 class="mb-0 fw-bold">{{ $t("flow.title") }}</h5>
                                <p><small class="text-muted">{{ $t("flow.subtitle") }}</small></p>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-auto ms-auto">
                    <button class="btn btn-primary btn-sm me-2" @click="save">
                        <LucideIcon icon="Save" :size="15" />
                        {{ $t("flow.save") }}
                    </button>
                </div>
            </div>
            <hr />
            <VueFlowComponent :isEdit="isEdit" :stepId="stepId" :stepOrder="stepOrder" @openNodeConfig="openNodeConfig"
                              ref="VueflowComponent" />
            <div class="offcanvas offcanvas-end" tabindex="-1" id="offcanvasRight" aria-labelledby="offcanvasRightLabel"
                 ref="sidebar">
                <div class="offcanvas-header">
                    <h5 id="offcanvasRightLabel">{{ $t("flow.sidebarTitle") }} {{ nodeFlow.label }}</h5>
                    <button type="button" class="btn-close text-reset" data-bs-dismiss="offcanvas" aria-label="Close"
                            @click="closeSidebar"></button>
                </div>
                <div :key="toolTypeSelected" class="offcanvas-body">
                    <div>
                        <div class="mb-3">
                            <div v-if="toolTypeSelected == 'Prompt'">
                                <h6>Prompts</h6>
                                <hr>
                                <div class="background-div">
                                    <select class="form-select" v-model="idSelected">
                                        <option v-for="item in promptlist" :key="item.id" :value="item.id">
                                            {{ item.name }}
                                        </option>
                                    </select>
                                </div>
                            </div>
                            <div v-else>
                                <h6>Inputs</h6>
                                <hr>
                                <div class="background-div">
                                    <textarea class="form-control" id="exampleFormControlTextarea1" rows="3"
                                              v-model="valueInput"></textarea>
                                </div>
                            </div>
                            <div class="mt-4">
                                <button type="button" class="btn btn-primary"
                                        @click="updateNode">
                                    {{ $t("labelSave") }}
                                </button>
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
    import PromptService from "@/services/prompts/PromptsService";
    import ToolsServices from '@/services/tools/ToolsServices';
    export default {
        name: "FlowPage",
        props: {
            stepId: {
                type: Number,
                required: false,
            },
            isEdit: {
                type: Boolean,
                required: false,
                default: false,
            },
            id: {
                type: Number,
                required: false,
                default: null,
            },
            stepOrder: {
                type: Number,
                required: false,
                default: 0,
            }
        },
        data() {
            return {
                isActiveCollapse: false,
                nodeFlow: {},
                valueInput: "",
                idSelected: 0,
                promptlist: [],
                toolTypeSelected: "",
            };
        },
        components: {
            VueFlowComponent,
        },
        methods: {
            redirectToIndex() {
                if (this.isEdit) {
                    return this.$router.push({ name: "EditWorkflow" });
                }
                return this.$router.push({ name: "NewWorkflow" });
            },
            showCollapse() {
                this.isActiveCollapse = !this.isActiveCollapse;
            },
            openNodeConfig(node) {
                this.findAllPrompts();
                this.toolTypeSelected = node.data.toolType;
                if (this.toolTypeSelected == "Prompt") {
                    this.idSelected = node.data.input;
                }
                else {
                    this.valueInput = node.data.input;
                }
                this.nodeFlow = node;
                const sidebar = new bootstrap.Offcanvas(this.$refs.sidebar);
                sidebar.show();
            },
            closeSidebar() {
                sidebar.hide();
            },
            updateNode() {
                let valueUpdate = "";
                if (this.valueInput != "") {
                    valueUpdate = this.valueInput;
                }
                else {
                    valueUpdate = this.idSelected.toString();
                }
                console.log(valueUpdate);
                this.$refs.VueflowComponent.updateNodeInput(this.nodeFlow.id, valueUpdate);
                try {
                    return this.$notify({
                        title: 'flow.title',
                        message: 'flow.formFlow.editFlowNodeSuccess',
                        variant: 'success',
                        icon: 'CircleCheckBig',
                    });
                }
                catch (e) {
                    this.$notify({
                        title: 'flow.title',
                        message: 'flow.formFlow.editFlowNodeFail',
                        variant: 'danger',
                        icon: 'CircleX',
                    });
                }
            },
            findAllPrompts() {
                PromptService.getPrompts()
                    .then((response) => {
                        this.promptlist = response;
                        console.log(response);
                    });
            },
            getToolsList() {
                ToolsServices.getToolsList()
                    .then((response) => {
                        this.toolsList = response;
                    });
            },
            save() {
                try {
                    let nodesList = this.$refs.VueflowComponent.buildFlowPayload();
                    console.log(nodesList)
                    this.$store.commit('setFlowByStep', {
                        stepOrder: this.stepOrder,
                        flowData: nodesList,
                        stepId: this.stepId
                    });
                    this.redirectToIndex();
                    return this.$notify({
                        title: 'flow.title',
                        message: 'flow.formFlow.progressFlowSuccess',
                        variant: 'success',
                        icon: 'CircleCheckBig',
                    });
                }
                catch (e) {
                    this.$notify({
                        title: 'flow.title',
                        message: 'flow.formFlow.progressFlowFail',
                        variant: 'danger',
                        icon: 'CircleX',
                    });
                }
            },
        },
        computed: {
            selectedItem() {
                if (!this.idSelected != 0)
                    return this.promptlist.find(item => item.id === this.idSelected)
            },
        },
    };
</script>


<style>
/* import the necessary styles for Vue Flow to work */
@import '@vue-flow/core/dist/style.css';

/* import the default theme, this is optional but generally recommended */
@import '@vue-flow/core/dist/theme-default.css';

.vue-flow-container {
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