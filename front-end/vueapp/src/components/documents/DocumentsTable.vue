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
            modalName="documents.title"
            emptyMessage="documents.notFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
            :pagination="table.pagination"
            @selectedRows="selectedRows"
            @change-page="changePage"
        >
            <template #cell-actions="{ data }">
            </template>
        </TableComponent>
    </div>

    <ConfirmModal
        id="deleteConfirm"
        title="documents.removeTitle"
        message="labelThisActionCannotBeUndone"
        cancelText="labelCancel"
        confirmText="labelConfirm"
        confirmVariant="primary"
        ref="DeleteDialog"
        :isLoading="isDeleting"
        @confirm="deleteDocument"
    />
</template>

<script>
    import TableComponent from "@/components/global/TableComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";

    export default {
        name: "DocumentsTable",
        components: {
            TableComponent,
            ConfirmModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "Id" },
                    { key: "name", label: "documents.name" },
                    { key: "description", label: "documents.name" },
                    { key: "reference_file", label: "documents.name" },
                    { key: "status", label: "documents.name" },
                    { key: "emailCreator", label: "documents.owner" },
                    { key: "actions", label: "questions.actions" },
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
            selectedDocument: {},
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
            getDocuments(obj) {
            },
            orderList: function (col) {
                if (this.isAscending) {
                    this.isAscending = false;
                } else {
                    this.isAscending = true;
                }
                this.colType = col;
                this.getQuestions({ search: "", page: this.queryPage, type: null });
            },
            selectedRows(selectedRows) {
                this.table.selectedRows = selectedRows;
            },
            openConfirmation(document) {
                this.selectedDocument = [document.id];
                this.$refs.DeleteDialog.open();
            },
            openConfirmationMultiple() {
                const ids = this.table.selectedRows.map((item) => item.id);
                this.selectedDocument = ids;
                this.$refs.DeleteDialog.open();
            },
            filterList(input) {
                this.searchInput = input;
                this.getDocuments({ search: input, page: this.queryPage, type: null });
            },
            deleteDocument() {
                console.log("Remove Doc")
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getDocuments({ search: "", page: this.queryPage, type: null });
        },
        computed: {
            showMultiDelete() {
                return this.table.selectedRows.length > 1;
            },
        },
    };
</script>