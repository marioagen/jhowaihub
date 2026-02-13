<template>
    <div class="mt-3 mb-3">
        <div class="d-flex justify-content-end align-items-center mb-3">
            <button class="btn btn-primary btn-sm"
                    @click="redirectToNewUpload">
                <LucideIcon icon="Plus"
                            :size="17" />
                {{ $t("documents.createBtn") }}
            </button>
        </div>
        <div class="card mb-3">
            <div class="card-body">
                <DocumentFilters :workflowsList="workflowsList"
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
    import StatusService from '@/services/status/StatusService';

    export default {
        name: "DocumentsPage",
        data() {
            return {
                teamsList: [],
                workflowsList: [],
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
            reloadData() {
                this.$refs.DocumentsTable.getDocuments();
            },
            filterData(filters) {
                this.$refs.DocumentsTable.filters = filters;
                this.reloadData();
            },
            getWorkflows() {
                var email =
                    this.$store.state.userProfile.login;
                WorkflowService.getWorkflowList(email).then(
                    (response) => {
                        if (response.error !== undefined) {
                            return this.$notify({
                                title: "workflows.title",
                                message: "workflows.error",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                        this.workflowsList = response;
                    }
                );
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
                return this.$store.state.userProfile
                    .keyMongoAccess;
            },
        },
        async created() {
            GlobalEventService.on(
                "all-uploads-complete",
                this.reloadData
            );
            GlobalEventService.on(
                "refresh-once",
                this.reloadData
            );
            this.getWorkflows();
            await this.getStatuses();
        },
        beforeUnmount() {
            GlobalEventService.off(
                "all-uploads-complete",
                this.reloadData
            );
            GlobalEventService.off(
                "refresh-once",
                this.reloadData
            );
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
