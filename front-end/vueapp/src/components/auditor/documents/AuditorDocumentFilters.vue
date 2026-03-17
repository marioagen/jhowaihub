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
                placeholder="ID, nome do documento ou esteira..."
                aria-label="Buscar Documento"
                :value="filters.search"
                @input="onSearchInput($event.target.value)"
            />
            <span
                v-if="filters.search"
                class="input-group-text border-start-0 py-1 clear-search"
                role="button"
                tabindex="0"
                aria-label="Limpar busca"
                @click="cleanInput"
                @keydown.enter="cleanInput"
            >
                <LucideIcon
                    icon="X"
                    :size="12"
                />
            </span>
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
                {{
                    filters.statusId
                        ? statusList.find((opt) => opt.value === filters.statusId)?.label
                        : "Todos os status"
                }}
                <LucideIcon
                    icon="ChevronDown"
                    :size="12"
                    class="ms-1"
                />
            </button>
            <ul class="dropdown-menu dropdown-menu-start">
                <li
                    v-for="opt in statusList"
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
    import LucideIcon from "@/components/global/LucideIcon.vue";

    export default {
        name: "AuditorDocumentFilters",
        components: {
            LucideIcon,
        },
        emits: ["filter"],
        data() {
            return {
                statusList: [
                    { value: "", label: "Todos os status" },
                    { value: "ativo", label: "Ativo" },
                    { value: "finalizado", label: "Finalizado" },
                ],
                filters: {
                    search: "",
                    statusId: "",
                },
            };
        },
        methods: {
            onSearchInput(input) {
                this.filters.search = input;
                this.$emit("filter", this.filters);
            },
            onStatusSelect(statusId) {
                this.filters.statusId = statusId;
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
