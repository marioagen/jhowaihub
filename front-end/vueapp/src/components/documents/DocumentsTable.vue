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
            <template #cell-created="{ data }">
                
            </template>
            <template #cell-status="{ data }">
                <BadgeComponent
                    v-if="data.row.status === 0" 
                    text="documents.statusList.notAnalyzed"
                />
                <BadgeComponent
                    v-else 
                    text="documents.statusList.analyzed"
                    variant="success"
                />
            </template>
            <template #cell-teams="{ data }">
                <BadgeOutlinedComponent
                    v-for="(team, index) in data.row.teams"
                    :key="index"
                    :text="team.name"
                    :clickable="false"
                />
            </template>
            <template #cell-actions="{ data }">
                <button
                    v-if="data.row.status === 0"
                    class="btn btn-outline-primary btn-sm table-btn"
                >
                    {{ $t("documents.actions.analyze") }}
                </button>
                <button
                    v-else
                    class="btn btn-outline-success btn-sm table-btn"
                >
                    {{ $t("documents.actions.consult") }}
                </button>
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
    import DocumentsServices from "@/services/documents/DocumentsServices";
    import BadgeComponent from "@/components/global/BadgeComponent";
    import BadgeOutlinedComponent from "@/components/global/BadgeOutlinedComponent"

    export default {
        name: "DocumentsTable",
        components: {
            BadgeOutlinedComponent,
            BadgeComponent,
            TableComponent,
            ConfirmModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "Id" },
                    { key: "name", label: "documents.name" },
                    { key: "description", label: "documents.description" },
                    { key: "created", label: "documents.createdDate" },
                    { key: "status", label: "documents.status" },
                    { key: "teams", label: "documents.teams" },
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
                this.table.isLoading = true;

                const teamIds = this.resolveTeamIds();
                if (teamIds.length === 0) return;

                const params = {
                    search: this.searchInput.trim() || "",
                    pageSize: this.selectedOption,
                    page: obj.page,
                    isAscending: this.isAscending,
                    colType: this.colType,
                    teamIds,
                };
                DocumentsServices.getDocuments(params)
                    .then((response) => {
                        console.log(response)
                        this.table.data = response.content;
                        this.table.pagination = response.pagination;
                    })
                    .finally(() => {
                        this.table.isLoading = false;
                    });
            },
            resolveTeamIds() {
                if (this.selectedTeamId === 0) {
                    return this.teamList.length > 0 ? this.teamList.map((team) => team.id) : [];
                }
                return [this.selectedTeamId];
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