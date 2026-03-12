<template>
    <div>
        <h6 class="small fw-semibold text-muted mb-2 d-flex align-items-center gap-1">
            <LucideIcon
                icon="Search"
                :size="14"
            />
            Buscar Esteira
        </h6>
        <div class="input-group input-group-sm auditor-workflow-filter mb-3">
            <span class="input-group-text border-end-0 py-1">
                <LucideIcon
                    icon="Search"
                    :size="14"
                />
            </span>
            <input
                type="text"
                class="form-control form-control-sm border-start-0 py-1"
                placeholder="Nome da esteira ou time..."
                aria-label="Buscar Esteira"
                :value="search"
                @input="$emit('update:search', $event.target.value)"
            />
        </div>
    </div>
</template>
<script>
    export default {
        name: "AuditorWorkflowFilters",
        props: {
            search: {
                type: String,
                default: "",
            },
        },
        emits: ["update:search"],
    };
</script>
<style scoped>
    .auditor-workflow-filter {
        font-size: 0.75rem;
    }
    .auditor-workflow-filter .form-control,
    .auditor-workflow-filter .input-group-text {
        font-size: 0.75rem;
    }
</style>
