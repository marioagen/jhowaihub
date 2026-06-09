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
                :placeholder="$t('auditor.users.filters.searchPlaceholder')"
                :aria-label="$t('auditor.users.filters.searchAria')"
                :value="filters.search"
                @input="onSearchInput($event.target.value)"
            />
            <span
                v-if="filters.search"
                class="input-group-text border-start-0 py-1 clear-search"
                role="button"
                tabindex="0"
                :aria-label="$t('auditor.users.filters.clearSearch')"
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
                    icon="UsersRound"
                    :size="12"
                    class="me-2"
                />
                {{
                    filters.teamId
                        ? teamList.find((opt) => opt.value === filters.teamId)?.label
                        : $t("auditor.users.filters.allTeams")
                }}
                <LucideIcon
                    icon="ChevronDown"
                    :size="12"
                    class="ms-1"
                />
            </button>
            <ul class="dropdown-menu dropdown-menu-start">
                <li
                    v-for="team in teamList"
                    :key="team.value"
                >
                    <a
                        class="dropdown-item"
                        href="#"
                        @click.prevent="onTeamSelect(team.value)"
                    >
                        {{ team.label }}
                    </a>
                </li>
            </ul>
        </div>
    </div>
</template>
<script>
    import TeamsService from "@/services/teams/TeamsService";

    export default {
        name: "AuditorUserFilters",
        emits: ["filter"],
        data() {
            return {
                teamList: [],
                filters: {
                    search: "",
                    teamId: "",
                },
            };
        },
        methods: {
            onSearchInput(input) {
                this.filters.search = input;
                this.$emit("filter", this.filters);
            },
            onTeamSelect(teamId) {
                this.filters.teamId = teamId;
                this.$emit("filter", this.filters);
            },
            cleanInput() {
                this.filters.search = "";
                this.$emit("filter", this.filters);
            },
            loadTeams() {
                TeamsService.getTeamListSimple()
                    .then((data) => {
                        if (data?.error || !Array.isArray(data)) return;
                        const options = data.map((t) => ({
                            value: String(t.id),
                            label: t.name || "",
                        }));
                        this.teamList = [
                            { value: "", label: this.$t("auditor.users.filters.allTeams") },
                            ...options,
                        ];
                    })
                    .catch(() => {});
            },
        },
        created() {
            this.teamList = [{ value: "", label: this.$t("auditor.users.filters.allTeams") }];
            this.loadTeams();
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
    .border {
        border: 1px solid var(--color-border-form-control) !important;
    }
</style>
