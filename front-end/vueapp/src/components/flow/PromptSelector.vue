<template>
    <main v-if="nodeData">
        <div class="container-fluid scroll-area mx-4 mt-4">
            <div class="row align-items-center">
                <div class="col-6">
                    <div class="row">
                        <div class="col-1">
                            <button
                                class="btn btn-outline-primary btn-table btn-sm table-btn"
                                type="button"
                                @click="backToFlow"
                            >
                                <LucideIcon icon="ArrowLeft" />
                            </button>
                        </div>
                        <div class="col-10">
                            <div>
                                <h5 class="mb-0 fw-bold">{{ nodeData.label }}</h5>
                                <small class="text-muted">
                                    {{ $t("flow.formFlow.configureToolParameters") }}
                                </small>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-auto ms-auto">
                    <button
                        class="btn btn-primary btn-sm"
                        @click="saveConfiguration"
                    >
                        <LucideIcon
                            icon="Save"
                            :size="15"
                        />
                        {{ $t("common.save") }}
                    </button>
                </div>
            </div>
            <div class="row mt-1">
                <div class="main-div shadow-sm">
                    <div class="mb-4">
                        <DependencySelector
                            :previousStepTools="previousStepTools"
                            v-model:selectedDependencies="selectedDependencies"
                            :allowedDependencyToolTypes="promptAllowedDependencyToolTypes"
                            dependenciesHintKey="flow.formFlow.promptDependenciesHint"
                        />
                    </div>
                    <div class="mb-4">
                        <h6 class="fw-bold mb-3">{{ $t("flow.formFlow.prompts") }}</h6>
                        <div class="mb-3">
                            <select
                                class="form-select"
                                v-model="selectedPromptId"
                            >
                                <option
                                    :value="null"
                                    disabled
                                >
                                    {{ $t("flow.formFlow.selectPrompt") }}
                                </option>
                                <option
                                    v-for="prompt in promptList"
                                    :key="prompt.id"
                                    :value="prompt.id"
                                >
                                    {{ prompt.name }}
                                    <span v-if="prompt.enableAccessToMcp">
                                        - [{{ $t("flow.formFlow.remoteAccess") }}]
                                    </span>
                                </option>
                            </select>
                        </div>
                        <button
                            v-if="!showCreateForm"
                            class="btn btn-outline-primary w-100 border-dashed"
                            @click="showCreateForm = true"
                        >
                            <LucideIcon
                                icon="Plus"
                                :size="16"
                                class="me-2"
                            />
                            {{ $t("flow.formFlow.createNewPrompt") }}
                        </button>
                    </div>
                    <div v-if="showCreateForm">
                        <PromptForm
                            :embedded="true"
                            @saved="onPromptSaved"
                            @cancelled="showCreateForm = false"
                        />
                    </div>
                </div>
            </div>
        </div>
    </main>
    <ConfirmModal
        id="confirm-leave-prompt-modal"
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
    import DependencySelector from "@/components/flow/DependencySelector.vue";
    import ToolType from "@/constants/ToolType";
    import PromptForm from "@/components/prompts/PromptForm.vue";
    import PromptService from "@/services/prompts/PromptsService";
    import LogService from "@/services/log/logService";
    import flowStateHelper from "@/helpers/flowStateHelper";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";

    export default {
        name: "PromptSelector",
        components: {
            DependencySelector,
            PromptForm,
            ConfirmModal,
        },
        data() {
            return {
                promptAllowedDependencyToolTypes: [ToolType.API, ToolType.Quiz, ToolType.Prompt, ToolType.Ocr],
                nodeData: null,
                previousStepTools: [],
                selectedDependencies: [],
                promptList: [],
                selectedPromptId: null,
                showCreateForm: false,
                flowState: null,
                pendingNavegation: null,
                leaveModalLoading: false,
                baselinePromptId: null,
                baselineDepsJson: "",
                baselineShowCreate: false,
            };
        },
        methods: {
            capturePromptBaseline() {
                this.baselinePromptId = this.selectedPromptId;
                this.baselineDepsJson = JSON.stringify(this.selectedDependencies || []);
                this.baselineShowCreate = this.showCreateForm;
            },
            hasPromptPageChanges() {
                if (this.showCreateForm !== this.baselineShowCreate) {
                    return true;
                }
                if (this.selectedPromptId !== this.baselinePromptId) {
                    return true;
                }
                return JSON.stringify(this.selectedDependencies || []) !== this.baselineDepsJson;
            },
            backToFlow() {
                if (this.hasPromptPageChanges()) {
                    this.checkNavigation(() => {
                        this.$router.go(-1);
                    });
                    return;
                }
                this.$router.go(-1);
            },
            saveConfiguration() {
                if (!this.flowState) return;

                if (!this.selectedPromptId) {
                    this.$notify({
                        title: "common.warning",
                        message: "flow.formFlow.promptRequired",
                        variant: "warning",
                        icon: "TriangleAlert",
                    });
                    return;
                }

                if (!this.selectedDependencies || this.selectedDependencies.length === 0) {
                    this.$notify({
                        title: "common.warning",
                        message: "flow.formFlow.dependenciesRequired",
                        variant: "warning",
                        icon: "TriangleAlert",
                    });
                    return;
                }
                const invalid = this.selectedDependencies.filter(
                    (d) => !this.dependencyExistsInStepTools(d)
                );
                if (invalid.length > 0) {
                    this.$notify({
                        title: "common.warning",
                        message: "flow.formFlow.dependenciesInvalidOrRemoved",
                        variant: "warning",
                        icon: "TriangleAlert",
                    });
                    return;
                }
                const selectedPrompt = this.promptList.find((p) => p.id === this.selectedPromptId);
                const subtitle = selectedPrompt?.name ?? null;
                const paramValue = this.selectedPromptId ? this.selectedPromptId.toString() : null;

                const success = flowStateHelper.commitNodeConfig(
                    this.nodeData.id,
                    paramValue,
                    subtitle,
                    this.selectedDependencies
                );

                if (!success) {
                    this.$notify({
                        title: "common.warning",
                        message: "flow.formFlow.configurationSaved",
                        variant: "warning",
                        icon: "TriangleAlert",
                    });
                    return;
                }
                this.$router.go(-1);
            },
            onPromptSaved(response) {
                this.loadPrompts().then(() => {
                    if (response && response.data && response.data.id) {
                        this.selectedPromptId = response.data.id;
                    } else if (response && response.id) {
                        this.selectedPromptId = response.id;
                    }
                    this.showCreateForm = false;
                });
            },
            async loadPrompts() {
                try {
                    const prompts = await PromptService.getPrompts();
                    this.promptList = prompts || [];
                } catch (error) {
                    LogService.showMessage("Erro ao carregar prompts");
                }
            },
            dependencyExistsInStepTools(dep) {
                if (!this.previousStepTools?.length) return false;
                const step = this.previousStepTools.find((s) => s.order === dep.stepOrder);
                if (!step?.stepTools?.length) return false;
                return step.stepTools.some((st) => st.order === dep.stepToolOrder);
            },
            loadState() {
                const stateStr = localStorage.getItem("flow_state_params");
                if (stateStr) {
                    const state = JSON.parse(stateStr);
                    this.flowState = state;
                    this.nodeData = state.selectedNode;
                    this.previousStepTools = state.previousStepTools || [];
                    this.selectedDependencies = state.selectedDependencies
                        ? JSON.parse(JSON.stringify(state.selectedDependencies))
                        : JSON.parse(JSON.stringify(this.nodeData.data.dependencies || []));
                    if (this.nodeData.data.parameters && this.nodeData.data.parameters.length > 0) {
                        const paramVal = this.nodeData.data.parameters[0].value;
                        this.selectedPromptId = paramVal ? parseInt(paramVal) : null;
                    }
                } else {
                    this.$router.push({ name: "Flow" });
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
            this.loadState();
            await this.loadPrompts();
            this.$nextTick(() => this.capturePromptBaseline());
        },
    };
</script>
<style scoped>
    .container-fluid {
        padding: 0 13px;
    }

    .main-div {
        border: 1px solid #d3d3d3;
        border-radius: 8px;
        background: white;
        padding: 20px 24px;
    }

    .icon-circle {
        width: 40px;
        height: 40px;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .color-acesso-externo {
        color: blue !important;
    }

    .border-dashed {
        border-style: dashed !important;
    }

    .icon-circle {
        width: 40px;
        height: 40px;
        display: flex;
        align-items: center;
        justify-content: center;
    }
</style>
