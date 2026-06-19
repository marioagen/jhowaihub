<template>
    <div v-if="previousStepTools && previousStepTools.length > 0">
        <h6 class="mb-0">
            {{ $t("flow.sidebar.dependencies") }}
            <span class="text-danger">*</span>
        </h6>
        <p class="text-muted small mb-2">
            {{ dependenciesHintText }}
        </p>

        <!-- Dropdown to Add Dependencies -->
        <div class="dropdown">
            <button
                class="btn btn-outline-secondary btn-sm w-100 d-flex align-items-center justify-content-between"
                type="button"
                data-bs-toggle="dropdown"
                aria-expanded="false"
            >
                <span>
                    {{
                        availableStepTools.length > 0
                            ? $t("flow.sidebar.addDependency")
                            : $t("flow.sidebar.noDependencies")
                    }}
                </span>
                <LucideIcon
                    :icon="'CirclePlus'"
                    :size="16"
                />
            </button>
            <ul class="dropdown-menu w-100">
                <li
                    v-if="availableStepTools.length === 0"
                    class="dropdown-item-text text-muted small"
                >
                    {{ $t("flow.sidebar.allDependenciesSelected") }}
                </li>
                <li
                    v-for="step in availableStepTools"
                    :key="step.id"
                >
                    <div v-if="step.stepTools.length">
                        <span class="dropdown-divider"></span>
                        <h6>{{ step.name }}</h6>
                        <a
                            v-for="stepTool in step.stepTools"
                            :key="stepTool.id"
                            class="dropdown-item"
                            href="#"
                            @click.prevent="addDependency(step, stepTool)"
                        >
                            <div class="d-flex align-items-center">
                                <div>
                                    <div class="fw-medium">
                                        {{ formatDependencyToolLabel(stepTool) }}
                                    </div>
                                </div>
                            </div>
                        </a>
                    </div>
                </li>
            </ul>
        </div>

        <div
            v-if="internalDependencies.length > 0"
            class="mb-3 mt-2"
        >
            <div
                v-for="(item, index) in internalDependencies"
                :key="index"
                class="d-flex align-items-center justify-content-between rounded p-2 mb-2"
            >
                <div class="d-flex align-items-center flex-grow-1">
                    <div class="flex-grow-1">
                        <div class="fw-medium">
                            {{ findStepNameByOrder(item.stepOrder) }}
                            <small class="text-muted">
                                ({{ findToolLabelById(item.stepOrder, item.stepToolOrder) }})
                            </small>
                        </div>
                    </div>
                </div>
                <button
                    type="button"
                    class="btn btn-sm btn-link text-danger p-0 ms-2"
                    @click="removeDependency(item)"
                    :title="$t('flow.sidebar.deleteDependency')"
                >
                    <LucideIcon
                        :icon="'CircleX'"
                        :size="16"
                    />
                </button>
            </div>
        </div>
    </div>
