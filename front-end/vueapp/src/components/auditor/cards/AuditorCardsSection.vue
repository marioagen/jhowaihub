<template>
    <div class="row g-3 auditor-cards-row">
        <div class="col-4">
            <div class="card rounded-3 auditor-summary-card">
                <div class="card-body d-flex flex-column auditor-summary-card-body">
                    <AuditorCardFilters
                        v-model:filter-params="filterParams"
                        :status-filter-options="statusFilterOptions"
                    />
                    <AuditorCardSummary
                        ref="summaryRef"
                        :selected-document="selectedDocument"
                        :filter-params="filterParams"
                        @select-document="onSelectDocument"
                    />
                </div>
            </div>
        </div>
        <div class="col-8">
            <div class="card rounded-3 auditor-detail-card">
                <AuditorCardDetail
                    :selected-document="selectedDocument"
                    :selected-workflow-id="selectedWorkflowId"
                    :audit-history-entries="auditHistoryEntries"
                    :stage-filter-options="stageFilterOptions"
                    v-model:selected-stage-id="selectedStageId"
                    v-model:history-search-input="historySearchInput"
                    @select-workflow="onSelectWorkflow"
                    @return-to-workflow-list="selectedWorkflowId = ''"
                />
            </div>
        </div>
    </div>
</template>
<script>
    import AuditorCardFilters from "./AuditorCardFilters.vue";
    import AuditorCardSummary from "./AuditorCardSummary.vue";
    import AuditorCardDetail from "./AuditorCardDetail.vue";

    export default {
        name: "AuditorCardsSection",
        components: {
            AuditorCardFilters,
            AuditorCardSummary,
            AuditorCardDetail,
        },
        data() {
            return {
                /** Filter params object received from Filters; when set, Summary calls getData again */
                filterParams: {
                    search: "",
                    statusId: "",
                },
                statusFilterOptions: [
                    { value: "", label: "Todos os status" },
                    { value: "ativo", label: "Ativo" },
                    { value: "finalizado", label: "Finalizado" },
                ],
                /** Current document selected in Summary; passed down to Detail */
                selectedDocument: null,
                /** When document has multiple workflows, the chosen workflow id */
                selectedWorkflowId: "",
                selectedStageId: "0",
                historySearchInput: "",
                auditHistoryByDocument: {
                    "boleto-2345": [
                        {
                            id: "h1",
                            userName: "João Ferreira",
                            actionTags: [
                                { label: "Deletar", variant: "secondary" },
                                { label: "Protocolo", variant: "secondary" },
                            ],
                            description: 'Documento deletado: "Upload duplicado"',
                            timestamp: "2024-02-08 11:25:15",
                        },
                        {
                            id: "h2",
                            userName: "João Ferreira",
                            actionTags: [
                                { label: "Upload", variant: "primary" },
                                { label: "Protocolo", variant: "secondary" },
                            ],
                            description: "Documento carregado no sistema",
                            timestamp: "2024-02-08 11:20:00",
                        },
                    ],
                    "nota-5673": [
                        {
                            id: "h3",
                            userName: "Maria Silva",
                            actionTags: [{ label: "Upload", variant: "primary" }],
                            description: "Nota fiscal registrada",
                            timestamp: "2024-02-07 14:00:00",
                        },
                    ],
                    "contrato-joao": [
                        {
                            id: "hc1",
                            userName: "Luciana Melo",
                            actionTags: [{ label: "Perguntar ao documento", variant: "primary" }],
                            description: "Pergunta enviada no fluxo Gestão de Documentos RH",
                            timestamp: "2024-02-12 10:45:00",
                        },
                        {
                            id: "hc2",
                            userName: "Dra. Mariana Costa",
                            actionTags: [{ label: "Análise", variant: "secondary" }],
                            description: "Pergunta no fluxo Análise Jurídica de Contratos",
                            timestamp: "2024-02-12 15:00:00",
                        },
                    ],
                },
            };
        },
        computed: {
            stageFilterOptions() {
                const w = this.selectedDocument?.workflows;
                if (!Array.isArray(w) || w.length === 0) return [];
                return w.map((wf) => ({
                    value: String(wf.id),
                    label: wf.name || String(wf.id),
                }));
            },
            auditHistoryEntries() {
                if (!this.selectedDocument || this.selectedDocument.cardId == null) return [];
                const list = this.auditHistoryByDocument[this.selectedDocument.cardId];
                return list || [];
            },
        },
        methods: {
            onSelectDocument(document) {
                this.selectedDocument = document;
                this.selectedWorkflowId = "";
            },
            onSelectWorkflow({ cardId, workflowId }) {
                this.selectedWorkflowId = workflowId;
                // TODO: call API with cardId and workflowId to load workflow-specific detail/history
            },
        },
        watch: {
            selectedDocument() {
                this.selectedStageId = "0";
                this.selectedWorkflowId = "";
            },
            filterParams: {
                handler(newVal, oldVal) {
                    if (oldVal && this.$refs.summaryRef) {
                        this.$refs.summaryRef.getAuditCardsSummary();
                    }
                },
                deep: true,
            },
        },
    };
</script>
<style scoped>
    .auditor-summary-card,
    .auditor-detail-card {
        height: 70vh;
        display: flex;
        flex-direction: column;
        overflow: hidden;
    }
    .auditor-summary-card .auditor-summary-card-body,
    .auditor-detail-card > * {
        flex: 1 1 0;
        min-height: 0;
        overflow: hidden;
        display: flex;
        flex-direction: column;
    }
</style>
