<template>
    <div>
        <TableComponent
            modalName="management.teams.title"
            emptyMessage="management.teams.noTeamWasFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
            :pagination="table.pagination"
            :hasSelection="false"
            @change-page="changePage"
        >
            <template #cell-name="{ data }">
                <span v-if="data.row.name === 'Analyst'">
                    {{ $t("management.teams.analyst") }}
                </span>
                <span v-else>
                    {{ data.row.name }}
                </span>
            </template>
            <template #cell-members="{ data }">
                <LucideIcon icon="UsersRound" :size="15" />
                {{ data.row.users.length }}
            </template>
            <template #cell-actions="{ data }">
                <ActionTableListComponent v-slot="{ actionClass }">
                    <a :class="actionClass" @click="editTeam(data.row)" v-tooltip="$t('common.edit')">
                        <LucideIcon icon="SquarePen" />
                    </a>
                    <a :class="actionClass" class="text-danger"  @click="openConfirmation(data.row)" v-tooltip="$t('common.delete')">
                        <LucideIcon icon="Trash2" />
                    </a>
                </ActionTableListComponent>
            </template>
        </TableComponent>
    </div>
    <ConfirmModal
        id="deleteConfirm"
        title="management.teams.youAreAboutToDeleteTeam"
        message="common.thisActionCannotBeUndone"
        cancelText="common.cancel"
        confirmText="common.confirm"
        confirmVariant="primary"
        ref="DeleteDialog"
        :isLoading="isDeleting"
        @confirm="deleteTeam"
    />
</template>

<script>
    import date from "@/helpers/date";
    import TableComponent from "@/components/global/TableComponent.vue";
    import TeamsService from "@/services/teams/TeamsService";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import ActionTableListComponent from "@/components/global/ActionTableListComponent.vue";

    export default {
        name: "TeamsTable",
        components: {
            ActionTableListComponent,
            TableComponent,
            ConfirmModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "common.id" },
                    { key: "name", label: "management.teams.teamName" },
                    { key: "members", label: "management.teams.members" },
                    { key: "actions", label: "common.actions" },
                ],
                data: [],
                pagination: {
                    currentPage: 1,
                    totalPages: 0,
                    itemsPerPage: 10,
                    totalItems: 0,
                },
            },
            isDeleting: false,
            selectedTeam: {},
            queryPage: 1,
            searchInput: "",
            selectedOption: 10,
            isAscending: false,
            colType: 2,
            modalTeamShow: false,
            modalAlertShow: false,
        }),
        methods: {
            getTeams(obj) {
                this.table.isLoading = true;
                this.searching = false;
                this.dataDocument = [];
                this.listIds = [];
                var paramsReq = {
                    search: obj.search.trim() ? obj.search.trim() : "",
                    pageSize: this.selectedOption,
                    page: obj.page,
                    isAscending: this.isAscending,
                    colType: this.colType,
                };

                TeamsService.getTeams(paramsReq)
                    .then((response) => {
                        const content = response?.content || [];
                        const pagination = response?.pagination || {};

                        this.table.data = content;
                        this.table.pagination = pagination;
                    })
                    .finally(() => {
                        if (obj.type === "search") this.searching = true;
                        this.table.isLoading = false;
                    });
            },
            orderList: function (col) {
                if (this.isAscending) {
                    this.isAscending = false;
                } else {
                    this.isAscending = true;
                }
                this.colType = col;
                this.getTeams({ search: "", page: this.queryPage, type: null });
            },
            formatDate(str) {
                return date.formatDate(str);
            },
            editTeam(team) {
                this.$router.push({
                    name: "EditTeam",
                    params: {
                        id: team.id,
                    },
                });
            },
            openConfirmation(team) {
                this.selectedTeam = team;
                this.$refs.DeleteDialog.open();
            },
            deleteTeam() {
                this.isDeleting = true;
                let teamId = this.selectedTeam.id;
                TeamsService.deleteTeamById(teamId)
                    .then((response) => {
                        if (response.error === undefined) {
                            this.$refs.DeleteDialog.close();
                            this.getTeams({ search: "", page: 1, type: null });
                            return this.$notify({
                                title: "management.teams.title",
                                message: "management.teams.deleteSuccess",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else if (response.error.response.data.errorCode == 5) {
                            return this.$notify({
                                title: "management.teams.title",
                                message: "management.teams.deleteDocError",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }

                        this.$notify({
                            title: "management.teams.title",
                            message: "management.teams.errors.deleteError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    })
                    .finally(() => {
                        this.isDeleting = false;
                        this.listIds = [];
                    });
            },
            filterList(input) {
                this.searchInput = input;
                this.getTeams({ search: input, page: this.queryPage, type: null });
            },
            changePage(page) {
                this.getTeams({ search: "", page: page, type: null });
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getTeams({ search: "", page: this.queryPage, type: null });
        },
    };
</script>

<style>
    .dropdown-toggle::after {
        display: none;
    }
</style>
