<template>
    <div v-if="previousStepTools && previousStepTools.length > 0" class="mt-4">
        <h6>{{ $t("flow.sidebar.dependencies") }}</h6>
        <hr>
        <p class="text-muted small">{{ $t("flow.sidebar.dependenciesHint") }}</p>
        
        <!-- Dropdown to Add Dependencies -->
        <div class="dropdown">
            <button 
                class="btn btn-outline-secondary btn-sm w-100 d-flex align-items-center justify-content-between" 
                type="button" 
                data-bs-toggle="dropdown" 
                aria-expanded="false"
            >
                <span>{{ availableStepTools.length > 0 ? $t('flow.sidebar.addDependency') : $t('flow.sidebar.noDependencies') }}</span>
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <circle cx="12" cy="12" r="10"></circle>
                    <line x1="12" y1="8" x2="12" y2="16"></line>
                    <line x1="8" y1="12" x2="16" y2="12"></line>
                </svg>
            </button>
            <ul class="dropdown-menu w-100">
                <li v-if="availableStepTools.length === 0" class="dropdown-item-text text-muted small">
                    {{ $t('flow.sidebar.allDependenciesSelected') }}
                </li>
                <li v-for="step in availableStepTools" :key="step.id">
                    <div v-if="step.stepTools.length" >
                        <span class="dropdown-divider"></span>
                        <h6>{{ step.name }}</h6>
                        <a v-for="stepTool in step.stepTools" :key="stepTool.id" class="dropdown-item" href="#" @click.prevent="addDependency(step, stepTool)">
                            <div class="d-flex align-items-center">
                                <div>
                                    <div class="fw-medium">{{ stepTool.tool.name }} <small class="text-muted">({{ stepTool.tool.toolType }})</small></div>                                
                                </div>
                            </div>
                        </a>
                    </div>
                </li>
            </ul>
        </div>
        <!-- Selected Dependencies Display -->
        <div v-if="selectedDependencies.length > 0" class="mb-3">
            <div v-for="(item, index) in selectedDependencies" :key="index" class="d-flex align-items-center justify-content-between bg-light rounded p-2 mb-2">
                <div class="d-flex align-items-center flex-grow-1">
                    <div class="flex-grow-1">
                        <div class="fw-medium">{{ item.step.name }} <small class="text-muted">({{ item.stepTool.tool.name }}/{{ item.stepTool.tool.toolType }})</small></div>
                    </div>
                </div>
                <button 
                    type="button" 
                    class="btn btn-sm btn-link text-danger p-0 ms-2" 
                    @click="removeDependency(item)"
                    :title="$t('labelRemove')"
                >
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <line x1="18" y1="6" x2="6" y2="18"></line>
                        <line x1="6" y1="6" x2="18" y2="18"></line>
                    </svg>
                </button>
            </div>
        </div>
    </div>
</template>

<script>

export default {
    name: 'DependencySelector',
    props: {
        previousStepTools: {
            type: Array,
            default: () => []
        },
        modelValue: {
            type: Array,
            default: () => []
        },
    },
    data() {
        return {
            // Create a deep copy to avoid sharing references between component instances
            selectedDependencies: this.modelValue ? JSON.parse(JSON.stringify(this.modelValue)) : []
        };
    },
    watch: {
        // Watch for changes in modelValue from parent and update local state
        modelValue: {
            handler(newValue) {
                // Create a deep copy to avoid shared references
                this.selectedDependencies = newValue ? JSON.parse(JSON.stringify(newValue)) : [];
            },
            deep: true
        }
    },
    computed: {
        selectedItems() {
            return this.previousStepTools.filter(tool => 
                this.selectedDependencies.includes(tool.id)
            );
        },
        availableStepTools() {
            return this.previousStepTools.map(step => ({
                ...step,
                stepTools: step.stepTools.filter(stepTool => 
                    !this.selectedDependencies.some(
                        selected => 
                            selected.step.order === step.order && 
                            selected.stepTool.id === stepTool.id
                    )
                )
            })).filter(step => step.stepTools.length > 0);
        }
    },
    methods: {
        updateModel() {
            this.$emit('update:modelValue', this.selectedDependencies);
        },
        addDependency(step, stepTool) {
            this.selectedDependencies.push({ step: step, stepTool: stepTool});
            this.updateModel();
        },
        removeDependency(item) {
            this.selectedDependencies = this.selectedDependencies
                .filter(dependency => dependency.stepTool.id !== item.stepTool.id || 
                                      dependency.step.order !== item.step.order);
            this.updateModel();
        },
        reloadData() {
            // Create a deep copy to avoid shared references
            this.selectedDependencies = this.modelValue ? JSON.parse(JSON.stringify(this.modelValue)) : [];
        }
    },
};
</script>

<style scoped>
.dropdown-menu {
    max-height: 300px;
    overflow-y: auto;
}
</style>
