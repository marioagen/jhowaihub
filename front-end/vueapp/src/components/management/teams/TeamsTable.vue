<template>
    <div>
        <TableComponent
            modalName="labelTeams"
            emptyMessage="labelNoTeamWasFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
            :pagination="table.pagination"
            :hasSelection="false"
            @change-page="changePage"
        >
            <template #cell-members="{ data }">
                <LucideIcon icon="UsersRound" :size="15" />
                {{ data.row.users.length }}
            </template>
            <template #cell-actions="{ data }">
                <DropdownComponent>
                    <li>
                        <a class="dropdown-item d-flex align-items-center gap-2" @click="editTeam(data.row)">
                            <LucideIcon icon="SquarePen" />
                            {{ $t("labelEdit") }}
                        </a>
                    </li>
                    <li>
                        <a
                            class="dropdown-item d-flex align-items-center gap-2"
                            @click="openConfirmation(data.row)"
                        >
                            <LucideIcon icon="Trash2" />
                            {{ $t("labelDelete") }}
                        </a>
                    </li>
                </DropdownComponent>
            </template>
        </TableComponent>
    </div>
    <modal-team
        v-if="modalTeamShow"
        :teamEditing="selectedTeam"
        @teamCreated="handleTeamCreated"
        @close="closeModalTeam"
    />

    <ConfirmModal
        id="deleteConfirm"
        title="labelYouAreAboutToDeleteTeam"
        message="labelThisActionCannotBeUndone"
        cancelText="labelCancel"
        confirmText="labelConfirm"
        confirmVariant="primary"
        ref="DeleteDialog"
        :isLoading="isDeleting"
        @confirm="deleteTeam"
    />
</template>

<script>
    import date from "@/helpers/date";
    import TableComponent from "@/components/global/TableComponent.vue";
    import ModalTeam from "@/components/management/teams/modals/TeamModal.vue";
    import TeamsService from "@/services/teams/TeamsService";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import DropdownComponent from "@/components/global/DropdownComponent.vue";

    export default {
        name: "TeamsTable",
        components: {
            DropdownComponent,
            TableComponent,
            ModalTeam,
            ConfirmModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "id" },
                    { key: "name", label: "labelTeamName" },
                    { key: "members", label: "labelMembers" },
                    { key: "actions", label: "labelAction" },
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
                this.selectedTeam = team;
                this.openModalTeam();
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
                                title: 'team.title',
                                message: 'team.deleteSuccess',
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                        }
                        else if (response.error.response.data.errorCode == 5) {
                             return this.$notify({
                                title: 'team.title',
                                message: 'team.deleteDocError',
                                variant: 'danger',
                                icon: 'CircleX',
                            });
                        }

                        this.$notify({
                            title: 'team.title',
                            message: 'team.deleteError',
                            variant: 'danger',
                            icon: 'CircleX',
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
            handleTeamCreated: function () {
                this.getTeams({ search: "", page: this.queryPage, type: null });
                this.closeModalTeam();
            },
            openModalTeam: function () {
                this.modalTeamShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModalTeam: function () {
                this.modalTeamShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
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
