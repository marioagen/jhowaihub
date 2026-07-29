<template>
    <div class="mt-3 mb-3">
        <div class="d-flex justify-content-end align-items-center gap-2 mb-3">
            <button
                class="btn btn-outline-secondary btn-sm"
                :disabled="isExportingCsv"
                @click="exportDocumentsCsv"
            >
                <span
                    v-if="isExportingCsv"
                    class="spinner-border spinner-border-sm me-1"
                    role="status"
                />
                <LucideIcon
                    v-else
                    icon="Download"
                    :size="15"
                    class="me-1"
                />
                {{ $t("documents.exportCsv") }}
            </button>
            <button
                class="btn btn-primary btn-sm"
                @click="redirectToNewUpload"
            >
                <LucideIcon
                    icon="Plus"
                    :size="17"
                    class="me-2"
                />
                {{ $t("documents.createBtn") }}
            </button>
        </div>
        <div class="card mb-3">
            <div class="card-body">
                <DocumentFilters
                    :workflowsList="workflowsList"
                    @filter="filterData"
                    ref="DocumentFilters"
                    :statusList="statusList"
                />
            </div>
        </div>
    </div>
    <DocumentsTable ref="DocumentsTable" />
</template>
<script>
    import GlobalEventService from "@/services/globalEventService.js";
    import DocumentFilters from "@/components/documentsHub/documents/filters/DocumentFilters.vue";
    import DocumentsTable from "@/components/documentsHub/documents/tables/DocumentsTable.vue";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import StatusService from "@/services/status/StatusService";
    import DocumentsServices from "@/services/documents/DocumentsServices";
    import { downloadCsv } from "@/helpers/csvHelper";
    import dates from "@/helpers/date";

    export default {
        name: "DocumentsPage",
        data() {
            return {
                teamsList: [],
                workflowsList: [],
                statusList: [],
                isExportingCsv: false,
            };
        },
        components: {
            DocumentFilters,
            DocumentsTable,
        },
        watch: {
            keyMongoAccess: {
                handler: async function (newValue) {
                    if (newValue) {
                        this.reloadData();
                    }
                },
            },
        },
        methods: {
            redirectToNewUpload() {
                this.$router.push({
                    name: "DocumentsUpload",
                });
            },
            async exportDocumentsCsv() {
                this.isExportingCsv = true;
                try {
                    const activeFilters = this.$refs.DocumentsTable?.filters ?? {};
                    const rows = await DocumentsServices.findAllForExport(activeFilters);

                    const columns = [
                        { key: "name",            header: this.$t("documents.csvColumns.name") },
                        { key: "description",     header: this.$t("documents.csvColumns.description") },
                        { key: "uploadDate",      header: this.$t("documents.csvColumns.uploadDate") },
                        { key: "workflows",       header: this.$t("documents.csvColumns.workflows") },
                        { key: "anonymizations",  header: this.$t("documents.csvColumns.anonymizations") },
                    ];

                    const exportRows = rows.map((doc) => ({
                        name:           doc.name ?? "",
                        description:    doc.description ?? "",
                        uploadDate:     doc.created ? dates.formatDate(doc.created) : "",
                        workflows:      (doc.workflowProgress ?? [])
                            .map((w) => `${w.workflowName} (${w.currentStep}/${w.totalSteps})`)
                            .join("; "),
                        anonymizations: doc.anonymizationAmount ?? 0,
                    }));

                    downloadCsv(exportRows, columns, "documentos-esteiras");
                } catch {
                    this.$notify({
                        title: this.$t("common.error"),
                        message: this.$t("documents.loadError"),
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isExportingCsv = false;
                }
            },
            reloadData() {
                this.$refs.DocumentsTable.getDocuments();
            },
            filterData(filters) {
                this.$refs.DocumentsTable.filters = filters;
                this.reloadData();
            },
            getWorkflows() {
                var email = this.$store.state.userProfile.login;
                WorkflowService.getWorkflowList(email).then((response) => {
                    if (response.error !== undefined) {
                        return this.$notify({
                            title: "workflows.title",
                            message: "workflows.error",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }
                    this.workflowsList = response;
                });
            },
            async getStatuses() {
                const response = await StatusService.getStatus();
                if (response?.error === undefined && Array.isArray(response)) {
                    this.statusList = response;
                }
            },
        },
        computed: {
            keyMongoAccess() {
                return this.$store.state.userProfile.keyMongoAccess;
            },
        },
        async created() {
            GlobalEventService.on("all-uploads-complete", this.reloadData);
            GlobalEventService.on("refresh-once", this.reloadData);
            this.getWorkflows();
            await this.getStatuses();
        },
        beforeUnmount() {
            GlobalEventService.off("all-uploads-complete", this.reloadData);
            GlobalEventService.off("refresh-once", this.reloadData);
        },
    };
</script>
<style scoped>
    .team-list {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
    }

    .team-badge {
        background-color: #f1f1f1;
        border: 1px solid #ccc;
        border-radius: 12px;
        padding: 4px 10px;
        font-size: 0.85rem;
        color: #333;
        white-space: nowrap;
    }

    .content-center {
        align-items: center;
        display: flex;
        flex-direction: row;
        flex-wrap: wrap;
        justify-content: center;
    }

    tbody {
        background-color: #fff !important;
    }

    .content-left-middle {
        text-align: left;
        vertical-align: middle;
    }

    .content-center-middle {
        text-align: center;
        vertical-align: middle;
    }

    .bg-success {
        background-color: #edfef2 !important;
        color: #0eaa42 !important;
        font-weight: inherit !important;
        padding: 8px 12px !important;
    }

    .navbar-container {
        padding-top: 0px;
        padding: 0;
    }

    .container-fluid {
        padding: 0 13px;
    }

    .scroll-area {
        display: list-item;
        overflow-y: auto;
    }

    @media (max-width: 768px) {
        .lines {
            display: none !important;
        }
    }
</style>
