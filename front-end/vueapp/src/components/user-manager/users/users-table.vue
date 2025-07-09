<template>
    <div>
        <table-component modalName="labelTeams"
                         emptyMessage="labelNoDocumentTypeWasFound"
                         :totalRows="table.pagination.rowCount"
                         :data="table.data"
                         :columns="table.columns"
                         :isLoading="table.isLoading">
            <template #cell-name="{ data }">
                <div v-if="!loading" class="p-1">
                    <div class="d-flex">
                        <label class="form-check-label d-flex align-items-center w-100">
                            <div class="rounded-circle d-flex align-items-center justify-content-center btn-primary fw-bold me-3 initials">
                                {{ getInitials(data.row.name) }}
                            </div>
                            <div>
                                <div class="fw-semibold">{{ data.row.name }}</div>
                                <div class="text-muted small">{{ data.row.email }}</div>
                            </div>
                        </label>
                    </div>
                </div>
            </template>
            <template #cell-teams="{ data }">
                <span class="badge">{{ data.row.teams.namw }}</span>
            </template>
            <template #cell-actions="{ data }">
                <div class="dropdown">
                    <a class="btn p-0 border-0" href="#" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                        <i class="fas fa-ellipsis-v"></i>
                    </a>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li><a class="dropdown-item" href="#">Editar</a></li>
                        <li><a class="dropdown-item" href="#">Excluir</a></li>
                    </ul>
                </div>
            </template>
        </table-component>
    </div>
    <div>
        <!--<pagination-container :pagination="{currentPage: 1, pageCount: 2, rowCount: 20, listPage: 1}"
                              :dataList="users"
                              :loading="table.isLoading"></pagination-container>-->
    </div>
</template>

<script>
    import api from "@/services/api";
    import dates from "@/helpers/Dates";
    import TableComponent from "@/components/global/table-component.vue";
    import PaginationDivider from "@/utils/paginationDivider";
    const divider = new PaginationDivider();
    export default {
        name: "UsersTable",
        components: {
            TableComponent,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "Id" },
                    { key: "name", label: "labelUserName" },
                    { key: "teams", label: "labelTeamsNames" },
                    { key: "actions", label: "labelAction" },
                ],
                data: [],
                pagination: {
                    currentPage: "",
                    pageCount: "",
                    rowCount: "",
                    listPage: "",
                    totalItems: 2000,
                },
            },
            queryPage: 1,
            searchInput: "",
            selectedOption: 10,
            isAscending: false,
            colType: 2,
        }),
        methods: {
            getUsers: function (obj) {
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
                api.get('/User/Paged', { params: paramsReq })
                    .then(function (response) {
                        self.table.data = response.data.content;
                        self.table.pagination = {
                            currentPage: response.data.currentPage,
                            pageCount: response.data.pageCount,
                            rowCount: response.data.rowCount,
                            listPage: divider.calculatePageCount(response.data.pageCount, response.data.currentPage)
                        };
                        console.log(self.table.pagination)
                        console.log(self.table.data);
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
                this.getUsers({ search: input, page: this.queryPage, type: null });
            },
            getInitials(name) {
                if (!name) return '';
                const parts = name.trim().split(' ');
                if (parts.length === 1) {
                    const n = parts[0];
                    return (n[0] || '').toUpperCase() + (n[n.length - 1] || '').toUpperCase();
                }
                const first = parts[0][0] || '';
                const last = parts[parts.length - 1].slice(-1) || '';
                return (first + last).toUpperCase();
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getUsers({ search: '', page: this.queryPage, type: null });
        },
    }
</script>
<style scoped>

    .badge {
        display: inline-block;
        background-color: #e0ecff;
        color: #0057d8;
        padding: 4px 10px;
        border-radius: 6px;
        font-size: 12px;
        margin-right: 4px;
    }

    .initials{
    width: 30px;
    height: 30px;
}
</style>