</template>
<script>
    import LucideIcon from "@/components/global/LucideIcon.vue";
    import ToolType from "@/constants/ToolType";

    export default {
        name: "DependencySelector",
        props: {
            previousStepTools: {
                type: Array,
                default: () => [],
            },
            selectedDependencies: {
                type: Array,
                default: () => [],
            },
            allowedDependencyToolTypes: {
                type: Array,
                default: null,
            },
            dependenciesHintKey: {
                type: String,
                default: null,
            },
        },
        components: {
            LucideIcon,
        },
        data() {
            return {
                internalDependencies: this.selectedDependencies
                    ? JSON.parse(JSON.stringify(this.selectedDependencies))
                    : [],
            };
        },
        watch: {
            selectedDependencies: {
                handler(newValue) {
                    this.internalDependencies = newValue
                        ? JSON.parse(JSON.stringify(newValue))
                        : [];
                },
                deep: true,
            },
        },
        computed: {
            dependenciesHintText() {
                const key = this.dependenciesHintKey || "flow.sidebar.dependenciesHint";
                return this.$t(key);
            },
            availableStepTools() {
                return this.previousStepTools
                    .map((step) => ({
                        ...step,
                        stepTools: step.stepTools.filter((stepTool) => {
                            if (
                                this.internalDependencies.some(
                                    (selected) =>
                                        selected.stepOrder === step.order &&
                                        selected.stepToolOrder === stepTool.order
                                )
                            ) {
                                return false;
                            }
                            return this.isStepToolTypeAllowedForDependency(stepTool);
                        }),
                    }))
                    .filter((step) => step.stepTools.length > 0);
            },
        },
        methods: {
            isStepToolTypeAllowedForDependency(stepTool) {
                const allowed = this.allowedDependencyToolTypes;
                if (!Array.isArray(allowed) || allowed.length === 0) {
                    return true;
                }
                const tt = (stepTool?.tool?.toolType || "").toString();
                if (!tt) {
                    return false;
                }
                const lower = tt.toLowerCase();
                return allowed.some((a) => String(a).toLowerCase() === lower);
            },
            updateModel() {
                this.$emit("update:selectedDependencies", this.internalDependencies);
            },
            addDependency(step, stepTool) {
                this.internalDependencies.push({
                    stepOrder: step.order,
                    stepToolOrder: stepTool.order,
                });
                this.updateModel();
            },
            removeDependency(item) {
                this.internalDependencies = this.internalDependencies.filter(
                    (dependency) =>
                        dependency.stepToolOrder !== item.stepToolOrder ||
                        dependency.stepOrder !== item.stepOrder
                );
                this.updateModel();
            },
            reloadData() {
                this.internalDependencies = this.selectedDependencies
                    ? JSON.parse(JSON.stringify(this.selectedDependencies))
                    : [];
            },
            findStepNameByOrder(order) {
                const step = this.previousStepTools.find((s) => s.order === order);
                return step ? step.name : "";
            },
            resolveToolResourceLabel(stepTool) {
                if (!stepTool?.tool) return "";
                const fromTool = stepTool.tool.resourceName || "";
                if (fromTool) return fromTool;
                const ptt = (stepTool.tool.toolType || "").toString().toLowerCase();
                if (
                    ptt === ToolType.Prompt.toLowerCase() &&
                    stepTool.parameters?.length > 0 &&
                    stepTool.parameters[0].promptName
                ) {
                    return stepTool.parameters[0].promptName;
                }
                return "";
            },
            normalizeConnectorTypeForI18n(connectorType) {
                const raw = (connectorType || "").toString();
                if (!raw) return "";
                if (/^api$/i.test(raw)) return "Api";
                if (/^n8n$/i.test(raw)) return "N8N";
                return raw;
            },
            localizeConnectorTypeDisplay(connectorType) {
                const key = this.normalizeConnectorTypeForI18n(connectorType);
                if (!key) return "";
                const path = `connectors.typeDisplay.${key}`;
                if (this.$te(path)) return this.$t(path);
                return toolType || "";
            },
            formatDependencyToolLabel(stepTool) {
                if (!stepTool?.tool) return "";
                const tt = (stepTool.tool.toolType || "").toString();
                const ttLower = tt.toLowerCase();
                const name = (this.resolveToolResourceLabel(stepTool) || "").trim();
                if (name && ttLower === ToolType.Prompt.toLowerCase()) {
                    return this.$t("flow.dependencies.optionAgent", { name });
                }
                if (name && ttLower === ToolType.API.toLowerCase()) {
                    return this.$t("flow.dependencies.optionApi", { name });
                }
                if (name && ttLower === ToolType.Quiz.toLowerCase()) {
                    return this.$t("flow.dependencies.optionQuiz", { name });
                }
                const toolName = stepTool.tool.name || "";
                const typeLabel = this.localizeConnectorTypeDisplay(tt);
                if (toolName && typeLabel) {
                    return `${toolName} (${typeLabel})`;
                }
                return toolName || typeLabel || tt;
            },
            findToolLabelById(stepOrder, stepToolOrder) {
                const step = this.previousStepTools.find((s) => s.order === stepOrder);
                const stepTool = step
                    ? step.stepTools.find((st) => st.order === stepToolOrder)
                    : null;
                if (!stepTool) return "";
                return this.formatDependencyToolLabel(stepTool);
            },
        },
    };
</script>
<style scoped>
    .dropdown-menu {
        max-height: 300px;
        overflow-y: auto;
        background-color: var(--color-bg-dropdown-menu) !important;
        color: var(--color-dropdown-menu) !important;
    }

    .dropdown-item {
        color: var(--color-dropdown-menu) !important;
    }

    .dropdown-item:hover,
    .dropdown-item:focus {
        background-color: var(--color-bg-table-tr-first) !important;
        color: var(--color-body-content) !important;
    }

    .dep-item {
        background-color: var(--color-bg-table-tr-first) !important;
        color: var(--color-body-content) !important;
        border: 1px solid var(--color-border-form-control);
    }
</style>
