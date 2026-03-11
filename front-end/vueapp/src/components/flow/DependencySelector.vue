<template>
    <div v-if="
        previousStepTools &&
        previousStepTools.length > 0
    " class="mt-4">
        <h6 class="mb-0">
            {{ $t("flow.sidebar.dependencies") }}
            <span class="text-danger">*</span>
        </h6>
        <p class="text-muted small mb-2">
            {{ $t("flow.sidebar.dependenciesHint") }}
        </p>

        <!-- Dropdown to Add Dependencies -->
        <div class="dropdown">
            <button class="btn btn-outline-secondary btn-sm w-100 d-flex align-items-center justify-content-between"
                type="button" data-bs-toggle="dropdown" aria-expanded="false">
                <span>
                    {{
                        availableStepTools.length > 0
                            ? $t(
                                "flow.sidebar.addDependency"
                            )
                            : $t(
                                "flow.sidebar.noDependencies"
                            )
                    }}
                </span>
                <LucideIcon :icon="'CirclePlus'" :size="16" />
            </button>
            <ul class="dropdown-menu w-100">
                <li v-if="availableStepTools.length === 0" class="dropdown-item-text text-muted small">
                    {{
                        $t(
                            "flow.sidebar.allDependenciesSelected"
                        )
                    }}
                </li>
                <li v-for="step in availableStepTools" :key="step.id">
                    <div v-if="step.stepTools.length">
                        <span class="dropdown-divider"></span>
                        <h6>{{ step.name }}</h6>
                        <a v-for="stepTool in step.stepTools" :key="stepTool.id" class="dropdown-item" href="#"
                            @click.prevent="
                                addDependency(
                                    step,
                                    stepTool
                                )
                                ">
                            <div class="d-flex align-items-center">
                                <div>
                                    <div class="fw-medium">
                                        {{
                                            stepTool.tool
                                                .name
                                        }}
                                        <small class="text-muted">
                                            ({{
                                                stepTool
                                                    .tool
                                                    .toolType
                                            }})
                                        </small>
                                    </div>
                                </div>
                            </div>
                        </a>
                    </div>
                </li>
            </ul>
        </div>
        <!-- Selected Dependencies Display -->
        <div v-if="selectedDependencies.length > 0" class="mb-3 mt-2">
            <div v-for="(
item, index
                ) in selectedDependencies" :key="index"
                class="d-flex align-items-center justify-content-between dep-item rounded p-2 mb-2">
                <div class="d-flex align-items-center flex-grow-1">
                    <div class="flex-grow-1">
                        <div class="fw-medium">
                            {{
                                findStepNameByOrder(
                                    item.stepOrder
                                )
                            }}
                            <small class="text-muted">
                                ({{
                                    findToolLabelById(
                                        item.stepOrder,
                                        item.stepToolOrder
                                    )
                                }})
                            </small>
                        </div>
                    </div>
                </div>
                <button type="button" class="btn btn-sm btn-link text-danger p-0 ms-2" @click="removeDependency(item)"
                    :title="$t('flow.sidebar.deleteDependency')
                        ">
                    <LucideIcon :icon="'CircleX'" :size="16" />
                </button>
            </div>
        </div>
    </div>
</template>
<script>
import LucideIcon from "@/components/global/LucideIcon.vue";

export default {
    name: "DependencySelector",
    props: {
        previousStepTools: {
            type: Array,
            default: () => [],
        },
        modelValue: {
            type: Array,
            default: () => [],
        },
    },
    components: {
        LucideIcon,
    },
    data() {
        return {
            selectedDependencies: this.modelValue
                ? JSON.parse(
                    JSON.stringify(this.modelValue)
                )
                : [],
        };
    },
    watch: {
        modelValue: {
            handler(newValue) {
                this.selectedDependencies = newValue
                    ? JSON.parse(
                        JSON.stringify(newValue)
                    )
                    : [];
            },
            deep: true,
        },
    },
    computed: {
        availableStepTools() {
            return this.previousStepTools
                .map((step) => ({
                    ...step,
                    stepTools: step.stepTools.filter(
                        (stepTool) =>
                            !this.selectedDependencies.some(
                                (selected) =>
                                    selected.stepOrder ===
                                    step.order &&
                                    selected.stepToolOrder ===
                                    stepTool.order
                            )
                    ),
                }))
                .filter(
                    (step) => step.stepTools.length > 0
                );
        },
    },
    methods: {
        updateModel() {
            this.$emit(
                "update:modelValue",
                this.selectedDependencies
            );
        },
        addDependency(step, stepTool) {
            this.selectedDependencies.push({
                stepOrder: step.order,
                stepToolOrder: stepTool.order,
            });
            this.updateModel();
        },
        removeDependency(item) {
            this.selectedDependencies =
                this.selectedDependencies.filter(
                    (dependency) =>
                        dependency.stepToolOrder !==
                        item.stepToolOrder ||
                        dependency.stepOrder !==
                        item.stepOrder
                );
            this.updateModel();
        },
        reloadData() {
            this.selectedDependencies = this.modelValue
                ? JSON.parse(
                    JSON.stringify(this.modelValue)
                )
                : [];
        },
        findStepNameByOrder(order) {
            const step = this.previousStepTools.find(
                (s) => s.order === order
            );
            return step ? step.name : "";
        },
        findToolLabelById(stepOrder, stepToolOrder) {
            const step = this.previousStepTools.find(
                (s) => s.order === stepOrder
            );
            const stepTool = step
                ? step.stepTools.find(
                    (st) => st.order === stepToolOrder
                )
                : null;
            return stepTool
                ? `${stepTool.tool.name}/${stepTool.tool.toolType}`
                : "";
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
