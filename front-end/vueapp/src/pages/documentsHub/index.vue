<template>
    <main>
        <div class="container-fluid scroll-area manage-user mx-2">
            <div class="row">
                <div class="col-12">
                    <h5 class="mb-0 fw-bold">
                        {{ pageTitle }}
                    </h5>
                    <p>{{ pageSubtitle }}</p>
                    <TabsComponent
                        :tabs="tabsList"
                        color="custom"
                        ref="TabsComponent"
                        @selected="onTabSelected"
                    >
                        <template #workflows>
                            <WorkflowsKanbanComponent />
                        </template>
                        <template #documents>
                            <DocumentsComponent />
                        </template>
                        <template #context-dossiers>
                            <ContextDossiersList />
                        </template>
                    </TabsComponent>
                </div>
            </div>
        </div>
    </main>
</template>
<script>
    import TabsComponent from "@/components/global/TabsComponent.vue";
    import WorkflowsKanbanComponent from "@/components/documentsHub/workflows/WorkflowsKanbanComponent.vue";
    import DocumentsComponent from "@/components/documentsHub/documents/DocumentsComponent.vue";
    import ContextDossiersList from "@/components/documentsHub/contextDossiers/ContextDossiersList.vue";

    export default {
        name: "ManagementIndex",
        components: {
            TabsComponent,
            WorkflowsKanbanComponent,
            DocumentsComponent,
            ContextDossiersList,
        },
        data: () => ({
            activeTab: "workflows",
            tabsList: [
                {
                    name: "workflows",
                    label: "documentsHub.workflows.title",
                    icon: "LayoutGrid",
                },
                {
                    name: "documents",
                    label: "documentsHub.documents.title",
                    icon: "List",
                },
                {
                    name: "context-dossiers",
                    label: "contextDossiers.tab",
                    icon: "Files",
                },
            ],
        }),
        mounted() {
            this.applyRouteTab();
        },
        watch: {
            "$route.query.tab"() {
                this.applyRouteTab();
            },
        },
        computed: {
            pageTitle() {
                return this.activeTab === "context-dossiers"
                    ? this.$t("contextDossiers.title")
                    : this.$t("documentsHub.title");
            },
            pageSubtitle() {
                return this.activeTab === "context-dossiers"
                    ? this.$t("contextDossiers.subtitle")
                    : this.$t("documentsHub.subtitle");
            },
        },
        methods: {
            applyRouteTab() {
                const activeTab = this.$route.query.tab;
                if (this.tabsList.some((tab) => tab.name === activeTab)) {
                    this.activeTab = activeTab;
                    this.$refs.TabsComponent?.setActiveTab(activeTab);
                }
            },
            onTabSelected(tabName) {
                this.activeTab = tabName;
                if (this.$route.query.tab === tabName) return;
                this.$router.replace({ query: { ...this.$route.query, tab: tabName } });
            },
        },
    };
</script>
<style>
    .scroll-area {
        display: list-item;
        overflow-y: auto;
    }
</style>
