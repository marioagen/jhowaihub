<template>
    <div v-if="previousStepTools && previousStepTools.length > 0" class="mt-4">
        <h6>{{ $t("flow.sidebar.dependencies") }}</h6>
        <hr>
        <p class="text-muted small">{{ $t("flow.sidebar.dependenciesHint") }}</p>
        <div v-for="prevTool in previousStepTools" :key="prevTool.id" class="form-check mb-2">
            <input 
                class="form-check-input" 
                type="checkbox" 
                :id="'dep-' + prevTool.id"
                :value="prevTool.id"
                v-model="selectedDependencies"
            />
            <label class="form-check-label d-flex align-items-center" :for="'dep-' + prevTool.id">
                <span class="badge bg-primary me-2">{{ prevTool.step?.order ?? '-' }}</span>
                <span>{{ prevTool.name }}</span>
                <span class="text-muted ms-2 small" v-if="prevTool.step?.name">({{ prevTool.step.name }})</span>
            </label>
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
        }
    }
};
</script>
