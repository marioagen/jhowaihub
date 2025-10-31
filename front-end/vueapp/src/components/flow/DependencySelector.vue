<template>
    <div v-if="previousStepTools && previousStepTools.length > 0" class="mt-4">
        <h6>{{ $t("flow.sidebar.dependencies") }}</h6>
        <hr>
        <p class="text-muted small">{{ $t("flow.sidebar.dependenciesHint") }}</p>
        
        <!-- Selected Dependencies Display -->
        <div v-if="selectedItems.length > 0" class="mb-3">
            <div v-for="item in selectedItems" :key="item.id" class="d-flex align-items-center justify-content-between bg-light rounded p-2 mb-2">
                <div class="d-flex align-items-center flex-grow-1">
                    <span class="badge bg-secondary me-2">{{ item.step?.order ?? '-' }}</span>
                    <div class="flex-grow-1">
                        <div class="fw-medium">{{ item.name }}</div>
                        <small class="text-muted">{{ item.description }}</small>
                    </div>
                </div>
                <button 
                    type="button" 
                    class="btn btn-sm btn-link text-danger p-0 ms-2" 
                    @click="removeDependency(item.id)"
                    :title="$t('labelRemove')"
                >
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                        <line x1="18" y1="6" x2="6" y2="18"></line>
                        <line x1="6" y1="6" x2="18" y2="18"></line>
                    </svg>
                </button>
            </div>
        </div>
        
        <!-- Dropdown to Add Dependencies -->
        <div class="dropdown">
            <button 
                class="btn btn-outline-secondary btn-sm w-100 d-flex align-items-center justify-content-between" 
                type="button" 
                :id="'dropdown-' + _uid"
                data-bs-toggle="dropdown" 
                aria-expanded="false"
            >
                <span>{{ availableTools.length > 0 ? $t('flow.sidebar.addDependency') : $t('flow.sidebar.noDependencies') }}</span>
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <circle cx="12" cy="12" r="10"></circle>
                    <line x1="12" y1="8" x2="12" y2="16"></line>
                    <line x1="8" y1="12" x2="16" y2="12"></line>
                </svg>
            </button>
            <ul class="dropdown-menu w-100" :aria-labelledby="'dropdown-' + _uid">
                <li v-if="availableTools.length === 0" class="dropdown-item-text text-muted small">
                    {{ $t('flow.sidebar.allDependenciesSelected') }}
                </li>
                <li v-for="tool in availableTools" :key="tool.id">
                    <a class="dropdown-item" href="#" @click.prevent="addDependency(tool)">
                        <div class="d-flex align-items-center">
                            <span class="badge bg-secondary me-2">{{ tool.step?.order ?? '-' }}</span>
                            <div>
                                <div class="fw-medium">{{ tool.name }}</div>
                                <small class="text-muted">{{ tool.description }}</small>
                            </div>
                        </div>
                    </a>
                </li>
            </ul>
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
        }
    },
    computed: {
        selectedDependencies: {
            get() {
                return this.modelValue;
            },
            set(value) {
                this.$emit('update:modelValue', value);
            }
        },
        selectedItems() {
            // Get full tool objects for selected IDs
            return this.previousStepTools.filter(tool => 
                this.selectedDependencies.includes(tool.id)
            ).map(tool => ({
                ...tool,
                description: tool.step?.name ? `{{${tool.name}.embeddings}}` : ''
            }));
        },
        availableTools() {
            // Get tools that haven't been selected yet
            return this.previousStepTools.filter(tool => 
                !this.selectedDependencies.includes(tool.id)
            ).map(tool => ({
                ...tool,
                description: tool.step?.name ? `{{${tool.name}.embeddings}}` : ''
            }));
        }
    },
    methods: {
        addDependency(tool) {
            if (!this.selectedDependencies.includes(tool.id)) {
                this.selectedDependencies = [...this.selectedDependencies, tool.id];
            }
        },
        removeDependency(toolId) {
            this.selectedDependencies = this.selectedDependencies.filter(id => id !== toolId);
        }
    }
};
</script>

<style scoped>
.dropdown-menu {
    max-height: 300px;
    overflow-y: auto;
}
</style>
