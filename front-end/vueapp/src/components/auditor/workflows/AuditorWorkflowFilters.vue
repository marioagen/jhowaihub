<template>
    <div>
        <div class="input-group input-group-sm auditor-filter-sm mb-3">
            <span class="input-group-text border-end-0 py-1">
                <LucideIcon
                    icon="Search"
                    :size="14"
                />
            </span>
            <input
                type="text"
                class="form-control form-control-sm border-start-0 py-1"
                :placeholder="$t('auditor.workflows.filters.searchPlaceholder')"
                :aria-label="$t('auditor.workflows.filters.searchAria')"
                :value="filters.search"
                @input="onSearchInput($event.target.value)"
            />
            <span
                v-if="filters.search"
                class="input-group-text border-start-0 py-1 clear-search"
                role="button"
                tabindex="0"
                :aria-label="$t('auditor.workflows.filters.clearSearch')"
                @click="cleanInput"
                @keydown.enter="cleanInput"
            >
                <LucideIcon
                    icon="X"
                    :size="12"
                />
            </span>
        </div>
    </div>
</template>
<script>
    export default {
        name: "AuditorWorkflowFilters",
        emits: ["filter"],
        data() {
            return {
                filters: {
                    search: "",
                },
            };
        },
        methods: {
            onSearchInput(input) {
                this.filters.search = input;
                this.$emit("filter", this.filters);
            },
            cleanInput() {
                this.filters.search = "";
                this.$emit("filter", this.filters);
            },
        },
    };
</script>
<style scoped>
    .auditor-filter-sm {
        font-size: 0.75rem;
    }
    .auditor-filter-sm .form-control,
    .auditor-filter-sm .input-group-text {
        font-size: 0.75rem;
    }
    .clear-search {
        cursor: pointer;
        background: var(--bs-body-bg);
    }
    .clear-search:hover {
        background: var(--bs-secondary-bg);
    }
</style>
