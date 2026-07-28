<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mb-3">
                <div class="mb-3 d-flex align-items-start justify-content-between flex-wrap gap-2">
                    <div>
                        <h5 class="mb-0 fw-bold d-flex align-items-center gap-2">
                            <LucideIcon
                                icon="ShieldCheck"
                                :size="22"
                                class="text-primary"
                            />
                            {{ $t("auditor.title") }}
                        </h5>
                        <p class="text-muted small mb-0 mt-1">
                            {{ $t("auditor.subtitle") }}
                        </p>
                    </div>
                </div>
                <TabsComponent
                    :tabs="tabsList"
                    color="custom"
                    :tab-bar-cols="5"
                    :compact="true"
                >
                    <template #documents>
                        <AuditorDocumentsSection />
                    </template>
                    <template #workflows>
                        <AuditorWorkflowsSection />
                    </template>
                    <template #users>
                        <AuditorUsersSection />
                    </template>
                    <template #tools>
                        <AuditorToolsSection />
                    </template>
                    <template #system>
                        <AuditorSystemSection />
                    </template>
                </TabsComponent>
            </div>
        </div>
    </main>
</template>
<script>
    import TabsComponent from "@/components/global/TabsComponent.vue";
    import AuditorDocumentsSection from "@/components/auditor/documents/AuditorDocumentsSection.vue";
    import AuditorWorkflowsSection from "@/components/auditor/workflows/AuditorWorkflowsSection.vue";
    import AuditorUsersSection from "@/components/auditor/users/AuditorUsersSection.vue";
    import AuditorToolsSection from "@/components/auditor/tools/AuditorToolsSection.vue";
    import AuditorSystemSection from "@/components/auditor/system/AuditorSystemSection.vue";

    export default {
        name: "AuditorComponent",
        components: {
            TabsComponent,
            AuditorDocumentsSection,
            AuditorWorkflowsSection,
            AuditorUsersSection,
            AuditorToolsSection,
            AuditorSystemSection,
        },
        data() {
            return {
                tabsList: [
                    { name: "documents", label: "auditor.tabs.documents", icon: "FileText" },
                    { name: "workflows", label: "auditor.tabs.workflows", icon: "Workflow" },
                    { name: "users", label: "auditor.tabs.users", icon: "UsersRound" },
                    { name: "tools", label: "auditor.tabs.tools", icon: "Wrench" },
                    { name: "system", label: "auditor.tabs.system", icon: "ShieldAlert" },
                ],
            };
        },
    };
</script>
<style scoped>
    .scroll-area {
        overflow-y: auto;
    }
</style>
