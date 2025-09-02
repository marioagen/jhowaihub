<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("documents.title") }}</h5>
                        <p>
                            <small class="text-muted">{{ $t("documents.subtitle") }}</small>
                        </p>
                    </div>
                    <button class="btn btn-primary btn-sm" @click="redirectToNewUpload">
                        <LucideIcon icon="Plus" size="17" />
                        {{ $t("documents.createBtn") }}
                    </button>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="d-flex align-items-center gap-2 flex-wrap">
                            <div class="flex-grow-1">
                                <DocumentFilters 
                                    @filter="filterData"
                                />
                            </div>

                            <div class="w-auto">
                                
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <DocumentsTable 
                ref="DocumentsTable"
            />
        </div>
    </main>
</template>

<script>
    import GlobalEventService from "@/services/globalEventService.js";
    import DocumentFilters from "@/components/documents/DocumentFilters.vue";
    import DocumentsTable from "@/components/documents/DocumentsTable.vue";

    export default {
        name: "DocumentsPage",
        data() {
            return {
            };
        },
        components: {
            DocumentFilters,
            DocumentsTable,
        },
        watch: {
            "$store.state.userProfile.language"() {
            },
            keyMongoAccess: {
                immediate: true,
                handler: async function (newValue) {
                    if (newValue) {
                        //filter data
                    }
                },
            },
        },
        methods: {
            redirectToNewUpload() {
                this.$router.push({ name: "DocumentsUpload" });
            },
            filterData(filters) {
                this.$refs.DocumentsTable.filters = filters;
                this.$refs.DocumentsTable.getDocuments();
            },
        },
        computed: {
            keyMongoAccess() {
                return this.$store.state.userProfile.keyMongoAccess;
            },
        },
        async created() {
            //Ask Gab
            GlobalEventService.on("all-uploads-complete", this.reloadList);
            GlobalEventService.on("refresh-once", this.reloadList);
            if (this.noTeams) {
                this.selectedTeamId = null;
            }
        },
        beforeUnmount() {
            //Ask Gab
            GlobalEventService.off("all-uploads-complete", this.reloadList);
            GlobalEventService.off("refresh-once", this.reloadList);
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
