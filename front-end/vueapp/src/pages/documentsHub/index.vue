<template>
    <main>
        <div class="container-fluid scroll-area manage-user mx-2">
            <div class="row">
                <div class="col-12">
                    <h5 class="mb-0 fw-bold">
                        {{ $t("documentsHub.title") }}
                    </h5>
                    <p>{{ $t("documentsHub.subtitle") }}</p>
                    <TabsComponent
                        :tabs="tabsList"
                        color="custom"
                        ref="TabsComponent"
                    >
                        <template #workflows>
                            <WorkflowsKanbanComponent />
                        </template>
                        <template #documents>
                            <DocumentsComponent />
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

    export default {
        name: "ManagementIndex",
        components: {
            TabsComponent,
            WorkflowsKanbanComponent,
            DocumentsComponent,
        },
        data: () => ({
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
            ],
        }),
        mounted() {
            let activeTab = this.$route.query.tab;
            if (activeTab !== undefined) {
                this.$refs.TabsComponent.setActiveTab(activeTab);
            }
        },
    };
</script>
<style>
    .scroll-area {
        display: list-item;
        overflow-y: auto;
    }
</style>
