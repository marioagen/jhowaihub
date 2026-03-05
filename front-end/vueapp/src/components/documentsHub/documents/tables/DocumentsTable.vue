<template>
    <div class="d-flex flex-column justify-content-between align-items-start mb-2">
        <div class="delete-container">
            <button
                class="btn btn-outline-danger btn-sm delete-button"
                @click="openConfirmation"
                :disabled="!enableMultiDelete"
            >
                <LucideIcon
                    icon="Trash2"
                    :size="15"
                />
                {{ $t("common.delete") }}
            </button>
            <small
                v-if="!enableMultiDelete"
                class="text-danger delete-tooltip"
            >
                {{ $t("documents.selectToDelete") }}
            </small>
        </div>
    </div>
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
            <template #cell-name="{ data }">
                <span class="d-flex align-items-center gap-1">
                    <LucideIcon
                        v-if="data.row.hasBatch"
                        icon="Files"
                        :size="16"
                        class="text-primary"
                        v-tooltip="$t('documents.batchFile')"
                    />
                    {{ data.row.name }}
                </span>
            </template>
            <template #cell-created="{ data }">
                {{ formatDate(data.row.created) }}
            </template>
            <template #cell-workflows="{ data }">
                <BadgeOutlinedComponent
                    v-for="(workflowData, index) in data.row.workflowProgress"
                    :key="index"
                    :text="`${workflowData.workflowName} (${workflowData.currentStep}/${workflowData.totalSteps})`"
                    :clickable="false"
                    class="ms-1"
                />
            </template>
            <template #cell-actions="{ data }">
                <ActionTableListComponent v-slot="{ actionClass }">
                    <a
                        :class="actionClass"
                        class="text-primary"
                        @click="getWorkFlowListByDocumentId(data.row.id)"
                        v-tooltip="$t('documents.actions.consult')"
                    >
                        <LucideIcon icon="Search" />
                    </a>
                </ActionTableListComponent>
            </template>
        </TableComponent>
    </div>
    <EmbeddingDocument
        v-if="isEmbedding"
        :docData="docDataEmbedding"
        :isReprocessing="isReprocessing"
        @close="isEmbedding = false"
    />
    <ConfirmModal
        id="deleteConfirm"
        title="documents.removeTitle"
        message="common.thisActionCannotBeUndone"
        cancelText="common.cancel"
        confirmText="common.confirm"
        confirmVariant="primary"
        ref="DeleteDialog"
        :isLoading="isDeleting"
        @confirm="deleteDocument"
    />
    <DocumentWorkflowListModal
        id="typeModalWorkflow"
        :documentId="selectedDocumentId"
        ref="ListWorkFlowModal"
    />
</template>
<script>
    import dates from "@/helpers/date";
    import TableComponent from "@/components/global/TableComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import DocumentsServices from "@/services/documents/DocumentsServices";
    import BadgeComponent from "@/components/global/BadgeComponent";
    import BadgeOutlinedComponent from "@/components/global/BadgeOutlinedComponent";
    import EmbeddingDocument from "@/components/documentsHub/documents/EmbeddingDocument.vue";
    import ActionTableListComponent from "@/components/global/ActionTableListComponent.vue";
    import DocumentWorkflowListModal from "@/components/documentsHub/documents/modals/DocumentWorkflowListModal.vue";

    export default {
        name: "DocumentsTable",
        components: {
            ActionTableListComponent,
            EmbeddingDocument,
            BadgeOutlinedComponent,
            BadgeComponent,
            TableComponent,
            ConfirmModal,
            DocumentWorkflowListModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "name", label: "common.name" },
                    {
                        key: "description",
                        label: "common.description",
                    },
                    {
                        key: "created",
                        label: "documents.createdDate",
                    },
                    {
                        key: "workflows",
                        label: "documents.workflows",
                    },
                    {
                        key: "actions",
                        label: "common.actions",
                    },
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
                workflowId: "",
                workflows: [],
                statusId: "",
                isAsc: true,
                isAllUsers: false,
                login: null,
                colType: 2,
                document: "1",
            },
            isEmbedding: false,
            isDeleting: false,
            docDataEmbedding: {},
            selectedDocumentId: null,
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
                    documentType: this.filters.document,
                };
                if (this.filters.statusId !== "" && this.filters.statusId != null) {
                    params.statusId = Number(this.filters.statusId);
                }

                DocumentsServices.getDocuments(params)
                    .then((response) => {
                        if (response?.error !== undefined) {
                            this.$notify({
                                title: "Error",
                                message: "response.error",
                                variant: "danger",
                                icon: "CircleX",
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
                            this.getDocuments({
                                search: "",
                                page: 1,
                                type: null,
                            });
                            this.$notify({
                                title: this.$t("documents.title"),
                                message: this.$t("documents.removeSuccess"),
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: this.$t("documents.title"),
                                message: this.$t("documents.removeError"),
                                variant: "danger",
                                icon: "CircleX",
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
                if (this.docDataEmbedding === undefined) {
                    return this.$notify({
                        title: "documents.title",
                        message: "documents.embeddError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                }

                this.docDataEmbedding.Id = id;
                this.isEmbedding = true;
            },
            redirectToConsult(id) {
                this.$router.push({
                    name: "Analyzer",
                    params: {
                        id: id,
                    },
                    query: {
                        page: this.table.pagination.currentPage,
                    },
                });
            },
            getWorkFlowListByDocumentId(id) {
                this.selectedDocumentId = id;
                this.$nextTick(() => {
                    this.$refs.ListWorkFlowModal.open();
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
            enableMultiDelete() {
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
    .modal-div {
        display: flex;
        flex-direction: column;
        width: 100%;
    }

    .modal-title,
    .modal-message {
        text-align: center;
    }

    .analyze-btn {
        width: 94px;
    }

    .delete-container {
        position: relative;
        display: inline-block;
    }

    .delete-button {
        position: relative;
    }

    .delete-tooltip {
        opacity: 0;
        pointer-events: none;
        visibility: hidden;
        transition:
            opacity 0.2s ease,
            visibility 0.2s ease;
        position: absolute;
        top: calc(100% + 8px);
        left: 0;
        white-space: nowrap;
        background-color: #fff;
        border: 1px solid #dc3545;
        border-radius: 6px;
        padding: 6px 12px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
        z-index: 1000;
    }

    .delete-tooltip::before {
        content: "";
        position: absolute;
        bottom: 100%;
        left: 20px;
        border: 6px solid transparent;
        border-bottom-color: #dc3545;
    }

    .delete-tooltip::after {
        content: "";
        position: absolute;
        bottom: 100%;
        left: 21px;
        border: 5px solid transparent;
        border-bottom-color: #fff;
    }

    .delete-container:hover .delete-tooltip {
        opacity: 1;
        visibility: visible;
    }

    .delete-button:not(:disabled) ~ .delete-tooltip {
        display: none !important;
    }
</style>
