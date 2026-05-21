<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="row align-items-center">
                <div class="col-10">
                    <div class="row">
                        <div class="col-auto">
                            <button
                                class="btn btn-outline-primary btn-table btn-sm"
                                @click="redirectToIndex"
                                type="button"
                            >
                                <LucideIcon icon="ArrowLeft" />
                            </button>
                        </div>
                        <div class="col-10">
                            <div>
                                <h5 class="mb-0 fw-bold">
                                    {{ $t("flow.title") }}
                                    <span v-if="step">- {{ step.name }}</span>
                                </h5>
                                <p>
                                    <small class="text-muted">
                                        {{ $t("flow.subtitle") }}
                                    </small>
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-2 ms-auto justify-content-end d-flex">
                    <button
                        class="btn btn-primary btn-sm me-2"
                        @click="save"
                    >
                        <LucideIcon
                            icon="Save"
                            :size="15"
                        />
                        {{ $t("common.save") }}
                    </button>
                </div>
            </div>
            <hr />
            <VueFlowComponent
                v-if="step !== null"
                :isEdit="isEdit"
                :stepId="stepId"
                :step="step"
                :stepOrder="stepOrder"
                :workflowId="workflowId"
                @openNodeConfig="openNodeConfig"
                @nodeDeleted="onNodeDeleted"
                @flowChanged="markFlowDirty"
                ref="VueflowComponent"
                :hasStepTools="hasStepTools"
            />
            <OffcanvasComponent
                ref="flowOffcanvas"
                id="offcanvasRight"
                label-id="offcanvasRightLabel"
                placement="end"
            >
                <template #header>
                    <div class="offcanvas-header">
                        <h5 id="offcanvasRightLabel">
                            {{ $t("flow.sidebarTitle") }}
                            {{ nodeFlow.label }}
                        </h5>
                        <button
                            type="button"
                            class="btn-close text-reset"
                            data-bs-dismiss="offcanvas"
                            aria-label="Close"
                            @click="closeSidebar"
                        ></button>
                    </div>
                </template>
                <div
                    class="cover"
                    v-if="loadingWebhooks || loadingInputs"
                >
                    <div class="spinner-cover">
                        <LucideIcon
                            icon="Loader"
                            :size="24"
                            class="me-1 animate-spin"
                        />
                    </div>
                </div>
                <DependencySelector
                    :previousStepTools="previousStepTools"
                    v-model:selectedDependencies="selectedDependencies"
                    ref="dependencyTools"
                />
                <hr />
                <div
                    v-if="isN8NTool"
                    class="mb-3"
                >
                    <select
                        class="form-select form-select-sm w-auto mb-3"
                        v-model="connector"
                        @change="changeWebhook"
                    >
                        <option
                            value=""
                            disabled
                        >
                            {{ $t("flow.sidebar.filter") }}
                        </option>
                        <option
                            v-for="connector in connectors"
                            :key="connector.id"
                            :value="connector.webhookId"
                        >
                            {{ connector.name }}
                        </option>
                    </select>
                    <div
                        v-for="field in formFields"
                        :key="field.name"
                    >
                        <div
                            class="mb-3"
                            v-if="field.type === 'string' || field.type === 'integer'"
                            :type="field.type === 'integer' ? 'number' : 'string'"
                        >
                            <label
                                :for="field.name"
                                class="form-label"
                            >
                                {{ field.label }}
                            </label>
                            <input
                                class="form-control form-control-sm"
                                :id="field.name"
                                v-model="formData[field.name]"
                            />
                        </div>
                        <div
                            v-else-if="field.type === 'boolean'"
                            class="form-check mb-3"
                            :disabled="loadingWebhooks || loadingInputs"
                        >
                            <input
                                class="form-check-input"
                                type="checkbox"
                                :id="field.name"
                                v-model="formData[field.name]"
                                :disabled="loadingWebhooks || loadingInputs"
                            />
                            <label
                                class="form-check-label"
                                for="flexCheckDefault"
                            >
                                {{ field.label }}
                            </label>
                        </div>
                        <div v-else-if="field.type === 'array'">
                            <h6>{{ field.label }}</h6>
                            <div
                                v-for="(item, index) in formData[field.name]"
                                :key="index"
                            >
                                <div
                                    class="mb-3"
                                    v-for="child in field.children"
                                    :key="child.name"
                                >
                                    <label
                                        v-if="child.label"
                                        :for="child.name"
                                        class="form-label"
                                        :disabled="loadingWebhooks || loadingInputs"
                                    >
                                        {{ child.label }}
                                    </label>
                                    <label
                                        v-else
                                        :for="child.name"
                                        class="form-label text-capitalize"
                                    >
                                        {{ child.name }}
                                    </label>
                                    <input
                                        :id="child.name"
                                        v-model="formData[field.name][index][child.name]"
                                        :disabled="loadingWebhooks || loadingInputs"
                                        class="form-control form-control-sm"
                                    />
                                </div>
                            </div>
                        </div>
                        <div v-else-if="field.type === 'text'">
                            <label
                                :for="field.name"
                                class="form-label"
                            >
                                {{ field.label }}
                            </label>
                            <textarea
                                class="form-control form-control-sm text-long"
                                :id="field.name"
                                v-model="formData[field.name]"
                                rows="4"
                            />
                        </div>
                    </div>

                    <div class="mt-4">
                        <button
                            type="button"
                            class="btn btn-primary"
                            @click="updateNodeWithForm"
                            :disabled="loadingWebhooks || loadingInputs"
                        >
                            {{ $t("common.save") }}
                        </button>
                    </div>
                </div>
                <div v-else-if="isQuizTool">
                    <h6>Quiz</h6>
                    <div class="background-div">
                        <select
                            class="form-select"
                            v-model="idSelected"
                            @change="onQuizSelect"
                        >
                            <option
                                v-for="item in quizlist"
                                :key="item.id"
                                :value="item.id"
                            >
                                {{ item.title }}
                            </option>
                        </select>
                    </div>

                    <div class="mt-4">
                        <button
                            type="button"
                            class="btn btn-primary"
                            @click="updateNode"
                        >
                            {{ $t("common.save") }}
                        </button>
                    </div>
                </div>
                <div
                    v-else
                    class="mb-3"
                >
                    <div v-if="!isEmbeddingTool">
                        <h6>
                            {{ $t("flow.sidebar.inputs") }}
                        </h6>
                        <div
                            class="background-div"
                            v-for="(param, index) in parameters"
                            :key="index"
                        >
                            <textarea
                                class="form-control"
                                id="exampleFormControlTextarea1"
                                rows="3"
                                v-model="parameters[index].value"
                            ></textarea>
                        </div>
                    </div>
                    <div class="mt-4">
                        <button
                            type="button"
                            class="btn btn-primary"
                            @click="updateNode"
                        >
                            {{ $t("common.save") }}
                        </button>
                    </div>
                </div>
            </OffcanvasComponent>
        </div>
        <ConfirmModalValidationInput
            id="removeToolValidationConfirm"
            title="workflow.removeToolValidationTitle"
            messageKey="workflow.removeToolValidationMessage"
            :messageParams="{ name: step?.name || '' }"
            cancelText="common.cancel"
            confirmText="workflow.confirmRemoveTool"
            :placeholder="$t('workflow.removeToolValidationPlaceholder', { name: step?.name })"
            :validationText="step?.name || ''"
            confirmVariant="danger"
            iconeName="AlertTriangle"
            iconVariant="warning"
            ref="RemoveToolValidationDialog"
            :isLoading="isLoading"
            @confirm="() => executeSave(true)"
        />
    </main>
    <ConfirmModal
        id="confirm-leave-flow-modal"
        :isLoading="leaveModalLoading"
        title="common.caution"
        message="workflow.leaveMessage"
        confirmText="common.confirm"
        cancelText="common.cancel"
        confirmVariant="primary"
        iconeName="AlertTriangle"
        iconVariant="warning"
        @confirm="confirmNavigation"
        @cancel="cancelNavigation"
        ref="confirmLeaveModal"
    />
