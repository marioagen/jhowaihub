<template>
    <main>
        <div class="container-fluid mt-4">
            <div>
                <div class="row align-items-center mb-4">
                    <div class="col-8 d-flex align-items-center">
                        <button
                            class="btn btn-outline-primary btn-table btn-sm me-2"
                            @click="goBack"
                            type="button"
                        >
                            <LucideIcon icon="ArrowLeft" />
                        </button>
                        <div>
                            <h4 class="fw-bold mb-0">
                                {{ $t("analyze.title") }}
                            </h4>
                            <div class="text-muted small">
                                {{ $t("analyze.subtitle") }}
                            </div>
                        </div>
                    </div>
                </div>
                <div class="d-flex align-items-center mt-1">
                    <span class="badge bg-light text-dark">
                        <i class="fas fa-project-diagram me-1 text-primary"></i>
                        {{ workflowName }}
                    </span>
                    /
                    <span class="badge bg-light text-dark">
                        <i class="fas fa-file-alt me-1 text-primary"></i>
                        {{ documentName }}
                    </span>
                    <div
                        class="btn-group-sm margin-left"
                        role="group"
                    >
                        <input
                            type="radio"
                            class="btn-check"
                            name="view"
                            id="view-doc"
                            autocomplete="off"
                            v-model="viewMode"
                            value="doc"
                        />
                        <label
                            class="btn btn-outline-primary"
                            for="view-doc"
                        >
                            <LucideIcon icon="PanelLeft" />
                        </label>

                        <input
                            type="radio"
                            class="btn-check ms-2"
                            name="view"
                            id="view-both"
                            autocomplete="off"
                            v-model="viewMode"
                            value="both"
                        />
                        <label
                            class="btn btn-outline-primary"
                            for="view-both"
                        >
                            <LucideIcon icon="Columns2" />
                        </label>

                        <input
                            type="radio"
                            class="btn-check"
                            name="view"
                            id="view-history"
                            autocomplete="off"
                            v-model="viewMode"
                            value="history"
                        />
                        <label
                            class="btn btn-outline-primary"
                            for="view-history"
                        >
                            <LucideIcon icon="PanelRight" />
                        </label>
                    </div>
                </div>
                <div class="row">
                    <PromptViewer
                        :hashDocument="hashDocument"
                        :historyListOrder="historyListOrder"
                        @showHistory="showHistory"
                        @unshiftHistoryList="unshiftHistoryList"
                        @pushHistoryList="pushHistoryList"
                        v-if="!isExpandedHistory"
                    />

                    <DocumentViewer
                        @showNormalize="normalize"
                        id="docView"
                        v-if="viewMode === 'doc' || viewMode === 'both'"
                        :documentView="viewMode"
                    />

                    <div
                        :id="'docHistory'"
                        :class="viewMode === 'both' ? 'col-md-6' : 'col-12'"
                    >
                        <AnalysisStepsSection
                            :document-id="parseInt(documentId)"
                            :card-id="parseInt(cardId)"
                            v-if="viewMode === 'history' || viewMode === 'both'"
                        />
                    </div>
                </div>
            </div>
            <div
                v-if="isExpandedHistory"
                style="position: absolute; top: 50%"
            >
                <a
                    class="btn btn-light btn-sm shadow"
                    :title="$t('quizzes.questionnaireAndAi')"
                    @click="expandHistory"
                >
                    <img src="./../../../assets/img/prompt.png" />
                </a>
            </div>
        </div>
        <NormalizeIndex
            :docData="dataView"
            :isReprocessing="isReprocessing"
            v-if="showLoading"
        />
    </main>
</template>
<script>
    import PromptViewer from "@/components/analyze/PromptViewer.vue";
    import DocumentViewer from "@/components/analyze/DocumentViewer.vue";
    import AnalysisStepsSection from "@/components/analyze/analysisSteps/AnalysisStepsSection.vue";
    import NormalizeIndex from "@/components/documentsHub/documents/EmbeddingDocument.vue";
    import CardsServices from "@/services/cards/CardsServices";
    import AnalyzerService from "@/services/documents/AnalyzerService";
    import LogService from "@/services/log/logService";

    export default {
        name: "AnalyzerIndex",
        data() {
            return {
                documentId: this.$route.params.documentId,
                cardId: this.$route.params.cardId,
                backPage: this.$route.query.page,
                hashDocument: "",
                isExpandedHistory: false,
                historyListOrder: "desc",
                toastShow: false,
                toastColor: "",
                toastMessage: "",
                myInterval: null,
                dataShowHistory: true,
                dataUnshiftHistoryList: {},
                dataPushHistoryList: {},
                dataView: {
                    Id: parseInt(this.documentId),
                    Embeddings_model_name: "",
                },
                isReprocessing: true,
                showLoading: false,
                workflowName: "",
                documentName: "",
                viewMode: "both",
            };
        },
        components: {
            PromptViewer,
            DocumentViewer,
            AnalysisStepsSection,
            NormalizeIndex,
        },
        methods: {
            normalize(dataView, isReprocessing) {
                this.dataView = dataView;
                this.isReprocessing = isReprocessing;
                this.showLoading = true;
            },
            expandHistory() {
                this.isExpandedHistory = !this.isExpandedHistory;
            },
            updateHistoryListOrder(data) {
                this.historyListOrder = data.value;
            },
            showHistory() {
                this.dataShowHistory = !this.dataShowHistory;
            },
            unshiftHistoryList(data) {
                this.dataUnshiftHistoryList = data;
            },
            pushHistoryList(data) {
                this.dataPushHistoryList = data;
            },
            getDataDocument() {
                AnalyzerService.getAnalyzeDocument(this.documentId)
                    .then((result) => {
                        this.hashDocument = result.referenceFile;
                    })
                    .catch((error) => {
                        LogService.showMessage("Error loading document: " + error);
                    });
            },
            async getCardHeaderInfo() {
                const result = await CardsServices.findCardHeaderInfo(this.cardId);
                if (result && !result.error) {
                    this.workflowName = result.workflowName;
                    this.documentName = result.cardName;
                }
            },
            goBack() {
                if (this.backPage) {
                    this.$router.push({
                        name: "Documents",
                        query: { page: this.backPage },
                    });
                } else {
                    this.$router.back();
                }
            },
        },
        created() {
            this.getDataDocument();
            this.getCardHeaderInfo();
        },
    };
</script>
<style scoped>
    .container-fluid {
        padding: 0 13px;
    }

    @media (min-width: 320px) and (max-width: 767px) {
        #docView {
            display: none;
        }

        #docHistory {
            display: none;
        }
    }

    #docHistory {
        overflow-y: auto;
        max-height: 70vh;
        min-height: 300px; /* Opcional: altura mínima para não ficar pequeno demais */
        height: auto !important;
    }
    .btn-check:checked + .btn {
        background-color: #0d6efd !important; /* azul bootstrap */
        color: white !important;
        border-color: #0d6efd !important;
    }
    .margin-left {
        margin-left: auto;
    }
</style>
