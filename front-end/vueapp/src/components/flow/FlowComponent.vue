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
                    <button 
                        class="btn btn-primary btn-sm me-2"
                        @click="save"
                    >
                        <LucideIcon icon="Save" :size="15" />
                        {{ $t("flow.save") }}
                    </button>
                </div>
            </div>
            <hr/>            
            <VueFlowComponent 
                :isEditMode="isEdit"
                :stepId="id"
                @openNodeConfig="openNodeConfig" 
                ref="vueflowComponent"
            />
            <div class="offcanvas offcanvas-end" tabindex="-1" id="offcanvasRight" aria-labelledby="offcanvasRightLabel" ref="sidebar">
                <div class="offcanvas-header">
                    <h5 id="offcanvasRightLabel">{{$t("flow.sidebarTitle")}} {{nodeFlow.label}}</h5>
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
    </main>
</template>

<script>
    import VueFlowComponent from '@/components/flow/VueFlowComponent.vue';
    export default {
        name: "FlowPage",
        props: {
            stepId: {
                type: Number,
                required: true
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
        },
        data() {
            return {
                isActiveCollapse: false,
                nodeFlow: {
                    data: {
                        input: {
                            type: "",
                            value: "",
                        }
                    },
                    label: ""
                },
            };
        },
        components: {
            VueFlowComponent,
        },
        methods: {
            setEdit() {
                console.log("Gotcha")
            },
            redirectToIndex() {
                if(this.isEdit) {
                    return this.$router.push({ name: "EditWorkflow" });
                }
                return this.$router.push({ name: "NewWorkflow" });
            },
            showCollapse() {
                this.isActiveCollapse = !this.isActiveCollapse;
            },
            save() {
                this.$store.commit('setFlowByStep', {
                    stepId: this.stepId,
                    flowData: this.nodeFlow,
                });
            },
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
            save() {
                try {
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
            }
        },
        created() {
            this.setEdit();
        },
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