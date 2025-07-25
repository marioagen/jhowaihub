<template>
    <button 
        v-if="showMultiDelete" 
        class="btn btn-outline-danger btn-sm mb-2 ms-2" 
        @click="openConfirmationMultiple"
    >
        <LucideIcon icon="Trash2" size="15" />
        {{ $t("labelDelete") }}
    </button>
    <div>
        <TableComponent
            modalName="labelTypes"
            emptyMessage="labelNoDocumentTypeWasFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
            :pagination="table.pagination"
            @selectedRows="selectedRows"
            @change-page="changePage"
        >
            <template #cell-created="{ data }">
                {{ formatDate(data.row.created) }}
            </template>
            <template #cell-actions="{ data }">
                <button 
                    class="btn btn-outline-success btn-sm table-btn" 
                    @click="openEditModal(data.row)"
                >
                    <LucideIcon icon="SquarePen" />
                </button>
                <button 
                    class="btn btn-outline-danger btn-sm ms-2 table-btn" 
                    @click="openConfirmation(data.row)"
                >
                    <LucideIcon icon="Trash2" />
                </button>
            </template>
        </TableComponent>
    </div>

    <TypesModal
        :isEdit="true"
        @reload="getTypes({ search: '', page: this.queryPage, type: null })"
        ref="TypesModal"
    />

    <ConfirmModal
        id="deleteConfirm"
        title="labelYouAreAboutToDeleteType"
        message="labelThisActionCannotBeUndone"
        cancelText="labelCancel"
        confirmText="labelConfirm"
        confirmVariant="primary"
        ref="DeleteDialog"
        :isLoading="isDeleting"
        @confirm="deleteType"
    />
</template>

<script>
    import dates from "@/helpers/Dates";
    import TypesService from "@/services/types/TypesService";
    import TableComponent from "@/components/global/TableComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import TypesModal from "@/components/types/TypesModal.vue";

    export default {
        name: "TypesTable",
        components: {
            TableComponent,
            ConfirmModal,
            TypesModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "Id" },
                    { key: "name", label: "labelName" },
                    { key: "created", label: "labelInclusionDate" },
                    { key: "emailCreator", label: "labelOwner" },
                    { key: "actions", label: "labelAction" },
                ],
                data: [],
                pagination: {
                    currentPage: 1,
                    totalPages: 100,
                    itemsPerPage: 10,
                    totalItems: 2000,
                },
                selectedRows: [],
            },
            selectedType: {},
            queryPage: 1,
            selectedOption: 10,
            isAscending: false,
            colType: 2,
            modalTypeShow: false,
            modalAlertShow: false,
            toastShow: false,
            toastColor: "",
            toastMessage: "",
            searchInput: "",
            isDeleting: false,
        }),
        methods: {
             getList: function (obj) {
                // obj = { search, page, type }
                this.listIds = [];
                this.searchInput = obj.search;
                this.loading = true;
                this.searching = false;
                this.dataQuestion = [];
                var paramsReq = {
                    search: this.searchInput.trim() ? this.searchInput.trim() : "",
                    page: obj.page,
                    pageSize: this.selectedOption,
                    isAscending: this.isAscending,
                    colType: this.colType,
                };
                let self = this;
                api.get("/Question/Paged", { params: paramsReq })
                    .then(function (response) {
                        // Handle success
                        self.dataQuestion = response.data.content;
                        self.pagination = {
                            currentPage: response.data.currentPage,
                            pageCount: response.data.pageCount,
                            rowCount: response.data.rowCount,
                            listPage: self.divider.calculatePageCount(
                                response.data.pageCount,
                                response.data.currentPage
                            ),
                        };
                        self.loading = false;
                        if (obj.type === "search") self.searching = true;
                    })
                    .catch(function (e) {
                        // Handle error
                        console.log(e);
                        self.loading = false;
                        if (obj.type === "search") self.searching = true;
                    })
                    .finally(function () {
                        // Always executed
                        console.log("Finished request.");
                    });
            },
            formatDate(date) {
                return dates.formatDate(date);
            },
            orderList: function (col) {
                if (this.isAscending) {
                    this.isAscending = false;
                } else {
                    this.isAscending = true;
                }
                this.colType = col;
                this.getTypes({ search: "", page: this.queryPage, type: null });
            },
            selectedRows(selectedRows) {
                this.table.selectedRows = selectedRows;
            },
            openEditModal(type) {
                this.$refs.TypesModal.open(type);
            },
            emitToast(message, color) {
                this.$emit("toast", { message, color });
            },
            finishEdit() {
                this.closeModal();
                this.getTypes({ search: "", page: 1, type: null });
            },
            openConfirmation(type) {
                this.selectedType = [type.id];
                this.$refs.DeleteDialog.open();
            },
            openConfirmationMultiple() {
                const ids = this.table.selectedRows.map((item) => item.id);
                this.selectedType = ids;
                this.$refs.DeleteDialog.open();
            },
            deleteType() {
                this.isDeleting = true;
                TypesService.deleteTypeById(this.selectedType)
                    .then((success) => {
                        if (success) {
                            this.$refs.DeleteDialog.close();
                            this.getTypes({ search: "", page: 1, type: null });
                            this.emitToast(
                                this.$t("labelDocumentTypeRemoveSuccess"), 
                                "toast-success"
                            );
                        } else {
                            this.emitToast(
                                this.$t("labelDocumentTypeRemoveError"), 
                                "toast-warning"
                            );
                        }
                    })
                    .finally(() => {
                        this.listIds = [];
                        this.table.selectedRows = [];
                        this.isDeleting = false;
                    });
            },
            filterList(input) {
                this.searchInput = input;
                this.getTypes({ search: input, page: this.queryPage, type: null });
            },
            openModalType: function () {
                this.modalTypeShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModalType: function () {
                this.modalTypeShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            confirmationDialog(team) {
                this.selectedTeam = team;
                this.modalAlertShow = true;
                document.getElementsByTagName("BODY")[0].children[1].className += " active";
            },
            closeModal() {
                this.modalAlertShow = false;
                this.modalTypeShow = false;
                document.getElementsByTagName("BODY")[0].children[1].className = "overlay";
            },
            changePage(page) {
                this.getTypes({ search: "", page: page, type: null });
            },
            alertToast: function (msg, color) {
                this.toastMessage = msg;
                this.toastColor = color;
                this.toastShow = true;
                let self = this;
                this.myInterval = setInterval(function () {
                    self.toastMessage = "";
                    self.toastColor = "";
                    self.toastShow = false;
                    clearInterval(self.myInterval);
                }, 4000);
            },
            closeToast: function () {
                this.toastShow = false;
                this.clearMyInterval();
            },
            clearMyInterval: function () {
                clearInterval(this.myInterval);
                this.myInterval = null;
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getTypes({ search: "", page: this.queryPage, type: null });
        },
        computed: {
            showMultiDelete() {
                return this.table.selectedRows.length > 1;
            },
        },
    };
</script>