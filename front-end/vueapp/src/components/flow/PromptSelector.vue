<template>
    <main v-if="nodeData">
        <div class="container-fluid scroll-area mx-4 mt-4">
            <div class="row align-items-center">
                <div class="col-6">
                    <div class="row">
                        <div class="col-1">
                            <button class="btn btn-outline-primary btn-table btn-sm table-btn" @click="backToFlow">
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
                    <button class="btn btn-primary btn-sm" @click="saveConfiguration">
                        <LucideIcon icon="Save" :size="15" />
                        {{ $t("common.save") }}
                    </button>
                </div>
            </div>
            <div class="row mt-1">
                <div class="main-div shadow-sm">
                    <div class="mb-4">
                        <DependencySelector :previousStepTools="previousStepTools"
                            v-model:selectedDependencies="selectedDependencies" />
                    </div>
                    <div class="mb-4">
                        <h6 class="fw-bold mb-3">{{ $t("flow.formFlow.prompts") }}</h6>
                        <div class="mb-3">
                            <select class="form-select" v-model="selectedPromptId">
                                <option :value="null" disabled>
                                    {{ $t("flow.formFlow.selectPrompt") }}
                                </option>
                                <option v-for="prompt in promptList" :key="prompt.id" :value="prompt.id">
                                    {{ prompt.name }}
                                </option>
                            </select>
                        </div>
                        <button v-if="!showCreateForm" class="btn btn-outline-primary w-100 border-dashed"
                            @click="showCreateForm = true">
                            <LucideIcon icon="Plus" :size="16" class="me-2" />
                            {{ $t("flow.formFlow.createNewPrompt") }}
                        </button>
                    </div>
                    <div v-if="showCreateForm">
                        <PromptForm :embedded="true" @saved="onPromptSaved" @cancelled="showCreateForm = false" />
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>
<script>
import DependencySelector from "@/components/flow/DependencySelector.vue";
import PromptForm from "@/components/prompts/PromptForm.vue";
import PromptService from "@/services/prompts/PromptsService";
import LogService from "@/services/log/logService";
import flowStateHelper from "@/helpers/flowStateHelper";

export default {
    name: "PromptSelector",
    components: {
        DependencySelector,
        PromptForm,
    },
    data() {
        return {
            nodeData: null,
            previousStepTools: [],
            selectedDependencies: [],
            promptList: [],
            selectedPromptId: null,
            showCreateForm: false,
            flowState: null,
        };
    },
    methods: {
        backToFlow() {
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
            const selectedPrompt = this.promptList.find(
                (p) => p.id === this.selectedPromptId
            );
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

            this.$notify({
                title: "common.success",
                message: "flow.formFlow.configurationSaved",
                variant: "success",
                icon: "CircleCheckBig",
            });

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
    },
    mounted() {
        this.loadState();
        this.loadPrompts();
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
