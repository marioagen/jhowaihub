<template>
    <div class="row">
        <div :class="`col-${findColSize('search')}`">
            <div class="input-group">
                <span
                    class="input-group-text border-end-0"
                >
                    <LucideIcon
                        icon="Search"
                        size="16"
                    />
                </span>
                <input
                    id="InputSearch"
                    type="text"
                    class="form-control form-control-sm border-start-0 custom-input"
                    :class="{
                        'border-end-0': showCleanBtn,
                    }"
                    v-model="filters.input"
                    @keydown.enter="filterData"
                    @keydown.delete="filterData"
                    :placeholder="$t('filters.workflowInput')"
                    ref="searchInpt"
                />
                <span
                    v-if="showCleanBtn"
                    class="input-group-text border-start-0"
                    @click="cleanInput"
                >
                    <LucideIcon
                        icon="X"
                        size="16"
                    />
                </span>
            </div>
        </div>
        <div :class="`col-${findColSize('orderBy')}`">
            <div class="input-group">
                <span
                    class="input-group-text border-end-0"
                >
                    <LucideIcon
                        icon="ArrowUpDown"
                        size="16"
                    />
                </span>
                <select
                    class="form-select form-select-sm border-start-0"
                    v-model="filters.orderBy"
                    @change="filterData"
                >
                    <option value="created desc">
                        {{ $t("filters.mostRecent") }}
                    </option>
                    <option value="created asc">
                        {{ $t("filters.mostOld") }}
                    </option>
                    <option value="name asc">
                        {{ $t("filters.nameAZ") }}
                    </option>
                    <option value="name desc">
                        {{ $t("filters.nameZA") }}
                    </option>
                </select>
            </div>
        </div>
        <div
            :class="`col-${findColSize('team')}`"
            v-if="hasTeams"
        >
            <div class="input-group">
                <span
                    class="input-group-text border-end-0"
                >
                    <LucideIcon
                        icon="Users"
                        size="16"
                    />
                </span>
                <Multiselect
                    v-model="filters.teamId"
                    :options="teamsListOptions"
                    valueProp="id"
                    label="name"
                    trackBy="name"
                    :searchable="true"
                    :placeholder="$t('filters.teamsSelect.all')"
                    mode="single"
                    :canClear="true"
                    :append-to-body="true"
                    @change="filterData"
                    class="border-start-0 workflow-filters-team-select"
                />
            </div>
        </div>
        <div
            :class="`col-${findColSize('user')}`"
            v-if="hasUsers"
        >
            <div class="input-group">
                <span
                    class="input-group-text border-end-0"
                >
                    <LucideIcon
                        icon="User"
                        size="16"
                    />
                </span>
                <select
                    class="form-select form-select-sm border-start-0"
                    v-model="filters.userId"
                    @change="filterData"
                >
                    <option value="">
                        {{ $t("filters.usersSelect.all") }}
                    </option>
                    <option
                        v-for="user in usersList"
                        :key="user.id"
                        :value="user.id"
                    >
                        {{ user.name }}
                    </option>
                </select>
            </div>
        </div>
    </div>
</template>

<script>
    import Multiselect from "@vueform/multiselect";
    export default {
        name: "WorkflowFilters",
        components: {
            Multiselect,
        },
        props: {
            teamsList: {
                type: Array,
                required: false,
                default: () => [],
            },
            usersList: {
                type: Array,
                required: false,
                default: () => [],
            },
        },
        data() {
            return {
                filters: {
                    orderBy: "name asc",
                    input: null,
                    isAllUsers: true,
                    login: this.$store.state.userProfile.login,
                    teamId: null,
                    userId: null,
                },
            };
        },
        methods: {
            filterData() {
                this.$emit("filter", this.filters);
            },
            filterUsers() {
                this.filters.isAllUsers = !this.filters.isAllUsers;
                this.filterData();
            },
            cleanInput() {
                this.filters.input = null;
                this.filterData();
            },
            findColSize(item) {
                switch (item) {
                    case "search":
                        return this.hasTeams || this.hasUsers ? "5" : "7";
                    case "orderBy":
                        return this.hasTeams || this.hasUsers ? "3" : "5";
                    case "team":
                        return this.hasTeams ? "2" : "0";
                    case "user":
                        return this.hasUsers ? "2" : "0";
                    default:
                        return "1";
                }
            },
        },
        computed: {
            showCleanBtn() {
                return this.filters.input !== null;
            },
            hasTeams() {
                return this.teamsList.length > 0;
            },
            hasUsers() {
                return this.usersList.length > 0;
            },
            teamsListOptions() {
                return [
                    {
                        id: "",
                        name: this.$t("filters.teamsSelect.all"),
                    },
                    ...this.teamsList,
                ];
            },
        },
    };
</script>

<style scoped>
    .custom-input {
        font-size: 12px;
    }

    .custom-input::placeholder {
        font-size: 12px;
        color: #999;
    }

    .workflow-filters-team-select {
        --ms-font-size: 0.875rem;
        --ms-option-font-size: 0.875rem;
        min-height: 31px;
        --ms-py: 1px;
        flex: 1 1 auto;
        width: auto;
        min-width: 0;
    }

    .workflow-filters-team-select :deep(.multiselect-wrapper) {
        min-width: 0;
        overflow: hidden;
    }

    .workflow-filters-team-select :deep(.multiselect-placeholder) {
        right: 0;
        min-width: 0;
        box-sizing: border-box;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
        justify-content: flex-start;
    }

    .workflow-filters-team-select :deep(.multiselect-single-label) {
        right: 0;
        min-width: 0;
    }
</style>
