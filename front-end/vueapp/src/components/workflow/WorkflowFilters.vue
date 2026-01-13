<template>

    <div class="row">
        <div :class="`col-${findColSize('search')}`">
            <div class="input-group">
                <span class="input-group-text border-end-0 bg-white">
                    <LucideIcon icon="Search" size="16" />
                </span>
                <input id="InputSearch" type="text" class="form-control form-control-sm border-start-0 custom-input"
                    :class="{ 'border-end-0': showCleanBtn }" v-model="filters.input" @keydown.enter="filterData"
                    @keydown.delete="filterData" :placeholder="$t('filters.workflowInput')" ref="searchInpt" />
                <span v-if="showCleanBtn" class="input-group-text border-start-0 bg-white" @click="cleanInput">
                    <LucideIcon icon="X" size="16" />
                </span>
            </div>
        </div>
        <div :class="`col-${findColSize('orderBy')}`">
            <div class="input-group">
                <span class="input-group-text border-end-0 bg-white">
                    <LucideIcon icon="ArrowUpDown" size="16" />
                </span>
                <select class="form-select form-select-sm border-start-0" v-model="filters.orderBy"
                    @change="filterData">
                    <option value="created desc">{{ $t("filters.mostRecent") }}</option>
                    <option value="created asc">{{ $t("filters.mostOld") }}</option>
                    <option value="name asc">{{ $t("filters.nameAZ") }}</option>
                    <option value="name desc">{{ $t("filters.nameZA") }}</option>
                </select>
            </div>
        </div>
        <div :class="`col-${findColSize('team')}`" v-if="hasTeams">
            <div class="input-group">
                <span class="input-group-text border-end-0 bg-white">
                    <LucideIcon icon="Users" size="16" />
                </span>
                <select class="form-select form-select-sm border-start-0" v-model="filters.teamId" @change="filterData">
                    <option value="">{{ $t("filters.teamsSelect.all") }}</option>
                    <option v-for="team in teamsList" :key="team.id" :value="team.id">{{ team.name }}</option>
                </select>
            </div>
        </div>
        <div :class="`col-${findColSize('user')}`" v-if="hasUsers">
            <div class="input-group">
                <span class="input-group-text border-end-0 bg-white">
                    <LucideIcon icon="User" size="16" />
                </span>
                <select class="form-select form-select-sm border-start-0" v-model="filters.userId" @change="filterData">
                    <option value="">{{ $t("filters.usersSelect.all") }}</option>
                    <option v-for="user in usersList" :key="user.id" :value="user.id">{{ user.name }}</option>
                </select>
            </div>
        </div>
    </div>
</template>

<script>
export default {
    name: "WorkflowFilters",
    props: {
        teamsList: {
            type: Array,
            required: false,
            default: () => []
        },
        usersList: {
            type: Array,
            required: false,
            default: () => []
        },
    },
    data() {
        return {
            filters: {
                orderBy: "created asc",
                input: null,
                isAllUsers: true,
                login: this.$store.state.userProfile.login,
                teamId: "",
                userId: ""
            },
        };
    },
    methods: {
        filterData() {
            this.$emit("filter", this.filters)
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
                    return this.hasTeams || this.hasUsers ? '5' : '7';
                case "orderBy":
                    return this.hasTeams || this.hasUsers ? '3' : '5';
                case "team":
                    return this.hasTeams ? '2' : '0';
                case "user":
                    return this.hasUsers ? '2' : '0';
                default:
                    return '1';
            }
        }
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
    },
};
</script>

<style scooped>
.custom-input {
    font-size: 12px;
}

.custom-input::placeholder {
    font-size: 12px;
    color: #999;
}
</style>
