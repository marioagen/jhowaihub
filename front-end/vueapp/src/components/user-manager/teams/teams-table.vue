<template>
    <div>
        <table-component
            modalName="labelUsers"
            emptyMessage="labelNoDocumentTypeWasFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
            :pagination="table.pagination"
        >
            <template #cell-members="{ data }">
                {{ data.row.users.length }}
            </template>
            <template #cell-actions="{ data }">
                <button
                    class="btn btn-outline-success btn-sm"
                    @click="editTeam(data.row)"
                >
                    Edit
                </button>
                <button
                    class="btn btn-outline-danger btn-sm ms-2"
                    @click="confirmationDialog(data.row)"
                >
                    Delete
                </button>
            </template>
        </table-component>
    </div>
    <modal-team 
        v-if="modalTeamShow" 
        :teamEditing="selectedTeam"
        @teamCreated="handleTeamCreated"
        @close="closeModalTeam" 
    />
    <modal-alert 
        v-if="modalAlertShow" 
        :type="'Confirm'" 
        :entity="selectedTeam" 
        :alertTitle="$t('labelYouAreAboutToDeleteTeam')" 
        :alertMessage="$t('labelThisActionCannotBeUndone')" 
        :okLabel="$t('labelConfirm')" 
        :cancelLabel="$t('labelCancel')" 
        @open="deleteTeam"
        @close="closeModal"
    />
</template>

<script>
    import api from "@/services/api";
    import dates from "@/helpers/Dates";
    import TableComponent from "@/components/global/table-component.vue";
    import ModalTeam from '@/components/user-manager/teams/modals/new-team.vue';
    import ModalAlert from '@/components/common/modal-alert';

    import PaginationDivider from "@/utils/paginationDivider";
    const divider = new PaginationDivider();

    export default {
        name: "TeamsTable",
        components: {
            TableComponent,
            ModalTeam,
            ModalAlert
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "Id" },
                    { key: "name", label: "labelTeamName" },
                    { key: "members", label: "labelMembers" },
                    { key: "actions", label: "labelAction" },
                ],
                data: [],
                pagination: {
                    currentPage: 1,
                    totalPages: 100,
                    itemsPerPage: 10,
                    totalItems: 2000,
                },
            },
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
            getTeams: function (obj) {
                this.table.isLoading = true;
                this.searching = false;
                this.dataDocument = [];
                this.listIds = [];
                var paramsReq = {
                    search: obj.search.trim() ?obj.search.trim() : '',
                    pageSize: this.selectedOption,
                    page: obj.page,
                    isAscending: this.isAscending,
                    colType: this.colType,
                }

                api.get('/Team/Paged', { params: paramsReq })
                    .then(({ data }) => {
                        this.table.data = data.content;
                        // this.table.pagination = {
                        //     currentPage: data.currentPage,
                        //     totalPages: data.pageCount,
                        //     rowCount: data.rowCount,
                        //     totalItems: divider.calculatePageCount(data.pageCount, data.currentPage)
                        // };
                        if (obj.type === "search") this.searching = true;
                    }).catch((e) => {
                        console.log(e);
                        if (obj.type === "search") this.searching = true;
                    }).finally(() => {
                        this.table.isLoading = false;
                    });                    
            },
            orderList: function (col) {
                if (this.isAscending) {
                    this.isAscending = false;
                }
                else {
                    this.isAscending = true;
                }
                this.colType = col;
                this.getList({ search: '', page: this.queryPage, type: null })
            },
            formatDate(date) {
                return dates.formatDate(date);
            },
            editTeam(team) {
                this.selectedTeam = team;
                this.openModalTeam();
            },
            deleteTeam() {
                let teamId = this.selectedTeam.id;
                api.delete('/Team/DeleteByIds', { data: [teamId] })
                    .then((response) => {
                        this.closeModal();
                        this.getTeams({ search: '', page: 1, type: null });
                    }).catch(function (e) { 
                        console.log(e);
                    }).finally(function () { 
                        console.log("Finished request.");
                    });
                this.listIds = [];
            },
            filterList(input) {
                this.searchInput = input;
                this.getTeams({ search: input, page: this.queryPage, type: null });
            },
            handleTeamCreated: function() {
                this.getList({ search: '', page: this.queryPage, type: null });
                this.closeModalTeam();
            },
            openModalTeam: function() {
                this.modalTeamShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModalTeam: function() {
                this.modalTeamShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            confirmationDialog(team) {
                this.selectedTeam = team;
                this.modalAlertShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModal() {
                this.modalAlertShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            changePage(page) {
                this.getTeams({ search: '', page: page, type: null });
            }
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getTeams({ search: '', page: this.queryPage, type: null });
        },
    }
</script>