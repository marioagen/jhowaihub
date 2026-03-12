<template>
    <div>
        <h6 class="small fw-semibold mb-2 d-flex align-items-center gap-1 auditor-user-heading">
            <LucideIcon
                icon="Search"
                :size="14"
            />
            Buscar Usuário
        </h6>
        <div class="input-group input-group-sm auditor-user-filter mb-3">
            <span class="input-group-text border-end-0 py-1">
                <LucideIcon
                    icon="Search"
                    :size="14"
                />
            </span>
            <input
                type="text"
                class="form-control form-control-sm border-start-0 py-1"
                placeholder="Nome do usuário..."
                aria-label="Buscar Usuário"
                :value="search"
                @input="$emit('update:search', $event.target.value)"
            />
        </div>
        <div class="dropdown mb-3">
            <button
                class="btn btn-light btn-sm w-100 text-start d-flex align-items-center justify-content-between border py-1 auditor-user-filter"
                type="button"
                data-bs-toggle="dropdown"
                aria-expanded="false"
            >
                <LucideIcon
                    icon="UsersRound"
                    :size="12"
                    class="me-2"
                />
                {{ selectedTeamLabel }}
                <LucideIcon
                    icon="ChevronDown"
                    :size="12"
                    class="ms-1"
                />
            </button>
            <ul class="dropdown-menu dropdown-menu-start">
                <li
                    v-for="opt in teamOptions"
                    :key="opt.value"
                >
                    <a
                        class="dropdown-item"
                        href="#"
                        @click.prevent="$emit('update:teamId', opt.value)"
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
        name: "AuditorUserFilters",
        props: {
            search: {
                type: String,
                default: "",
            },
            teamId: {
                type: String,
                default: "",
            },
            teamOptions: {
                type: Array,
                default: () => [],
            },
        },
        emits: ["update:search", "update:teamId"],
        computed: {
            selectedTeamLabel() {
                const opt = this.teamOptions.find((o) => o.value === this.teamId);
                return opt ? opt.label : (this.teamOptions[0]?.label ?? "Todos os times");
            },
        },
    };
</script>
<style scoped>
    .auditor-user-heading {
        color: var(--bs-body-color);
    }
    .auditor-user-filter {
        font-size: 0.75rem;
    }
    .auditor-user-filter .form-control,
    .auditor-user-filter .input-group-text {
        font-size: 0.75rem;
    }
</style>
