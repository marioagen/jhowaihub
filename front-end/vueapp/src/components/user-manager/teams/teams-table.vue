<template>
    <div>
        <table-component
            modalName="labelUsers"
            emptyMessage="labelNoDocumentTypeWasFound"
            :totalRows="table.pagination.rowCount"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
        >
            <template #cell-actions="{ data }">
                <button
                    class="btn btn-outline-success btn-sm"
                >
                    Edit
                </button>
                <button
                    class="btn btn-outline-danger btn-sm ms-2"
                    @click="deleteTeam"
                >
                    Delete
                </button>
            </template>
        </table-component>
    </div>
    <div>
        <pagination-container
            :pagination="{currentPage: 1, pageCount: 2, rowCount: 20, listPage: 1}"
            :dataList="teams"
            :loading="table.isLoading"
        ></pagination-container>
    </div>
</template>

<script>
    import api from "@/services/api";
    import dates from "@/helpers/Dates";
    import TableComponent from "@/components/global/table-component.vue";
    import PaginationDivider from "@/utils/paginationDivider";
    const divider = new PaginationDivider();
    export default {
        name: "TeamsTable",
        components: {
            TableComponent,
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
                    currentPage: "",
                    pageCount: "",
                    rowCount: "",
                    listPage: "",
                },
            },
            queryPage: 1,
            searchInput: "",
            selectedOption: 10,
            isAscending: false,
            colType: 2,
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
                console.log(paramsReq)
                let self = this;
                api.get('/Team/Paged', { params: paramsReq })
                    .then(function (response) {
                        self.table.data = response.data.content;
                        self.table.pagination = {
                            currentPage: response.data.currentPage,
                            pageCount: response.data.pageCount,
                            rowCount: response.data.rowCount,
                            listPage: divider.calculatePageCount(response.data.pageCount, response.data.currentPage)
                        };
                        console.log(self.table.pagination)
                        if (obj.type === "search") self.searching = true;
                    }).catch(function (e) {
                        console.log(e);
                        if (obj.type === "search") self.searching = true;
                    }).finally(function () {
                        console.log("Finished request.");
                        self.table.isLoading = false;
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
            deleteTeam() {
                api.delete('/Team/DeleteByIds', { data: this.listIds })
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
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getTeams({ search: '', page: this.queryPage, type: null });
        },
    }
</script>