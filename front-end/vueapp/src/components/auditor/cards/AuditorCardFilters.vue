<template>
    <div>
        <div class="input-group input-group-sm auditor-filter-sm mb-3">
            <span class="input-group-text bg-white border-end-0 py-1">
                <LucideIcon
                    icon="Search"
                    :size="14"
                />
            </span>
            <input
                type="text"
                class="form-control form-control-sm border-start-0 py-1"
                placeholder="ID, nome do documento ou esteira..."
                aria-label="Buscar Documento"
                :value="filterParams.search"
                @input="onSearchInput($event.target.value)"
            />
        </div>
        <div class="dropdown mb-3">
            <button
                class="btn btn-light btn-sm w-100 text-start d-flex align-items-center justify-content-between border py-1 auditor-filter-sm"
                type="button"
                data-bs-toggle="dropdown"
                aria-expanded="false"
            >
                <LucideIcon
                    icon="Filter"
                    :size="12"
                    class="me-2"
                />
                {{ selectedStatusLabel }}
                <LucideIcon
                    icon="ChevronDown"
                    :size="12"
                    class="ms-1"
                />
            </button>
            <ul class="dropdown-menu dropdown-menu-start">
                <li
                    v-for="opt in statusFilterOptions"
                    :key="opt.value"
                >
                    <a
                        class="dropdown-item"
                        href="#"
                        @click.prevent="onStatusSelect(opt.value)"
                    >
                        {{ opt.label }}
                    </a>
                </li>
            </ul>
        </div>
    </div>
</template>
<script>
    export default {
        name: "AuditorCardFilters",
        props: {
            filterParams: {
                type: Object,
                default: () => ({ search: "", statusId: "" }),
            },
            statusFilterOptions: {
                type: Array,
                default: () => [],
            },
        },
        emits: ["update:filterParams"],
        computed: {
            selectedStatusLabel() {
                const opt = this.statusFilterOptions.find(
                    (o) => o.value === this.filterParams.statusId
                );
                return opt ? opt.label : (this.statusFilterOptions[0]?.label ?? "");
            },
        },
        methods: {
            onSearchInput(value) {
                this.$emit("update:filterParams", {
                    ...this.filterParams,
                    search: value,
                });
            },
            onStatusSelect(statusId) {
                this.$emit("update:filterParams", {
                    ...this.filterParams,
                    statusId,
                });
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
</style>
