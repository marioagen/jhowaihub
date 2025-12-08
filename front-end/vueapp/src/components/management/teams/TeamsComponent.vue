<template>
    <div class="mt-3 mb-3">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <div>
                <h6 class="mb-0 fw-bold">{{ $t("labelTeams") }}</h6>
                <p>
                    <small class="text-muted">{{ $t("labelTeamsMessage") }}</small>
                </p>
            </div>
            <button 
                class="btn btn-primary btn-sm" 
                @click="redirectToForm"
            >
                <LucideIcon icon="Plus" />
                {{ $t("management.teams.createBtn") }}
            </button>
        </div>
        <div class="card mb-3">
            <div class="card-body">
                <SearchComponent :entity="entitySearch" :resetInput="resetInputSearch" @search="filterList" />
            </div>
        </div>
        <teams-table ref="TeamsTable" />
    </div>
</template>

<script>
    import paginationDivider from "@/utils/paginationDivider";
    import TeamsTable from "@/components/management/teams/TeamsTable.vue";
    import SearchComponent from "@/components/global/SearchComponent.vue";
    import editIcon from "@/assets/img/edit-outlined.svg";
    import deleteIcon from "@/assets/img/delete-outlined.svg";    

    export default {
        name: "TeamsManager",
        data() {
            return {
                menuActions: {},
                loading: false,
                searching: false,
                modalAlertShow: false,
                modalTeamShow: false,
                modalEntity: {},
                search: "",
                queryPage: this.$route.query.page ? this.$route.query.page : 1,
                pagination: { currentPage: 0, pageCount: 0, rowCount: 0, listPage: 0 },
                teams: [],
                divider: new paginationDivider(),
                listIds: [],
                teamEditing: {},
                entitySearch: {},
                resetInputSearch: "",
            };
        },
        watch: {
            searchInput: function (val) {
                this.searching = false;
            },
            "$store.state.userProfile.keyMongoAccess"(newValue) {
                if (newValue) {
                    this.$refs.TeamsTable.getTeams({ search: "", page: this.queryPage, type: null });
                }
            },
        },
        components: {
            TeamsTable,
            SearchComponent,
        },
        methods: {
            redirectToForm() {
                this.$router.push({
                    name: 'NewTeam',
                });
            },
            setMenuActions: function () {
                this.menuActions = {
                    options: [
                        { label: this.$t("labelEdit"), value: "edit", icon: editIcon },
                        {
                            label: this.$t("labelDelete"),
                            value: "delete",
                            icon: deleteIcon,
                            color: "text-danger",
                        },
                    ],
                };
            },
            handleMenuAction: function (option, item) {
                if (option.value === "edit") {
                    this.teamEditing = {
                        id: item.id,
                        name: item.name,
                        users: item.users,
                    };
                    this.openModalTeam();
                } else if (option.value === "delete") {
                    this.listIds = [item.id];
                    this.confirmationDialog(item);
                }
            },
            handleTeamCreated() {
                this.$refs.TeamsTable.getTeams({ search: "", page: this.queryPage, type: null });
                this.closeModalTeam();
            },
            filterList(obj) {
                this.$refs.TeamsTable.filterList(obj.search);
            },
            setEntitySearch: function () {
                this.entitySearch = {
                    screen: "team",
                    labelInput: this.$t("labelSearchTeams"),
                    placeholderInput: this.$t("labelTypeTeamName"),
                };
            },
        },
        created() {
            this.setMenuActions();
            this.setEntitySearch();
        },
    };
</script>

<style scoped>
    .show {
        display: block;
    }
    .table td,
    .table th {
        vertical-align: middle;
    }
</style>
