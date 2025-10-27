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
                <div class="offcanvas-body">
                    <div class="cover" v-if="loadingWebhooks || loadingInputs">
                        <div class="spinner-cover">
                            <LucideIcon icon="Loader" :size="24" class="me-1 animate-spin" />
                        </div>
                    </div>
                    <div v-if="isN8NTool" class="mb-3">
                        <select
                            class="form-select form-select-sm w-auto mb-3"
                            v-model="connector"
                            @change="changeWebhook"
                        >
                            <option value="" disabled>{{ $t("flow.sidebar.filter") }}</option>
                            <option v-for="connector in connectors"
                                    :key="connector.id"
                                    :value="connector.webhookId">
                                {{ connector.name }}
                            </option>
                        </select>
                        <div v-for="field in formFields" :key="field.name">
                            <div class="mb-3" v-if="field.type === 'string' || field.type === 'integer'" :type="field.type === 'integer' ? 'number' : 'text'">
                                <label :for="field.name" class="form-label">{{ field.label }}</label>
                                <input class="form-control form-control-sm" :id="field.name" v-model="formData[field.name]" />
                            </div>
                            <div v-else-if="field.type === 'boolean'" class="form-check mb-3">
                                <input class="form-check-input"
                                       type="checkbox"
                                       :id="field.name"
                                       v-model="formData[field.name]" />
                                <label class="form-check-label" for="flexCheckDefault"> {{ field.label }} </label>
                            </div>
                            <div v-else-if="field.type === 'array'">
                                <h6> {{ field.label }}</h6>
                                <div v-for="(item, index) in formData[field.name]" :key="index">
                                    <div class="mb-3" v-for="child in field.children" :key="child.name">
                                        <label v-if="child.label" :for="child.name" class="form-label">{{ child.label }}</label>

                                        <label v-else :for="child.name" class="form-label text-capitalize">{{ child.name }}</label>
                                        <input :id="child.name" v-model="formData[field.name][index][child.name]" class="form-control form-control-sm" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="mt-4">
                            <button type="button" class="btn btn-primary" @click="updateNodeWithForm">{{ $t("labelSave") }}</button>
                        </div>
                    </div>
                    <div v-else-if="isPromptTool">
                        <h6>Prompts</h6>
                        <hr>
                        <div class="background-div">
                            <select class="form-select" v-model="idSelected">
                                <option v-for="item in promptlist" :key="item.id" :value="item.id">
                                    {{ item.name }}
                                </option>
                            </select>
                        </div>
                        <div class="mt-4">
                            <button type="button" class="btn btn-primary"
                                    @click="updateNode">
                                {{ $t("labelSave") }}
                            </button>
                        </div>
                    </div>
                    <div v-else class="mb-3">
                        <h6>Inputs</h6>
                        <hr>
                        <div class="background-div" v-for="(param, index) in parameters" :key="index">
                            <textarea class="form-control" id="exampleFormControlTextarea1" rows="3"
                                      v-model="parameters[index].value"></textarea>
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
    </main>
</template>

