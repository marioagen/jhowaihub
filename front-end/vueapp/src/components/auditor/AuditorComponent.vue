<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mb-3">
                <div class="mb-3">
                    <h5 class="mb-0 fw-bold">
                        <LucideIcon
                            icon="ShieldUser"
                            :size="22"
                            class="align-text-bottom me-1"
                        />
                        Auditoria
                    </h5>
                    <p class="text-muted small mb-0">
                        Acompanhe o histórico completo de ações em documentos, esteiras e usuários
                    </p>
                </div>
                <TabsComponent
                    :tabs="tabsList"
                    color="custom"
                    :tab-bar-cols="3"
                    :compact="true"
                >
                    <template #documents>
                        <AuditorCardsSection />
                    </template>
                    <template #workflows>
                        <AuditorWorkflowsSection />
                    </template>
                    <template #users>
                        <AuditorUsersSection />
                    </template>
                </TabsComponent>
            </div>
        </div>
    </main>
</template>
<script>
    import TabsComponent from "@/components/global/TabsComponent.vue";
    import AuditorCardsSection from "@/components/auditor/cards/AuditorCardsSection.vue";
    import AuditorWorkflowsSection from "@/components/auditor/workflows/AuditorWorkflowsSection.vue";
    import AuditorUsersSection from "@/components/auditor/users/AuditorUsersSection.vue";

    export default {
        name: "AuditorComponent",
        components: {
            TabsComponent,
            AuditorCardsSection,
            AuditorWorkflowsSection,
            AuditorUsersSection,
        },
        data() {
            return {
                tabsList: [
                    { name: "documents", label: "Documentos", icon: "FileText" },
                    { name: "workflows", label: "Esteiras", icon: "Workflow" },
                    { name: "users", label: "Usuários", icon: "UsersRound" },
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
