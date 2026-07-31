<template>
    <div class="bulk-actions-bar mb-2" v-if="enableMultiDelete">
        <span class="bulk-actions-bar__count">
            <LucideIcon icon="CheckSquare" :size="14" />
            {{ $t("documents.selectedCount", { count: table.selectedRows.length }) }}
        </span>
        <div class="bulk-actions-bar__buttons">
            <button
                class="btn btn-outline-secondary btn-sm"
                :disabled="isExportingSelected"
                @click="exportSelectedAsCsv"
            >
                <span
                    v-if="isExportingSelected"
                    class="spinner-border spinner-border-sm me-1"
                    role="status"
                />
                <LucideIcon v-else icon="Download" :size="14" />
                {{ $t("documents.exportCsv") }}
            </button>
            <button
                class="btn btn-outline-danger btn-sm"
                @click="openConfirmation"
            >
                <LucideIcon icon="Trash2" :size="14" />
                {{ $t("common.delete") }}
            </button>
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
            <template #cell-anonymizations="{ data }">
                <button
                    v-if="data.row.anonymizationAmount > 0"
                    class="btn btn-outline-success btn-sm"
                    @click="openAnonymizationsModal(data.row.id)"
                    :disabled="!data.row.id"
                >
                    <LucideIcon
                        icon="ShieldCheck"
                        :size="16"
                    />
                    {{ $t("analyze.anonymizations") }}
                    <small>({{ data.row.anonymizationAmount }})</small>
                </button>
                <span
                    v-else
                    class="text-muted"
                >
                    -
                </span>
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
                    <a
                        :class="[actionClass, exportingRowId === data.row.id ? 'disabled-action' : '']"
                        class="text-secondary"
                        @click="exportRowAsCsv(data.row)"
                        v-tooltip="$t('documents.exportCsv')"
                    >
                        <span
                            v-if="exportingRowId === data.row.id"
                            class="spinner-border spinner-border-sm"
                            role="status"
                        />
                        <LucideIcon v-else icon="Download" />
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
    <DocumentAnonymizationsModal ref="DocumentAnonymizationsModal" />
</template>
<script>
    import dates from "@/helpers/date";
    import TableComponent from "@/components/global/TableComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import DocumentsServices from "@/services/documents/DocumentsServices";
    import AnonymizationServices from "@/services/anonymization/AnonymizationServices";
    import BadgeComponent from "@/components/global/BadgeComponent";
    import BadgeOutlinedComponent from "@/components/global/BadgeOutlinedComponent";
    import EmbeddingDocument from "@/components/documentsHub/documents/EmbeddingDocument.vue";
    import ActionTableListComponent from "@/components/global/ActionTableListComponent.vue";
    import DocumentWorkflowListModal from "@/components/documentsHub/documents/modals/DocumentWorkflowListModal.vue";
    import DocumentAnonymizationsModal from "@/components/analyze/modals/DocumentAnonymizationsModal.vue";
    import { downloadCsv } from "@/helpers/csvHelper";

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
            DocumentAnonymizationsModal,
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
                        key: "anonymizations",
                        label: "analyze.anonymizations",
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
            isExportingSelected: false,
            docDataEmbedding: {},
            selectedDocumentId: null,
            exportingRowId: null,
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
                                title: "common.error",
                                message: "documents.loadError",
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
                        message: "documents.embedError",
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
            async exportSelectedAsCsv() {
                if (this.isExportingSelected) return;
                this.isExportingSelected = true;
                try {
                    const columns = [
                        { key: "name",           header: this.$t("documents.csvColumns.name") },
                        { key: "description",    header: this.$t("documents.csvColumns.description") },
                        { key: "uploadDate",     header: this.$t("documents.csvColumns.uploadDate") },
                        { key: "workflows",      header: this.$t("documents.csvColumns.workflows") },
                        { key: "anonymizations", header: this.$t("documents.csvColumns.anonymizations") },
                    ];

                    const rows = this.table.selectedRows.map((row) => ({
                        name:           row.name ?? "",
                        description:    row.description ?? "",
                        uploadDate:     row.created ? this.formatDate(row.created) : "",
                        workflows:      (row.workflowProgress ?? [])
                            .map((w) => `${w.workflowName} (${w.currentStep}/${w.totalSteps})`)
                            .join("; "),
                        anonymizations: row.anonymizationAmount ?? 0,
                    }));

                    downloadCsv(rows, columns, "documentos-selecionados");
                } finally {
                    this.isExportingSelected = false;
                }
            },
            async exportRowAsCsv(row) {
                if (this.exportingRowId === row.id) return;
                this.exportingRowId = row.id;
                try {
                    const columns = [
                        { key: "name",           header: this.$t("documents.csvColumns.name") },
                        { key: "description",    header: this.$t("documents.csvColumns.description") },
                        { key: "uploadDate",     header: this.$t("documents.csvColumns.uploadDate") },
                        { key: "workflows",      header: this.$t("documents.csvColumns.workflows") },
                        { key: "anonymizations", header: this.$t("documents.csvColumns.anonymizations") },
                    ];

                    const exportRow = {
                        name:           row.name ?? "",
                        description:    row.description ?? "",
                        uploadDate:     row.created ? this.formatDate(row.created) : "",
                        workflows:      (row.workflowProgress ?? [])
                            .map((w) => `${w.workflowName} (${w.currentStep}/${w.totalSteps})`)
                            .join("; "),
                        anonymizations: row.anonymizationAmount ?? 0,
                    };

                    const filename = (row.name ?? "documento").replace(/\.[^.]+$/, "");
                    downloadCsv([exportRow], columns, filename);
                } finally {
                    this.exportingRowId = null;
                }
            },
            openAnonymizationsModal(documentId) {
                AnonymizationServices.getDocumentAnonymizations(documentId).then((response) => {
                    if (response && !response.error) {
                        this.$refs.DocumentAnonymizationsModal.open(response.data);
                    } else {
                        this.$notify({
                            title: "common.error",
                            message: "analyze.anonymizationsLoadError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                });
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

    .bulk-actions-bar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 0.75rem;
        padding: 0.45rem 0.85rem;
        border-radius: 8px;
        background: var(--bs-primary-bg-subtle, rgba(13, 110, 253, 0.06));
        border: 1px solid var(--bs-primary-border-subtle, rgba(13, 110, 253, 0.25));
        animation: bulk-bar-in 0.15s ease;
    }

    @keyframes bulk-bar-in {
        from { opacity: 0; transform: translateY(-4px); }
        to   { opacity: 1; transform: translateY(0); }
    }

    .bulk-actions-bar__count {
        display: flex;
        align-items: center;
        gap: 0.4rem;
        font-size: 0.8rem;
        font-weight: 600;
        color: var(--bs-primary, #0d6efd);
    }

    .bulk-actions-bar__buttons {
        display: flex;
        align-items: center;
        gap: 0.5rem;
    }

    .disabled-action {
        pointer-events: none;
        opacity: 0.45;
    }
</style>
