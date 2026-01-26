<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="row align-items-center">
                <div class="col-auto">
                    <div class="row">
                        <div class="col-2">
                            <button
                                class="btn btn-outline-primary btn-table btn-sm table-btn"
                                @click="redirectToIndex"
                            >
                                <LucideIcon
                                    icon="ArrowLeft"
                                />
                            </button>
                        </div>
                        <div class="col-10">
                            <div>
                                <h5 class="mb-0 fw-bold">
                                    {{ $t("flow.title") }}
                                </h5>
                                <p>
                                    <small
                                        class="text-muted"
                                    >
                                        {{
                                            $t(
                                                "flow.subtitle"
                                            )
                                        }}
                                    </small>
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-auto ms-auto">
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
                :isEdit="isEdit"
                :stepId="stepId"
                :stepOrder="stepOrder"
                @openNodeConfig="openNodeConfig"
                ref="VueflowComponent"
                :hasStepTools="hasStepTools"
            />

            <div
                class="offcanvas offcanvas-end"
                tabindex="-1"
                id="offcanvasRight"
                aria-labelledby="offcanvasRightLabel"
                ref="sidebar"
            >
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
                <div class="offcanvas-body">
                    <div
                        class="cover"
                        v-if="
                            loadingWebhooks || loadingInputs
                        "
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
                        :previousStepTools="
                            previousStepTools
                        "
                        v-model="selectedDependencies"
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
                                {{
                                    $t(
                                        "flow.sidebar.filter"
                                    )
                                }}
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
                                v-if="
                                    field.type ===
                                        'string' ||
                                    field.type === 'integer'
                                "
                                :type="
                                    field.type === 'integer'
                                        ? 'number'
                                        : 'string'
                                "
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
                                    v-model="
                                        formData[field.name]
                                    "
                                />
                            </div>
                            <div
                                v-else-if="
                                    field.type === 'boolean'
                                "
                                class="form-check mb-3"
                                :disabled="
                                    loadingWebhooks ||
                                    loadingInputs
                                "
                            >
                                <input
                                    class="form-check-input"
                                    type="checkbox"
                                    :id="field.name"
                                    v-model="
                                        formData[field.name]
                                    "
                                    :disabled="
                                        loadingWebhooks ||
                                        loadingInputs
                                    "
                                />
                                <label
                                    class="form-check-label"
                                    for="flexCheckDefault"
                                >
                                    {{ field.label }}
                                </label>
                            </div>
                            <div
                                v-else-if="
                                    field.type === 'array'
                                "
                            >
                                <h6>{{ field.label }}</h6>
                                <div
                                    v-for="(
                                        item, index
                                    ) in formData[
                                        field.name
                                    ]"
                                    :key="index"
                                >
                                    <div
                                        class="mb-3"
                                        v-for="child in field.children"
                                        :key="child.name"
                                    >
                                        <label
                                            v-if="
                                                child.label
                                            "
                                            :for="
                                                child.name
                                            "
                                            class="form-label"
                                            :disabled="
                                                loadingWebhooks ||
                                                loadingInputs
                                            "
                                        >
                                            {{
                                                child.label
                                            }}
                                        </label>
                                        <label
                                            v-else
                                            :for="
                                                child.name
                                            "
                                            class="form-label text-capitalize"
                                        >
                                            {{ child.name }}
                                        </label>
                                        <input
                                            :id="child.name"
                                            v-model="
                                                formData[
                                                    field
                                                        .name
                                                ][index][
                                                    child
                                                        .name
                                                ]
                                            "
                                            :disabled="
                                                loadingWebhooks ||
                                                loadingInputs
                                            "
                                            class="form-control form-control-sm"
                                        />
                                    </div>
                                </div>
                            </div>
                            <div
                                v-else-if="
                                    field.type === 'text'
                                "
                            >
                                <label
                                    :for="field.name"
                                    class="form-label"
                                >
                                    {{ field.label }}
                                </label>
                                <textarea
                                    class="form-control form-control-sm text-long"
                                    :id="field.name"
                                    v-model="
                                        formData[field.name]
                                    "
                                    rows="4"
                                />
                            </div>
                        </div>

                        <div class="mt-4">
                            <button
                                type="button"
                                class="btn btn-primary"
                                @click="updateNodeWithForm"
                                :disabled="
                                    loadingWebhooks ||
                                    loadingInputs
                                "
                            >
                                {{ $t("common.save") }}
                            </button>
                        </div>
                    </div>
                    <div v-else-if="isPromptTool">
                        <h6>Prompts</h6>
                        <div class="background-div">
                            <select
                                class="form-select"
                                v-model="idSelected"
                                @change="onPromptSelect"
                            >
                                <option
                                    v-for="item in promptlist"
                                    :key="item.id"
                                    :value="item.id"
                                >
                                    {{ item.name }}
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
                        <h6>
                            {{ $t("flow.sidebar.inputs") }}
                        </h6>
                        <div
                            class="background-div"
                            v-for="(
                                param, index
                            ) in parameters"
                            :key="index"
                        >
                            <textarea
                                class="form-control"
                                id="exampleFormControlTextarea1"
                                rows="3"
                                v-model="
                                    parameters[index].value
                                "
                            ></textarea>
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
                </div>
            </div>
        </div>
    </main>
