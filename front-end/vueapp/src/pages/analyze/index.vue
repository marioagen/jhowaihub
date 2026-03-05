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
                    <div
                        v-if="documentsBatch"
                        class="input-group w-auto me-2 analyze-document-select"
                    >
                        <span class="input-group-text border-end-0 bg-white">
                            <LucideIcon
                                icon="FileText"
                                size="16"
                            />
                        </span>
                        <select
                            class="form-select form-select-sm border-start-0"
                            v-model="cardId"
                            @change="changeDocument"
                        >
                            <option
                                v-for="document in documentsBatch"
                                :key="document.cardId"
                                :value="document.cardId"
                            >
                                {{ document.documentName }}
                            </option>
                        </select>
                    </div>
                    <span class="badge bg-light text-primary">
                        <LucideIcon
                            icon="Waypoints"
                            :size="15"
                        />
                        {{ workflowName }}
                    </span>
                    /
                    <span class="badge bg-light text-primary">
                        <LucideIcon
                            icon="FileText"
                            :size="15"
                        />
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
                    <button
                        v-if="canReject"
                        class="btn btn-outline-danger btn-sm ms-2"
                        @click="openRejectModal"
                        :disabled="!workflowId"
                    >
                        <i class="fas fa-times-circle me-1"></i>
                        {{ $t("analyze.rejection.reject") }}
                    </button>
                    <button
                        v-if="isRejected"
                        class="btn btn-outline-warning btn-sm ms-2"
                        @click="openViewRejectionModal"
                    >
                        <i class="fas fa-info-circle me-1"></i>
                        {{ $t("analyze.justification.viewJustification") }}
                    </button>
                </div>
                <ResizeColumnsComponent
                    v-if="viewMode === 'both'"
                    preference-key="analyzeLeftColumnPercent"
                    :min-height="'300px'"
                >
                    <template #left>
                        <DocumentViewer
                            @showNormalize="normalize"
                            id="docView"
                            :documentView="'both'"
                            :fillContainer="true"
                        />
                    </template>
                    <template #right>
                        <div
                            id="docHistory"
                            class="analyze-doc-history"
                        >
                            <AnalysisStepsSection
                                :document-id="parseInt(documentId)"
                                :card-id="parseInt(cardId)"
                            />
                        </div>
                    </template>
                </ResizeColumnsComponent>
                <div
                    v-else
                    class="row"
                >
                    <DocumentViewer
                        @showNormalize="normalize"
                        id="docView"
                        v-if="viewMode === 'doc'"
                        :documentView="viewMode"
                    />
                    <div
                        :id="'docHistory'"
                        class="col-12"
                        v-if="viewMode === 'history'"
                    >
                        <AnalysisStepsSection
                            :document-id="parseInt(documentId)"
                            :card-id="parseInt(cardId)"
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
                    <img src="@/assets/img/prompt.png" />
                </a>
            </div>
        </div>
        <NormalizeIndex
            :docData="dataView"
            :isReprocessing="isReprocessing"
            v-if="showLoading"
        />
    </main>
    <DocumentRejectionModal
        v-if="canReject"
        ref="modalReject"
        :cardId="cardId"
        :documentId="documentId"
        @success="handleRejectSuccess"
    />
    <DocumentViewRejectionModal
        v-if="isRejected"
        ref="modalViewRejection"
    />
</template>
<script>
    import { hasPermission } from "@/utils/permissions";
    import DocumentRejectionModal from "@/components/analyze/DocumentRejectionModal.vue";
    import DocumentViewRejectionModal from "@/components/analyze/DocumentViewRejectionModal.vue";
    import ResizeColumnsComponent from "@/components/global/ResizeColumnsComponent.vue";
    import PermissionGroups from "@/constants/PermissionGroups";
    import PermissionNames from "@/constants/PermissionNames";
    import DocumentViewer from "@/components/analyze/DocumentViewer.vue";
    import AnalysisStepsSection from "@/components/analyze/analysisSteps/AnalysisStepsSection.vue";
    import NormalizeIndex from "@/components/documentsHub/documents/EmbeddingDocument.vue";
    import CardsServices from "@/services/cards/CardsServices";
    import api from "@/services/api";
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
                documentsBatch: null,
            };
        },
        components: {
            DocumentViewer,
            AnalysisStepsSection,
            NormalizeIndex,
            ResizeColumnsComponent,
            DocumentRejectionModal,
            DocumentViewRejectionModal,
        },
        computed: {
            canReject() {
                return (
                    hasPermission(PermissionGroups.Documents, PermissionNames.Reject) &&
                    this.currentStepOrder > 1
                );
            },
            isRejected() {
                return this.cardStatus?.toLowerCase() === "rejected";
            },
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
            getDataDocument: function () {
                let self = this;
                api.get("/DocumentMetadata/Analyze/" + this.documentId)
                    .then(function (result) {
                        self.hashDocument = result.data.referenceFile;
                        if (result.data && result.data.documentBatchId != null) {
                            self.getBatchDocuments(result.data.documentBatchId);
                        }
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
                    this.$router.push({
                        name: "Workflow",
                    });
                }
            },
            openRejectModal() {
                if (this.workflowId) {
                    this.$refs.modalReject.open(this.workflowId);
                }
            },
            handleRejectSuccess() {
                setTimeout(() => {
                    this.goBack();
                }, 2000);
            },
            openViewRejectionModal() {
                this.$refs.modalViewRejection.open(this.cardId);
            },
            getBatchDocuments(documentBatchId) {
                CardsServices.getCardsByBatch(documentBatchId)
                    .then((response) => {
                        if (response && !response.error) {
                            this.documentsBatch = response;
                        }
                    })
                    .catch((e) => {
                        LogService.showMessage("Error loading batch documents: " + e);
                    });
            },
            changeDocument() {
                const selectedDocument = this.documentsBatch.find(
                    (doc) => doc.cardId === this.cardId
                );

                this.$router.push({
                    name: "Analyzer",
                    params: {
                        documentId: selectedDocument.documentId,
                        cardId: selectedDocument.cardId,
                    },
                    query: { page: this.backPage },
                });
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

    .analyze-document-select {
        max-width: 300px;
    }

    @media (min-width: 320px) and (max-width: 767px) {
        #docView {
            display: none;
        }

        .analyze-doc-history,
        #docHistory {
            overflow-y: auto;
            max-height: calc(100vh - 200px);
            min-height: 300px;
            height: auto !important;
        }

        .btn-check:checked + .btn {
            background-color: #0d6efd !important;
            color: white !important;
            border-color: #0d6efd !important;
        }
        .margin-left {
            margin-left: auto;
        }
    }
</style>