<script>
    import VueFlowComponent from '@/components/flow/VueFlowComponent.vue';
    import AutomationServices from '@/services/automation/AutomationServices';
    import PromptService from "@/services/prompts/PromptsService";
    import ToolType from '@/constants/ToolType';

    export default {
        name: "FlowPage",
        props: {
            stepId: {
                type: Number,
                required: false,
            },
            stepTools: {
                type: [Object, Array],
                required: false,
                default: () => [],
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
                parameters: [],
                connectors: [],
                connector: "",
                formFields: [],
                formData: [],
                sidebar: null,
                loadingWebhooks: false,
                loadingInputs: false,
                valueInput: "",
                idSelected: 0,
                promptlist: [],
                toolType: "",
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
            changeWebhook() {
                this.getInputs(false);
            },
            getInputs(dataFromParameters) {										   
                this.loadingInputs = true;
                let params ={
                    toolId: this.nodeFlow.data.toolId,
                    workflowId: this.connector
                }
                AutomationServices.getWorkflowWebhookInputs(params)
                    .then((response) => {      
                        if (response.error === undefined) {
                            this.formFields = response;                         
                            this.formData = [];
                            if (dataFromParameters){
                                this.formData = JSON.parse(this.parameters[0].value);
                            }
                            else{
                                this.formData = this.transformToObject(response); 
                            }
                        }
                        else{
                            this.$notify({
                                title: "flow.title",
                                message: "flow.formFlow.connectorWorkflowFail",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                    })
                .finally(() => {
                    this.loadingInputs = false;
                });
            },
            transformToObject(fields) {
                const result = {};
                fields.forEach(field => {
                    if (field.type === 'array') {
                        if (field.children && field.children.length > 0) {
                            result[field.name] = [this.transformToObject(field.children)];
                        } else {
                            result[field.name] = [];
                        }
                    } else {
                        result[field.name] = this.getDefaultValue(field.type);
                    }
                });

                return result;
            },
            getDefaultValue(type) {
                switch (type) {
                    case 'array':
                        return [];
                    default:
                        return null;
                }
            },
            fillFormFields() {
                if (this.parameters.length > 0 && this.parameters.value) {
                    const data = JSON.parse(this.parameters.value)
                    this.fillValues(this.formFields, data)
                }
            },
            openNodeConfig(node) {
                this.nodeFlow = node;
                this.parameters = node.data.parameters;
                this.toolType = node.data.toolType;

                if (this.isTargetTool(ToolType.N8N)){
                    this.loadingWebhooks = true
                    this.resetFormConnector();              
                    AutomationServices.getWorkflows(node.data.toolId)
                        .then((result) => {
                            if (result.error === undefined) {                                
                                this.connectors = result;
                                this.parameters = node.data.parameters;
                                if (this.parameters.length===0){
                                    this.parameters.push({stepToolId: 0, value: null, requiredFile: false, webhookId: null});
                                }
                                else{
                                    this.connector = this.parameters[0].webhookId;
                                    this.getInputs(true);
                                }
                            }                    
                            else{
                                this.$notify({
                                    title: "flow.title",
                                    message: "flow.formFlow.connectorWorkflowFail",
                                    variant: "danger",
                                    icon: "CircleX",
                                });
                            }
                        })
                        .finally(() => {
                            this.loadingWebhooks = false;
                        });
                }
                else if (this.isTargetTool(ToolType.Prompt)){
                    this.findAllPrompts();
                    if (this.parameters.length === 0) {
                        this.parameters.push({ stepToolId: 0, value: null, requiredFile: false, webhookId: null });
                    }
                    else {
                        this.idSelected = parseInt(this.parameters[0]?.value);
                    }
                }
                else if (this.parameters.length === 0) {
                    this.parameters.push({ stepToolId: 0, value: null, requiredFile: false, webhookId: null });
                }

                this.sidebar = new bootstrap.Offcanvas(this.$refs.sidebar);
                this.sidebar.show();            
            },
            closeSidebar() {
                const sidebarEl = this.$refs.sidebar;
                const sidebar = bootstrap.Offcanvas.getInstance(sidebarEl);
                if (sidebar) {
                    sidebar.hide();
                }
            },
            updateNode() {
                this.closeSidebar();
                if (this.idSelected) {
                    this.parameters[0].value = this.idSelected.toString();
                }
                this.$refs.VueflowComponent.updateNodeInput(this.nodeFlow.id, this.parameters);
                this.showMessage();
            },
            showMessage() {
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
            updateNodeWithForm() {
                this.parameters[0].requiredFile = false;
                if (Object.prototype.hasOwnProperty.call(this.formData, 'requiredFile')) {
                    this.parameters[0].requiredFile = this.formData['requiredFile'];
                }
                this.parameters[0].value = JSON.stringify(this.formData);
                this.parameters[0].webhookId = this.connector;
                this.$refs.VueflowComponent.updateNodeInput(this.nodeFlow.id, this.parameters);
                this.closeSidebar();
                this.showMessage();
            },
            save() {
                try {
                    let nodesList = this.$refs.VueflowComponent.buildFlowPayload();
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
            fillValues(fields, data) {
                fields.forEach(field => {
                    if (Object.prototype.hasOwnProperty.call(data, field.name)) {
                        const value = data[field.name]

                        if (field.type === 'array' && Array.isArray(value)) {
                            field.value = value.map(item => {
                                const clonedChildren = field.children
                                    ? field.children.map(c => ({
                                        ...c,
                                        value: null,
                                        children: c.children ? [...c.children] : [],
                                    }))
                                    : []

                                this.fillValues(clonedChildren, item)
                                return clonedChildren
                            })
                        } else if (field.children && field.children.length > 0 && typeof value === 'object') {
                            this.fillValues(field.children, value)
                        } else {
                            field.value = value
                        }
                    }
                })
            },
            findAllPrompts() {
                PromptService.getPrompts()
                    .then((response) => {
                        this.promptlist = response;
                    });
            },
            resetFormConnector(){
                this.connectors = [];
                this.parameters = [];
                this.formFields = [];
                this.formData = [];
                this.connector = "";
            },
            isTargetTool(targetToolType){
                return this.toolType?.toLowerCase().includes(targetToolType.toLowerCase()) || false
            },
        },
        computed: {
            selectedItem() {
                if (this.idSelected != 0)
                    return this.promptlist.find(item => item.id === this.idSelected)
                return null;
            },
            isN8NTool(){
                return this.isTargetTool(ToolType.N8N);
            },
            isPromptTool(){
                return this.isTargetTool(ToolType.Prompt);
            }
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

    .animate-spin {
        animation: spin 1s linear infinite;
        color: var(--color-bg-icon-active);
    }

    .spinner-cover {
        position: absolute;
        inset: calc(.25rem * 0);
        align-items: center;
        display: flex;
        justify-content: center;
        z-index: 10;
        background-color: var(--color-card-content);
        opacity: 0.8;
    }
</style>