</template>
<script>
    import VueFlowComponent from "@/components/flow/VueFlowComponent.vue";
    import DependencySelector from "@/components/flow/DependencySelector.vue";
    import AutomationServices from "@/services/automation/AutomationServices";
    import PromptService from "@/services/prompts/PromptsService";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import LogService from "@/services/log/logService";
    import ToolType from "@/constants/ToolType";

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
                sidebar: null,
                loadingWebhooks: false,
                loadingInputs: false,
                valueInput: "",
                idSelected: 0,
                promptlist: [],
                toolType: "",
                previousStepTools: [],
                selectedDependencies: [],
                nodes: [],
            };
        },
        components: {
            VueFlowComponent,
            DependencySelector,
        },
        methods: {
            redirectToIndex() {
                if (this.workflowId) {
                    const routeName = this.isEdit
                        ? "EditWorkflow"
                        : "NewWorkflow";
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
                this.isActiveCollapse =
                    !this.isActiveCollapse;
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
                AutomationServices.getWorkflowWebhookInputs(
                    params
                )
                    .then((response) => {
                        if (response.error === undefined) {
                            this.formFields = response;
                            this.formData = [];
                            if (dataFromParameters) {
                                this.formData = JSON.parse(
                                    this.parameters[0].value
                                );
                            } else {
                                this.formData =
                                    this.transformToObject(
                                        response
                                    );
                            }
                        } else {
                            this.$notify({
                                title: "flow.title",
                                message:
                                    "flow.formFlow.connectorWorkflowFail",
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
                        if (
                            field.children &&
                            field.children.length > 0
                        ) {
                            result[field.name] = [
                                this.transformToObject(
                                    field.children
                                ),
                            ];
                        } else {
                            result[field.name] = [];
                        }
                    } else {
                        result[field.name] =
                            this.getDefaultValue(
                                field.type
                            );
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
                if (
                    this.parameters.length > 0 &&
                    this.parameters.value
                ) {
                    const data = JSON.parse(
                        this.parameters.value
                    );
                    this.fillValues(this.formFields, data);
                }
            },
            openNodeConfig(nodes, selectedNode) {
                this.nodes = nodes;
                this.nodeFlow = selectedNode;
                this.parameters =
                    selectedNode.data.parameters;
                this.toolType = selectedNode.data.toolType;

                this.loadPreviousStepTools(selectedNode);

                this.selectedDependencies =
                    selectedNode.data.dependencies;

                if (this.isTargetTool(ToolType.N8N)) {
                    this.loadingWebhooks = true;
                    this.resetFormConnector();
                    AutomationServices.getWorkflows(
                        selectedNode.data.toolId
                    )
                        .then((result) => {
                            if (
                                result.error === undefined
                            ) {
                                this.connectors = result;
                                this.parameters =
                                    selectedNode.data.parameters;
                                if (
                                    this.parameters
                                        .length === 0
                                ) {
                                    this.parameters.push({
                                        stepToolId: 0,
                                        value: null,
                                        requiredFile: false,
                                        webhookId: null,
                                    });
                                } else {
                                    this.connector =
                                        this.parameters[0].webhookId;
                                    this.getInputs(true);
                                }
                            } else {
                                this.$notify({
                                    title: "flow.title",
                                    message:
                                        "flow.formFlow.connectorWorkflowFail",
                                    variant: "danger",
                                    icon: "CircleX",
                                });
                            }
                        })
                        .finally(() => {
                            this.loadingWebhooks = false;
                        });
                } else if (
                    this.isTargetTool(ToolType.Prompt)
                ) {
                    this.findAllPrompts();
                    if (this.parameters.length === 0) {
                        this.idSelected = 0;
                        this.parameters.push({
                            stepToolId: 0,
                            value: null,
                            requiredFile: false,
                            webhookId: null,
                        });
                    } else {
                        this.idSelected = parseInt(
                            this.parameters[0]?.value
                        );
                    }
                } else if (this.parameters.length === 0) {
                    this.parameters.push({
                        stepToolId: 0,
                        value: null,
                        requiredFile: false,
                        webhookId: null,
                    });
                }
                this.$refs.dependencyTools.reloadData();
                this.sidebar = new bootstrap.Offcanvas(
                    this.$refs.sidebar
                );
                this.sidebar.show();
            },
            closeSidebar() {
                const sidebarEl = this.$refs.sidebar;
                const sidebar =
                    bootstrap.Offcanvas.getInstance(
                        sidebarEl
                    );
                if (sidebar) {
                    sidebar.hide();
                }
            },
            updateNode() {
                if (this.idSelected) {
                    this.parameters[0].value =
                        this.idSelected.toString();
                    const selectedPrompt = this.promptlist.find(p => p.id === this.idSelected);
                    if (selectedPrompt) {
                        this.nodeFlow.data.subtitle = selectedPrompt.name;
                    }
                }

                if (
                    !this.selectedDependencies ||
                    this.selectedDependencies.length === 0
                ) {
                    this.$notify({
                        title: "common.warning",
                        message:
                            "flow.formFlow.dependenciesRequired",
                        variant: "warning",
                        icon: "TriangleAlert",
                    });
                    return;
                }

                this.$refs.VueflowComponent.updateNodeInput(
                    this.nodeFlow.id,
                    this.parameters,
                    this.selectedDependencies
                );
                this.closeSidebar();
                this.showMessage();
            },
            showMessage() {
                try {
                    return this.$notify({
                        title: "flow.title",
                        message:
                            "flow.formFlow.editFlowNodeSuccess",
                        variant: "success",
                        icon: "CircleCheckBig",
                    });
                } catch (e) {
                    this.$notify({
                        title: "flow.title",
                        message:
                            "flow.formFlow.editFlowNodeFail",
                        variant: "danger",
                        icon: "CircleX",
                    });
                }
            },
            updateNodeWithForm() {
                this.parameters[0].requiredFile = false;
                if (
                    Object.prototype.hasOwnProperty.call(
                        this.formData,
                        "requiredFile"
                    )
                ) {
                    this.parameters[0].requiredFile =
                        this.formData["requiredFile"];
                }
                this.parameters[0].value = JSON.stringify(
                    this.formData
                );
                this.parameters[0].webhookId =
                    this.connector;

                this.$refs.VueflowComponent.updateNodeInput(
                    this.nodeFlow.id,
                    this.parameters,
                    this.selectedDependencies
                );
                this.closeSidebar();
                this.showMessage();
            },
            async save() {
                try {
                    let nodesList =
                        this.$refs.VueflowComponent.buildFlowPayload();
                    if (this.workflowId) {
                        const workflow =
                            await WorkflowService.getWorkflowById(
                                this.workflowId
                            );
                        if (workflow.error) {
                            throw new Error(
                                "Failed to load workflow data"
                            );
                        }
                        const allSteps = workflow.steps.map(
                            (step) => {
                                if (
                                    step.order ===
                                    this.stepOrder
                                ) {
                                    return {
                                        id: step.id || 0,
                                        order: step.order,
                                        stepTools:
                                            nodesList,
                                    };
                                }
                                return {
                                    id: step.id || 0,
                                    order: step.order,
                                    stepTools:
                                        step.stepTools ||
                                        [],
                                };
                            }
                        );

                        const params = {
                            workflowId: this.workflowId,
                            steps: allSteps,
                        };

                        const result =
                            await WorkflowService.updatePhase3(
                                params
                            );
                        if (result.error !== undefined) {
                            if (
                                result.error.response
                                    ?.data &&
                                result.error.response?.data
                                    ?.labelError
                            ) {
                                return this.$notify({
                                    title: "flow.title",
                                    message:
                                        result.error
                                            .response.data
                                            .labelError,
                                    variant: "danger",
                                    icon: "CircleX",
                                });
                            }
                            return this.$notify({
                                title: "flow.title",
                                message:
                                    "flow.formFlow.progressFlowUpdateFail",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                    }
                    this.redirectToIndex();
                    return this.$notify({
                        title: "flow.title",
                        message:
                            "flow.formFlow.progressFlowSuccess",
                        variant: "success",
                        icon: "CircleCheckBig",
                    });
                } catch (e) {
                    this.$notify({
                        title: "flow.title",
                        message:
                            e.message ||
                            "flow.formFlow.progressFlowFail",
                        variant: "danger",
                        icon: "CircleX",
                    });
                }
            },
            fillValues(fields, data) {
                fields.forEach((field) => {
                    if (
                        Object.prototype.hasOwnProperty.call(
                            data,
                            field.name
                        )
                    ) {
                        const value = data[field.name];

                        if (
                            field.type === "array" &&
                            Array.isArray(value)
                        ) {
                            field.value = value.map(
                                (item) => {
                                    const clonedChildren =
                                        field.children
                                            ? field.children.map(
                                                  (c) => ({
                                                      ...c,
                                                      value: null,
                                                      children:
                                                          c.children
                                                              ? [
                                                                    ...c.children,
                                                                ]
                                                              : [],
                                                  })
                                              )
                                            : [];

                                    this.fillValues(
                                        clonedChildren,
                                        item
                                    );
                                    return clonedChildren;
                                }
                            );
                        } else if (
                            field.children &&
                            field.children.length > 0 &&
                            typeof value === "object"
                        ) {
                            this.fillValues(
                                field.children,
                                value
                            );
                        } else {
                            field.value = value;
                        }
                    }
                });
            },
            findAllPrompts() {
                PromptService.getPrompts().then(
                    (response) => {
                        this.promptlist = response;
                    }
                );
            },
            async loadPreviousStepTools(node) {
                let workflowSteps = [];
                if (this.workflowId) {
                    try {
                        const workflow =
                            await WorkflowService.getWorkflowById(
                                this.workflowId
                            );
                        if (!workflow.error) {
                            workflowSteps =
                                workflow.steps || [];
                        }
                    } catch (error) {
                        LogService.showMessage(
                            "Error loading workflow steps: " +
                                error
                        );
                    }
                }

                if (workflowSteps.length === 0) {
                    workflowSteps =
                        this.$store.state.tempWorkflow
                            .list || [];
                }

                const relevantSteps = workflowSteps.filter(
                    (step) => step.order <= this.stepOrder
                );

                if (
                    !relevantSteps ||
                    relevantSteps.length === 0
                ) {
                    this.previousStepTools = [];
                    return;
                }

                const maxOrder = Math.max(
                    ...relevantSteps.map(
                        (step) => step.order
                    )
                );
                const nodesToolIds = this.nodes
                    .map((n) => n.data?.toolId)
                    .filter(Boolean);

                this.previousStepTools = relevantSteps.map(
                    (step) => ({
                        id: step.id,
                        name:
                            step?.name ||
                            step.name ||
                            "Unnamed Tool",
                        order: step.order,
                        stepTools: (
                            step.stepTools || []
                        ).filter(
                            (stepTool) =>
                                step.order < maxOrder ||
                                (step.order === maxOrder &&
                                    stepTool.order <
                                        node.data.order &&
                                    nodesToolIds.includes(
                                        stepTool.tool?.id
                                    ))
                        ),
                    })
                );
            },
            resetFormConnector() {
                this.connectors = [];
                this.parameters = [];
                this.formFields = [];
                this.formData = [];
                this.connector = "";
            },
            isTargetTool(targetToolType) {
                return (
                    this.toolType
                        ?.toLowerCase()
                        .includes(
                            targetToolType.toLowerCase()
                        ) || false
                );
            },
            onPromptSelect() {
                const selectedPrompt = this.promptlist.find(
                    (p) => p.id === this.idSelected
                );
                if (selectedPrompt) {
                    this.nodeFlow.data.subtitle =
                        selectedPrompt.name;
                }
            },
        },
        computed: {
            selectedItem() {
                if (this.idSelected != 0)
                    return this.promptlist.find(
                        (item) =>
                            item.id === this.idSelected
                    );
                return null;
            },
            isN8NTool() {
                return this.isTargetTool(ToolType.N8N);
            },
            isPromptTool() {
                return this.isTargetTool(ToolType.Prompt);
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
</style>