</template>
<script>
    import VueFlowComponent from "@/components/flow/VueFlowComponent.vue";
    import DependencySelector from "@/components/flow/DependencySelector.vue";
    import AutomationServices from "@/services/automation/AutomationServices";
    import PromptService from "@/services/prompts/PromptsService";
    import QuizzesService from "@/services/quizzes/QuizzesService";
    import TemplateService from "@/services/template/TemplateService";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import LogService from "@/services/log/logService";
    import ToolType from "@/constants/ToolType";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import ConfirmModalValidationInput from "@/components/global/ConfirmModalValidationInput.vue";
    import OffcanvasComponent from "@/components/global/OffcanvasComponent.vue";

    export default {
        name: "FlowPage",
        components: {
            VueFlowComponent,
            DependencySelector,
            OffcanvasComponent,
            ConfirmModal,
            ConfirmModalValidationInput,
        },
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
            },
            phase: {
                type: Number,
                required: false,
                default: 0,
            },
            workflowId: {
                type: Number,
                required: false,
                default: null,
            },
            hasStepTools: {
                type: Boolean,
                required: false,
                default: false,
            },
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
                loadingWebhooks: false,
                loadingInputs: false,
                valueInput: "",
                idSelected: 0,
                promptlist: [],
                quizlist: [],
                toolType: "",
                previousStepTools: [],
                selectedDependencies: [],
                nodes: [],
                step: null,
                canLeave: true,
                pendingNavegation: null,
                leaveModalLoading: false,
                isLoading: false,
            };
        },
        methods: {
            parameterRowHasUserContent(p) {
                if (!p || typeof p !== "object") return false;
                if (p.requiredFile === true) return true;
                if (p.webhookId != null && p.webhookId !== "") return true;
                if (p.value != null && p.value !== "") return true;
                return false;
            },
            parametersHaveUserContent(params) {
                return (
                    Array.isArray(params) && params.some((p) => this.parameterRowHasUserContent(p))
                );
            },
            cloneParameterList(params) {
                if (!Array.isArray(params)) return [];
                return params.map((p) => ({ ...p }));
            },
            resolveParametersForNodeSave(nodeId, sidebarParams) {
                const list = Array.isArray(sidebarParams) ? sidebarParams : [];
                if (this.parametersHaveUserContent(list)) {
                    return this.cloneParameterList(list);
                }
                const vue = this.$refs.VueflowComponent;
                const node = vue?.nodes?.find((n) => String(n.id) === String(nodeId));
                const existing = node?.data?.parameters;
                if (this.parametersHaveUserContent(existing)) {
                    return this.cloneParameterList(existing);
                }
                return this.cloneParameterList(list);
            },
            markFlowDirty() {
                this.canLeave = false;
            },
            redirectToIndex() {
                if (!this.canLeave) {
                    this.checkNavigation(() => {
                        this.canLeave = true;
                        this.doRedirectToIndex();
                    });
                    return;
                }
                this.doRedirectToIndex();
            },
            doRedirectToIndex() {
                if (this.workflowId) {
                    const routeName = this.isEdit ? "EditWorkflow" : "NewWorkflow";
                    const params = this.isEdit
                        ? { id: this.workflowId, phase: 3 }
                        : {
                              phase: 3,
                              workflowId: this.workflowId,
                          };

                    return this.$router.push({
                        name: routeName,
                        params: params,
                    });
                }

                if (this.isEdit) {
                    return this.$router.push({
                        name: "EditWorkflow",
                        params: {
                            phase: this.phase,
                        },
                    });
                }
                return this.$router.push({
                    name: "NewWorkflow",
                    params: {
                        phase: this.phase,
                    },
                });
            },
            showCollapse() {
                this.isActiveCollapse = !this.isActiveCollapse;
            },
            changeWebhook() {
                this.getInputs(false);
            },
            getInputs(dataFromParameters) {
                this.loadingInputs = true;
                let params = {
                    toolId: this.nodeFlow.data.toolId,
                    workflowId: this.connector,
                };
                AutomationServices.getWorkflowWebhookInputs(params)
                    .then((response) => {
                        if (response.error === undefined) {
                            this.formFields = response;
                            this.formData = [];
                            if (dataFromParameters) {
                                this.formData = JSON.parse(this.parameters[0].value);
                            } else {
                                this.formData = this.transformToObject(response);
                            }
                        } else {
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
                fields.forEach((field) => {
                    if (field.type === "array") {
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
                    case "array":
                        return [];
                    default:
                        return null;
                }
            },
            fillFormFields() {
                if (this.parameters.length > 0 && this.parameters.value) {
                    const data = JSON.parse(this.parameters.value);
                    this.fillValues(this.formFields, data);
                }
            },
            async openNodeConfig(nodes, selectedNode) {
                this.nodes = nodes;
                this.nodeFlow = selectedNode;
                if (!Array.isArray(selectedNode.data.parameters)) {
                    selectedNode.data.parameters = [];
                }
                this.parameters = selectedNode.data.parameters;
                this.toolType = selectedNode.data.toolType;
                await this.loadPreviousStepTools(selectedNode);

                const rawDeps = selectedNode.data.dependencies || [];
                const validDeps = this.filterDependenciesToValidOnly(rawDeps);
                this.selectedDependencies = validDeps;

                if (this.isTargetTool(ToolType.API)) {
                    this.saveFlowStateLocal(selectedNode, nodes);
                    this.$router.push({
                        name: "TemplateConfiguration",
                    });
                    return;
                } else if (this.isTargetTool(ToolType.N8N)) {
                    this.loadingWebhooks = true;
                    this.resetFormConnector();
                    AutomationServices.getWorkflows(selectedNode.data.toolId)
                        .then((result) => {
                            if (result.error === undefined) {
                                this.connectors = result;
                                this.parameters = selectedNode.data.parameters;
                                if (this.parameters.length === 0) {
                                    this.parameters.push({
                                        stepToolId: 0,
                                        value: null,
                                        requiredFile: false,
                                        webhookId: null,
                                    });
                                } else {
                                    this.connector = this.parameters[0].webhookId;
                                    this.getInputs(true);
                                }
                            } else {
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
                } else if (this.isTargetTool(ToolType.Prompt)) {
                    this.saveFlowStateLocal(selectedNode, nodes);
                    this.$router.push({
                        name: "PromptSelector",
                    });
                    return;
                } else if (this.isTargetTool(ToolType.Quiz)) {
                    this.findAllQuizzes();
                    if (this.parameters.length === 0) {
                        this.idSelected = 0;
                        this.parameters.push({
                            stepToolId: 0,
                            value: null,
                            requiredFile: false,
                            webhookId: null,
                        });
                    } else {
                        this.idSelected = parseInt(this.parameters[0]?.value);
                    }
                } else if (this.parameters.length === 0 && !this.isEmbeddingTool) {
                    this.parameters.push({
                        stepToolId: 0,
                        value: null,
                        requiredFile: false,
                        webhookId: null,
                    });
                }
                this.$refs.dependencyTools.reloadData();
                this.$refs.flowOffcanvas?.open();
            },
            closeSidebar() {
                this.$refs.flowOffcanvas?.close();
            },
            saveFlowStateLocal(selectedNode, nodes) {
                const edges = this.$refs.VueflowComponent.edges;
                const orderedToolNodes =
                    this.$refs.VueflowComponent.getNodesOrderedByEdges?.() ??
                    nodes.filter((n) => n.id !== "start");
                const startNode = nodes.find((n) => n.id === "start");
                const nodesInFlowOrder = startNode
                    ? [startNode, ...orderedToolNodes]
                    : orderedToolNodes;
                const state = {
                    selectedNode: selectedNode,
                    previousStepTools: this.previousStepTools,
                    selectedDependencies: this.selectedDependencies,
                    nodes: nodesInFlowOrder,
                    edges: edges,
                    step: this.step,
                };
                localStorage.setItem("flow_state_params", JSON.stringify(state));
            },
            onNodeDeleted(nodeId) {
                if (!this.step?.stepTools?.length) return;
                const idStr = String(nodeId);
                const index = this.step.stepTools.findIndex((st) => st.id.toString() === idStr);
                if (index === -1) return;
                this.step.stepTools.splice(index, 1);
                this.step.stepTools.forEach((st, i) => {
                    st.order = i + 1;
                });
                this.markFlowDirty();
            },
            updateNode() {
                if (this.idSelected) {
                    this.parameters[0].value = this.idSelected.toString();
                    const selectedPrompt = this.promptlist.find((p) => p.id === this.idSelected);
                    if (selectedPrompt) {
                        this.nodeFlow.data.subtitle = selectedPrompt.name;
                    }
                }

                const depsToSave = this.filterDependenciesToValidOnly(
                    this.selectedDependencies || []
                );
                if (!depsToSave.length) {
                    this.$notify({
                        title: "common.warning",
                        message: "flow.formFlow.dependenciesRequired",
                        variant: "warning",
                        icon: "TriangleAlert",
                    });
                    return;
                }

                const parametersToSave = this.resolveParametersForNodeSave(
                    this.nodeFlow.id,
                    this.parameters
                );
                this.$refs.VueflowComponent.updateNodeInput(
                    this.nodeFlow.id,
                    parametersToSave,
                    depsToSave
                );
                this.markFlowDirty();
                this.closeSidebar();
                this.showMessage();
            },
            showMessage() {
                try {
                    return this.$notify({
                        title: "flow.title",
                        message: "flow.formFlow.editFlowNodeSuccess",
                        variant: "success",
                        icon: "CircleCheckBig",
                    });
                } catch (e) {
                    this.$notify({
                        title: "flow.title",
                        message: "flow.formFlow.editFlowNodeFail",
                        variant: "danger",
                        icon: "CircleX",
                    });
                }
            },
            updateNodeWithForm() {
                this.parameters[0].requiredFile = false;
                if (Object.prototype.hasOwnProperty.call(this.formData, "requiredFile")) {
                    this.parameters[0].requiredFile = this.formData["requiredFile"];
                }
                this.parameters[0].value = JSON.stringify(this.formData);
                this.parameters[0].webhookId = this.connector;

                const depsToSave = this.filterDependenciesToValidOnly(
                    this.selectedDependencies || []
                );
                const parametersToSave = this.resolveParametersForNodeSave(
                    this.nodeFlow.id,
                    this.parameters
                );
                this.$refs.VueflowComponent.updateNodeInput(
                    this.nodeFlow.id,
                    parametersToSave,
                    depsToSave
                );
                this.markFlowDirty();
                this.closeSidebar();
                this.showMessage();
            },
            async save() {
                this.isLoading = true;
                try {
                    if (!this.workflowId) {
                        await this.executeSave();
                        return;
                    }

                    const documentsCount = await WorkflowService.countDocuments(this.workflowId);
                    if (documentsCount > 0) {
                        this.$refs.RemoveToolValidationDialog.open();
                    } else {
                        await this.executeSave();
                    }
                } catch (error) {
                    this.$notify({
                        title: "flow.title",
                        message: "workflow.errors.fetchError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isLoading = false;
                }
            },
            async executeSave(resetDocuments = false) {
                this.$refs.RemoveToolValidationDialog?.close();
                this.isLoading = true;
                try {
                    let nodesList = this.$refs.VueflowComponent.buildFlowPayload();
                    if (this.workflowId) {
                        const workflow = await WorkflowService.getWorkflowById(this.workflowId);
                        if (workflow.error) {
                            throw new Error("Failed to load workflow data");
                        }
                        const allSteps = workflow.steps.map((step) => {
                            if (step.order === this.stepOrder) {
                                return {
                                    id: step.id || 0,
                                    order: step.order,
                                    stepTools: nodesList,
                                };
                            }
                            return {
                                id: step.id || 0,
                                order: step.order,
                                stepTools: step.stepTools || [],
                            };
                        });

                        const params = {
                            workflowId: this.workflowId,
                            steps: allSteps,
                            resetDocuments: resetDocuments,
                        };

                        const result = await WorkflowService.updatePhase3(params);
                        if (result.error !== undefined) {
                            if (
                                result.error.response?.data &&
                                result.error.response?.data?.labelError
                            ) {
                                return this.$notify({
                                    title: "flow.title",
                                    message: result.error.response.data.labelError,
                                    variant: "danger",
                                    icon: "CircleX",
                                });
                            }
                            return this.$notify({
                                title: "flow.title",
                                message: "flow.formFlow.progressFlowUpdateFail",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                    }
                    this.canLeave = true;
                    localStorage.removeItem("flow_state_params");
                    this.doRedirectToIndex();
                    return this.$notify({
                        title: "flow.title",
                        message: "flow.formFlow.progressFlowSuccess",
                        variant: "success",
                        icon: "CircleCheckBig",
                    });
                } catch (e) {
                    this.$notify({
                        title: "flow.title",
                        message: e.message || "flow.formFlow.progressFlowFail",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isLoading = false;
                }
            },
            fillValues(fields, data) {
                fields.forEach((field) => {
                    if (Object.prototype.hasOwnProperty.call(data, field.name)) {
                        const value = data[field.name];
                        if (field.type === "array" && Array.isArray(value)) {
                            field.value = value.map((item) => {
                                const clonedChildren = field.children
                                    ? field.children.map((c) => ({
                                          ...c,
                                          value: null,
                                          children: c.children ? [...c.children] : [],
                                      }))
                                    : [];

                                this.fillValues(clonedChildren, item);
                                return clonedChildren;
                            });
                        } else if (
                            field.children &&
                            field.children.length > 0 &&
                            typeof value === "object"
                        ) {
                            this.fillValues(field.children, value);
                        } else {
                            field.value = value;
                        }
                    }
                });
            },
            findAllPrompts() {
                PromptService.getPrompts().then((response) => {
                    this.promptlist = response;
                });
            },
            findAllQuizzes() {
                QuizzesService.getQuizzesList().then((response) => {
                    this.quizlist = response;
                });
            },
            priorStepsContainToolType(priorSteps, toolType) {
                return priorSteps.some((s) =>
                    (s.stepTools || []).some((st) => st.tool?.toolType === toolType)
                );
            },
            async loadPromptIdToNameMapForDependencyLabels() {
                try {
                    const prompts = await PromptService.getPrompts();
                    return new Map(prompts.map((p) => [String(p.id), p.name]));
                } catch (error) {
                    LogService.showMessage("Error loading prompts for dependency labels: " + error);
                    return new Map();
                }
            },
            async loadQuizIdToNameMapForDependencyLabels() {
                try {
                    const quizzes = await QuizzesService.getQuizzesList();
                    if (quizzes?.error || !Array.isArray(quizzes)) return new Map();
                    return new Map(quizzes.map((q) => [String(q.id), q.title || q.name || ""]));
                } catch (error) {
                    LogService.showMessage("Error loading quizzes for dependency labels: " + error);
                    return new Map();
                }
            },
            async loadApiTemplateIdToNameMapForDependencyLabels() {
                try {
                    const templates = await TemplateService.getAllTemplates();
                    if (templates?.error || !Array.isArray(templates)) return new Map();
                    return new Map(templates.map((t) => [String(t.id), t.name || ""]));
                } catch (error) {
                    LogService.showMessage(
                        "Error loading API templates for dependency labels: " + error
                    );
                    return new Map();
                }
            },
            async fetchPriorStepDependencyLabelMaps(priorSteps) {
                const maps = {
                    promptIdToName: new Map(),
                    quizIdToName: new Map(),
                    templateIdToName: new Map(),
                };
                if (this.priorStepsContainToolType(priorSteps, ToolType.Prompt)) {
                    maps.promptIdToName = await this.loadPromptIdToNameMapForDependencyLabels();
                }
                if (this.priorStepsContainToolType(priorSteps, ToolType.Quiz)) {
                    maps.quizIdToName = await this.loadQuizIdToNameMapForDependencyLabels();
                }
                if (this.priorStepsContainToolType(priorSteps, ToolType.API)) {
                    maps.templateIdToName =
                        await this.loadApiTemplateIdToNameMapForDependencyLabels();
                }
                return maps;
            },
            getBaseResourceNameForPriorStepTool(st) {
                const promptName = st.parameters?.[0]?.promptName;
                return st.tool?.resourceName || promptName || "";
            },
            resolvePromptResourceNameForPriorStepTool(st, resourceName, promptIdToName) {
                if (resourceName) return resourceName;
                if (st.tool?.toolType !== ToolType.Prompt) return resourceName;
                const val = st.parameters?.[0]?.value;
                if (val == null || val === "") return resourceName;
                return promptIdToName.get(String(val)) || "";
            },
            resolveQuizResourceNameForPriorStepTool(st, resourceName, quizIdToName) {
                if (resourceName) return resourceName;
                if (st.tool?.toolType !== ToolType.Quiz) return resourceName;
                const val = st.parameters?.[0]?.value;
                if (val == null || val === "") return resourceName;
                return quizIdToName.get(String(val)) || "";
            },
            resolveApiResourceNameForPriorStepTool(st, resourceName, templateIdToName) {
                if (resourceName) return resourceName;
                if (st.tool?.toolType !== ToolType.API) return resourceName;
                const raw = st.parameters?.[0]?.value;
                if (raw == null || raw === "") return resourceName;
                try {
                    const cfg = typeof raw === "string" ? JSON.parse(raw) : raw;
                    const tid = cfg?.templateId;
                    if (tid == null || tid === "") return resourceName;
                    return templateIdToName.get(String(tid)) || "";
                } catch {
                    return resourceName;
                }
            },
            enrichPriorStepToolWithDependencyLabels(st, labelMaps) {
                let resourceName = this.getBaseResourceNameForPriorStepTool(st);
                resourceName = this.resolvePromptResourceNameForPriorStepTool(
                    st,
                    resourceName,
                    labelMaps.promptIdToName
                );
                resourceName = this.resolveQuizResourceNameForPriorStepTool(
                    st,
                    resourceName,
                    labelMaps.quizIdToName
                );
                resourceName = this.resolveApiResourceNameForPriorStepTool(
                    st,
                    resourceName,
                    labelMaps.templateIdToName
                );
                return {
                    ...st,
                    tool: {
                        ...(st.tool || {}),
                        resourceName,
                    },
                };
            },
            mapPriorStepToolsWithDependencyLabels(stepTools, labelMaps) {
                return (stepTools || []).map((st) =>
                    this.enrichPriorStepToolWithDependencyLabels(st, labelMaps)
                );
            },
            buildStepToolsFromFlowNodesBeforeOrder(node) {
                return this.nodes
                    .filter(
                        (n) =>
                            n.id !== "start" &&
                            n.data?.order != null &&
                            n.data.order < node.data.order
                    )
                    .map((n) => ({
                        order: n.data.order,
                        tool: {
                            id: n.data.toolId,
                            name: n.label,
                            toolType: n.data.toolType || "",
                            resourceName: n.data.subtitle || "",
                        },
                    }));
            },
            buildPreviousStepToolsPayload(relevantSteps, maxOrder, node, labelMaps) {
                const mapPrior = (stepTools) =>
                    this.mapPriorStepToolsWithDependencyLabels(stepTools, labelMaps);
                return relevantSteps.map((step) => {
                    if (step.order < maxOrder) {
                        return {
                            id: step.id,
                            name: step?.name || step.name || "Unnamed Tool",
                            order: step.order,
                            stepTools: mapPrior(step.stepTools),
                        };
                    }
                    return {
                        id: step.id,
                        name: step?.name || step.name || "Unnamed Tool",
                        order: step.order,
                        stepTools: this.buildStepToolsFromFlowNodesBeforeOrder(node),
                    };
                });
            },
            async loadWorkflowStepsForPreviousTools() {
                let workflowSteps = [];
                if (this.workflowId) {
                    try {
                        const workflow = await WorkflowService.getWorkflowById(this.workflowId);
                        if (!workflow.error) {
                            workflowSteps = workflow.steps || [];
                        }
                    } catch (error) {
                        LogService.showMessage("Error loading workflow steps: " + error);
                    }
                }
                if (workflowSteps.length === 0) {
                    workflowSteps = this.$store.state.tempWorkflow.list || [];
                }
                return workflowSteps;
            },
            async loadPreviousStepTools(node) {
                const workflowSteps = await this.loadWorkflowStepsForPreviousTools();
                const relevantSteps = workflowSteps.filter((step) => step.order <= this.stepOrder);

                if (!relevantSteps?.length) {
                    this.previousStepTools = [];
                    return;
                }

                const maxOrder = Math.max(...relevantSteps.map((step) => step.order));
                const priorSteps = relevantSteps.filter((s) => s.order < maxOrder);
                const labelMaps = await this.fetchPriorStepDependencyLabelMaps(priorSteps);

                this.previousStepTools = this.buildPreviousStepToolsPayload(
                    relevantSteps,
                    maxOrder,
                    node,
                    labelMaps
                );
            },
            filterDependenciesToValidOnly(dependencies) {
                if (!dependencies?.length || !this.previousStepTools?.length)
                    return dependencies || [];
                return dependencies.filter((d) => {
                    const step = this.previousStepTools.find((s) => s.order === d.stepOrder);
                    if (!step || !step.stepTools?.length) return false;
                    return step.stepTools.some((st) => st.order === d.stepToolOrder);
                });
            },
            resetFormConnector() {
                this.connectors = [];
                this.parameters = [];
                this.formFields = [];
                this.formData = [];
                this.connector = "";
            },
            isTargetTool(targetToolType) {
                return this.toolType?.toLowerCase().includes(targetToolType.toLowerCase()) || false;
            },
            async fetchStepName() {
                if (this.workflowId) {
                    try {
                        if (this.stepId != 0) {
                            this.step = await WorkflowService.getStepById(this.stepId);
                        }
                    } catch (error) {
                        LogService.showMessage("Error fetching step name: " + error);
                    }
                }
            },
            onPromptSelect() {
                const selectedPrompt = this.promptlist.find((p) => p.id === this.idSelected);
                if (selectedPrompt) {
                    this.nodeFlow.data.subtitle = selectedPrompt.name;
                }
            },
            loadStorageFlowState() {
                const flowStateJson = localStorage.getItem("flow_state_params");
                if (!flowStateJson || !this.step) {
                    return;
                }

                const flowState = JSON.parse(flowStateJson);
                if (!flowState.nodes || !this.step.stepTools) {
                    return;
                }

                const toolNodes = flowState.nodes.filter((n) => n.id !== "start");
                const newStepTools = toolNodes.map((node, index) => {
                    const existing = this.step.stepTools.find((st) => st.id.toString() === node.id);
                    const order = index + 1;
                    if (existing) {
                        return {
                            ...existing,
                            parameters: node.data.parameters || [],
                            dependencies: node.data.dependencies || [],
                            positionX: node.position.x,
                            positionY: node.position.y,
                            order,
                        };
                    }
                    return {
                        id: parseInt(node.id) || 0,
                        positionX: node.position.x,
                        positionY: node.position.y,
                        toolId: node.data.toolId,
                        order,
                        parameters: node.data.parameters || [],
                        dependencies: node.data.dependencies || [],
                        tool: {
                            id: node.data.toolId,
                            name: node.label,
                            isEditableInput: node.data.isEditableInput,
                            toolType: node.data.toolType,
                        },
                    };
                });
                this.step.stepTools = newStepTools;

                this.$nextTick(() => {
                    if (this.$refs.VueflowComponent) {
                        this.$refs.VueflowComponent.reloadFlow();
                        this.canLeave = false;
                    }
                });

                localStorage.removeItem("flow_state_params");
            },
            onQuizSelect() {
                const selectedQuiz = this.quizlist.find((q) => q.id === this.idSelected);
                if (selectedQuiz) {
                    this.nodeFlow.data.subtitle = selectedQuiz.name;
                }
            },
            checkNavigation(next) {
                this.pendingNavegation = next;
                this.$refs.confirmLeaveModal.open();
            },
            confirmNavigation() {
                this.$refs.confirmLeaveModal.close();
                if (this.pendingNavegation) {
                    this.pendingNavegation();
                    this.pendingNavegation = null;
                }
            },
            cancelNavigation() {
                this.$refs.confirmLeaveModal.close();
                this.pendingNavegation = null;
            },
        },
        async mounted() {
            await this.fetchStepName();
            setTimeout(() => {
                this.loadStorageFlowState();
            }, 100);
        },
        computed: {
            selectedItem() {
                if (this.idSelected != 0)
                    return this.promptlist.find((item) => item.id === this.idSelected);
                return null;
            },
            isN8NTool() {
                return this.isTargetTool(ToolType.N8N);
            },
            isPromptTool() {
                return this.isTargetTool(ToolType.Prompt);
            },
            isQuizTool() {
                return this.isTargetTool(ToolType.Quiz);
            },
            isEmbeddingTool() {
                return this.isTargetTool(ToolType.Embeddings);
            },
        },
    };
</script>
<style>
    @import "@vue-flow/core/dist/style.css";
    @import "@vue-flow/core/dist/theme-default.css";

    .vue-flow-container {
        height: calc(100vh - 200px);
    }

    .background-div {
        background-color: var(--color-bg-body-content) !important;
        color: var(--color-body-content) !important;
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
        inset: calc(0.25rem * 0);
        align-items: center;
        display: flex;
        justify-content: center;
        z-index: 10;
        background-color: var(--color-card-content);
        opacity: 0.8;
    }

    .text-long {
        resize: none;
    }

    .offcanvas {
        background-color: var(--color-card-content) !important;
    }
</style>
