<template>
    <button 
        v-if="showMultiDelete" 
        class="btn btn-outline-danger btn-sm mb-2 ms-2" 
        @click="openConfirmation"
    >
        <LucideIcon icon="Trash2" :size="15" />
        {{ $t("labelDelete") }}
    </button>
    <div v-if="showTable">
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
                {{ formatDate(data.row.created) }}
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
            <template #cell-workflows="{ data }">
                <BadgeOutlinedComponent
                    v-for="(workflowData, index) in data.row.workflows"
                    :key="index"
                    :text="workflowData.name"
                    :clickable="false"
                    class="ms-1"
                />
            </template>
            <template #cell-actions="{ data }">
                <DropdownComponent>
                    <li v-if="data.row.status === 0">
                        <a 
                            class="dropdown-item d-flex align-items-center gap-2" 
                            @click="embedData(data.row.id)"
                        >
                            <LucideIcon icon="TextSearch" />
                            {{ $t("documents.actions.analyze") }}
                        </a>
                    </li>
                    <li v-else>
                        <a
                            class="dropdown-item d-flex align-items-center gap-2"
                            @click="redirectToConsult(data.row.id)"
                        >
                            <LucideIcon icon="Search" />
                            {{ $t("documents.actions.consult") }}
                        </a>
                    </li>
                </DropdownComponent>
            </template>
        </TableComponent>
    </div>
    <EmbeddingDocument
        v-if="isEmbedding"
        :docData="docDataEmbedding"
        :isReprocessing="isReprocessing"
    />
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
    import dates from "@/helpers/date";
    import TableComponent from "@/components/global/TableComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import DocumentsServices from "@/services/documents/DocumentsServices";
    import BadgeComponent from "@/components/global/BadgeComponent";
    import BadgeOutlinedComponent from "@/components/global/BadgeOutlinedComponent"
    import EmbeddingDocument from "@/components/documents/EmbeddingDocument.vue";
    import DropdownComponent from "@/components/global/DropdownComponent.vue";

    export default {
        name: "DocumentsTable",
        components: {
            DropdownComponent,
            EmbeddingDocument,
            BadgeOutlinedComponent,
            BadgeComponent,
            TableComponent,
            ConfirmModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "name", label: "documents.name" },
                    { key: "description", label: "documents.description" },
                    { key: "created", label: "documents.createdDate" },
                    { key: "status", label: "documents.status" },
                    { key: "workflows", label: "documents.workflows" },
                    { key: "actions", label: "questions.actions" },
                ],
                data: [],
                pagination: {
                    currentPage: 1,
                    totalPages: 0,
                    itemsPerPage: 10,
                    totalItems: 0,
                },
                selectedRows: [],
            },
            filters: {
                input: "",
                teamId: "",
                workflowId: "",
                workflows: [],
                isAsc: true,
                isAllUsers: false,
                login: null,
                colType: 2,
            },
            isEmbedding: false,
            isDeleting: false,
        }),
        methods: {
            getDocuments() {
                this.table.isLoading = true;
                const params = {
                    search: this.filters.input,
                    pageSize: 10,
                    page: this.table.pagination.currentPage,
                    isAscending: this.filters.isAsc,
                    isAllUsers: this.filters.isAllUsers,
                    colType: this.filters.colType,
                    login: this.filters.login,
                    workflowIds: this.filters.workflows,
                };
                
                DocumentsServices.getDocuments(params)
                    .then((response) => {
                        if (response?.error !== undefined) {
                            this.$notify({
                                title: 'Error',
                                message: "response.error",
                                variant: 'danger',
                                icon: 'CircleX',
                            });
                        }
                        this.table.data = response.content;
                        this.table.pagination = response.pagination;
                    })
                    .finally(() => {
                        this.table.isLoading = false;
                    });
            },
            selectedRows(selectedRows) {
                this.table.selectedRows = selectedRows;
            },
            openConfirmation() {
                const ids = this.table.selectedRows.map((item) => item.id);
                this.selectedDocument = ids;
                this.$refs.DeleteDialog.open();
            },
            deleteDocument() {
                this.isDeleting = true;
                DocumentsServices.deleteDocument(this.selectedDocument)
                    .then((success) => {
                        if (success) {
                            this.$refs.DeleteDialog.close();
                            this.getDocuments({ search: "", page: 1, type: null });
                            this.$notify({
                                title: this.$t("documents.title"),
                                message: this.$t("documents.removeSuccess"),
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                        } else {
                            this.$notify({
                                title: this.$t("documents.title"),
                                message: this.$t("documents.removeError"),
                                variant: 'danger',
                                icon: 'CircleX',
                            });
                        }
                    })
                    .finally(() => {
                        this.table.selectedRows = [];
                        this.isDeleting = false;
                    });
            },
            formatDate(date) {
                return dates.formatDate(date);
            },
            embedData(id) {
                this.docDataEmbedding.Id = id;
                this.isEmbedding = true
            },
            redirectToConsult(id) {
                this.$router.push({ 
                    name: "Analyzer", 
                    params: { 
                        id: id 
                    },
                    query: { 
                        page: this.table.pagination.currentPage
                    } 
                });
            },
            changePage(page) {
                this.table.pagination.currentPage = page;
                this.getDocuments();
            },
        },
        created() {
            this.filters.login = this.$store.state.userProfile.login;
            this.table.pagination.currentPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getDocuments();
        },
        computed: {
            showMultiDelete() {
                return this.table.selectedRows.length > 0;
            },
            showTable() {
                return this.table.data !== undefined;
            },
        },
    };
</script>

<style scoped>
    .analyze-btn {
        width: 94px;
    }
</style>