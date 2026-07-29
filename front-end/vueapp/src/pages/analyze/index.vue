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
                        <div class="d-flex align-items-center flex-wrap gap-2">
                            <div>
                                <h4 class="fw-bold mb-0">
                                    {{ $t("analyze.title") }}
                                </h4>
                                <div class="text-muted small">
                                    {{ $t("analyze.subtitle") }}
                                </div>
                            </div>
                            <div class="analyze-actions-dropdown" :class="{ open: actionsDropdownOpen }">
                                <button
                                    class="analyze-actions-btn"
                                    type="button"
                                    @click.stop="actionsDropdownOpen = !actionsDropdownOpen"
                                    :aria-expanded="actionsDropdownOpen"
                                >
                                    <LucideIcon icon="LayoutList" :size="14" class="analyze-actions-btn-icon" />
                                    <span>{{ $t("analyze.actions") }}</span>
                                    <LucideIcon icon="ChevronDown" :size="13" class="analyze-actions-chevron" />
                                </button>
                                <div class="analyze-actions-menu" @click.stop>
                                    <button
                                        class="analyze-actions-item"
                                        type="button"
                                        @click="openDocumentHistoryModal(); actionsDropdownOpen = false"
                                    >
                                        <span class="analyze-actions-item-icon">
                                            <LucideIcon icon="History" :size="14" />
                                        </span>
                                        <span>{{ $t("analyze.checkHistoric") }}</span>
                                    </button>
                                    <div class="analyze-actions-divider" />
                                    <button
                                        class="analyze-actions-item"
                                        type="button"
                                        :disabled="isExportingCsv"
                                        @click="exportToolOutputsCsv(); actionsDropdownOpen = false"
                                    >
                                        <span class="analyze-actions-item-icon analyze-actions-item-icon--export">
                                            <LucideIcon icon="Download" :size="14" />
                                        </span>
                                        <span>{{ $t("analyze.exportCsv") }}</span>
                                        <span v-if="isExportingCsv" class="analyze-actions-spinner" />
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="d-flex align-items-center my-1">
                    <div class="col-6">
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <div>
                                <div
                                    v-if="documentsBatch"
                                    class="input-group w-auto me-2 analyze-document-select"
                                >
                                    <span class="input-group-text border-end-0">
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
                            </div>
                            <div class="d-flex align-items-center gap-2">
                                <button
                                    v-if="documentAnonymizations.length > 0"
                                    class="btn btn-outline-success btn-sm"
                                    @click="openDocumentAnonymizationsModal"
                                >
                                    <LucideIcon icon="ShieldCheck" />
                                    {{ $t("analyze.anonymizations") }}
                                    <small>({{ documentAnonymizations.length }})</small>
                                    <LucideIcon
                                        icon="ChevronRight"
                                        :size="15"
                                    />
                                </button>
                                <button
                                    class="btn btn-outline-primary btn-sm"
                                    @click="openAnonymizationModal"
                                >
                                    <LucideIcon icon="ShieldCheck" />
                                    {{ $t("analyze.anonymizeDocument") }}
                                </button>
                            </div>
                        </div>
                    </div>
                    <div class="col-6 d-flex justify-content-end align-items-center">
                        <div
                            class="btn-group btn-group-sm ms-auto section-buttons"
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
                                class="btn-check"
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
                        <div
                            v-if="canReject"
                            class="d-flex align-items-center gap-2 ms-3"
                        >
                            <button
                                class="btn btn-outline-danger btn-sm analyze-reject-btn"
                                @click="openRejectModal"
                                :disabled="!workflowId"
                            >
                                <LucideIcon
                                    icon="CircleX"
                                    :size="15"
                                />
                                {{ $t("analyze.rejection.reject") }}
                            </button>
                            <button
                                class="btn btn-outline-success btn-sm analyze-approve-btn"
                                @click="approveCard"
                                :disabled="!workflowId || isAdvancing"
                            >
                                <LucideIcon
                                    v-if="!isAdvancing"
                                    icon="CircleCheck"
                                    :size="15"
                                />
                                <span
                                    v-else
                                    class="spinner-border spinner-border-sm"
                                    role="status"
                                ></span>
                                {{ $t("common.approve") }}
                            </button>
                        </div>
                        <button
                            v-if="isRejected"
                            class="btn btn-outline-warning btn-sm ms-3"
                            @click="openViewRejectionModal"
                        >
                            <i class="fas fa-info-circle me-1"></i>
                            {{ $t("analyze.justification.viewJustification") }}
                        </button>
                    </div>
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
                        <div id="docHistory">
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
        :card-ids="[parseInt(String(cardId), 10)]"
        @success="handleRejectSuccess"
    />
    <DocumentViewRejectionModal
        v-if="isRejected"
        ref="modalViewRejection"
    />
    <DocumentHistoryModal ref="documentHistoryModal" />
    <AnonymizationModal
        ref="modalAnonymization"
        :documentId="documentId"
        :cardId="cardId"
        :workflowId="workflowId"
        @success="handleAnonymizationSuccess"
    />
    <DocumentAnonymizationsModal ref="modalDocumentAnonymizations" />
</template>
<script>
    import { hasPermission } from "@/utils/permissions";
    import DocumentRejectionModal from "@/components/analyze/modals/DocumentRejectionModal.vue";
    import DocumentViewRejectionModal from "@/components/analyze/modals/DocumentViewRejectionModal.vue";
    import AnonymizationModal from "@/components/analyze/modals/AnonymizationModal.vue";
    import DocumentAnonymizationsModal from "@/components/analyze/modals/DocumentAnonymizationsModal.vue";
    import ResizeColumnsComponent from "@/components/global/ResizeColumnsComponent.vue";
    import PermissionGroups from "@/constants/PermissionGroups";
    import PermissionNames from "@/constants/PermissionNames";
    import DocumentViewer from "@/components/analyze/DocumentViewer.vue";
    import DocumentHistoryModal from "@/components/analyze/modals/DocumentHistoryModal.vue";
    import AnalysisStepsSection from "@/components/analyze/analysisSteps/AnalysisStepsSection.vue";
    import NormalizeIndex from "@/components/documentsHub/documents/EmbeddingDocument.vue";
    import CardsServices from "@/services/cards/CardsServices";
    import { downloadCsv } from "@/helpers/csvHelper";
    import LogService from "@/services/log/logService";
    import DocumentMetadataServices from "@/services/documents/DocumentMetadataServices";
    import AnonymizationServices from "@/services/anonymization/AnonymizationServices";

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
                workflowId: null,
                currentStepOrder: 0,
                cardStatus: "",
                documentAnonymizations: [],
                isAdvancing: false,
                isExportingCsv: false,
                actionsDropdownOpen: false,
            };
        },
        components: {
            DocumentViewer,
            DocumentHistoryModal,
            AnalysisStepsSection,
            NormalizeIndex,
            ResizeColumnsComponent,
            DocumentRejectionModal,
            DocumentViewRejectionModal,
            AnonymizationModal,
            DocumentAnonymizationsModal,
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
            async getDataDocument() {
                const result = await DocumentMetadataServices.getDocumentAnalyze(this.documentId);
                this.hashDocument = result.referenceFile;
            },
            async getCardHeaderInfo() {
                const result = await CardsServices.findCardHeaderInfo(this.cardId);
                if (result.error === undefined) {
                    this.workflowName = result.workflowName;
                    this.documentName = result.cardName;
                    this.workflowId = result.workflowId ?? null;
                    this.currentStepOrder = result.currentStepOrder;
                    this.cardStatus = result.statusName ?? "";
                    return result.documentBatchId ?? null;
                }
                return null;
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
            async approveCard() {
                if (!this.workflowId || this.isAdvancing) return;
                this.isAdvancing = true;
                try {
                    const params = {
                        CardId: parseInt(this.cardId),
                        NextStepOrder: this.currentStepOrder + 1,
                        WorkflowId: this.workflowId,
                    };
                    const response = await CardsServices.updateStepAndStatus(params);
                    if (response?.error !== undefined) {
                        throw new Error(response.error?.response?.data?.labelError);
                    }
                    this.$notify({
                        title: "analyze.title",
                        message: "analyze.approveSuccess",
                        variant: "success",
                        icon: "CircleCheckBig",
                    });
                    setTimeout(() => this.goBack(), 1500);
                } catch (e) {
                    this.$notify({
                        title: "common.error",
                        message: "card.errorAdvancingCard",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isAdvancing = false;
                }
            },
            openRejectModal() {
                if (this.workflowId) {
                    this.$refs.modalReject.open(this.workflowId);
                }
            },
            async handleRejectSuccess() {
                await this.getCardHeaderInfo();
                setTimeout(() => {
                    this.goBack();
                }, 2000);
            },
            async handleAnonymizationSuccess() {
                await this.getCardHeaderInfo();
            },
            openViewRejectionModal() {
                this.$refs.modalViewRejection.open(this.cardId);
            },
            async getBatchDocuments(documentBatchId) {
                try {
                    const response = await CardsServices.getCardsByBatch(documentBatchId, this.workflowId);
                    if (response && !response.error && response.length > 1) {
                        this.documentsBatch = response;
                    }
                } catch (e) {
                    LogService.showMessage("Error loading batch documents: " + e);
                }
            },
            changeDocument() {
                const selectedDocument = this.documentsBatch.find(
                    (doc) => Number(doc.cardId) === Number(this.cardId)
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
            _closeActionsDropdown() {
                this.actionsDropdownOpen = false;
            },
            openDocumentHistoryModal() {
                this.$refs.documentHistoryModal?.open(this.documentId, this.workflowId);
            },
            async exportToolOutputsCsv() {
                this.isExportingCsv = true;
                try {
                    const rows = await CardsServices.findToolOutputsForExport(this.cardId);
                    if (!Array.isArray(rows) || rows.length === 0) {
                        this.$notify({
                            title: "analyze.exportCsvTitle",
                            message: "common.noData",
                            variant: "warning",
                            icon: "AlertCircle",
                        });
                        return;
                    }
                    const t = this.$t.bind(this);
                    const columns = [
                        { key: "cardId",       header: t("analyze.csvColumns.cardId") },
                        { key: "documentName", header: t("analyze.csvColumns.documentName") },
                        { key: "stepName",     header: t("analyze.csvColumns.stepName") },
                        { key: "toolName",     header: t("analyze.csvColumns.toolName") },
                        { key: "executionDate", header: t("analyze.csvColumns.executionDate") },
                        { key: "output",       header: t("analyze.csvColumns.output") },
                    ];
                    const date = new Date().toISOString().slice(0, 10);
                    downloadCsv(rows, columns, `woopi_outputs_card_${this.cardId}_${date}`);
                } catch {
                    this.$notify({
                        title: "analyze.exportCsvTitle",
                        message: "common.error",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isExportingCsv = false;
                }
            },
            openAnonymizationModal() {
                this.$refs.modalAnonymization.open();
            },
            openDocumentAnonymizationsModal() {
                this.$refs.modalDocumentAnonymizations.open(this.documentAnonymizations);
            },
            getDocumentsAnonymizationByDocument() {
                AnonymizationServices.getDocumentAnonymizations(this.documentId).then(
                    (response) => {
                        if (response && !response.error) {
                            this.documentAnonymizations = response.data;
                        }
                    }
                );
            },
        },
        async created() {
            await this.getDataDocument();
            const documentBatchId = await this.getCardHeaderInfo();
            if (documentBatchId != null) {
                await this.getBatchDocuments(documentBatchId);
            }
            await this.getDocumentsAnonymizationByDocument();
        },
        mounted() {
            document.addEventListener("click", this._closeActionsDropdown);
        },
        beforeUnmount() {
            document.removeEventListener("click", this._closeActionsDropdown);
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

    .analyze-approve-btn,
    .analyze-reject-btn {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 6px;
        min-height: 32px;
        padding-top: 0.25rem;
        padding-bottom: 0.25rem;
        font-weight: 500;
        cursor: pointer;
        transition: background-color 0.15s ease, color 0.15s ease, box-shadow 0.15s ease;
    }

    .analyze-approve-btn {
        color: #0eaa42;
        border-color: #0eaa42;
    }

    .analyze-approve-btn:not(:disabled):hover {
        background-color: #0eaa42;
        border-color: #0eaa42;
        color: #fff;
        box-shadow: 0 2px 8px rgba(14, 170, 66, 0.3);
    }

    .analyze-reject-btn {
        color: #dc3545;
        border-color: #dc3545;
    }

    .analyze-reject-btn:not(:disabled):hover {
        background-color: #dc3545;
        border-color: #dc3545;
        color: #fff;
        box-shadow: 0 2px 8px rgba(220, 53, 69, 0.3);
    }

    .section-buttons .btn-check:checked + .btn,
    .section-buttons .btn-check:active + .btn {
        background-color: #0d6efd !important;
        color: #fff !important;
        border-color: #0d6efd !important;
    }

    .section-buttons .btn-check:focus {
        outline: none !important;
        box-shadow: none !important;
    }

    .section-buttons .btn-check:focus + .btn {
        outline: none !important;
        box-shadow: none !important;
    }

    #docHistory {
        overflow-y: auto;
        overflow-x: hidden;
        max-height: calc(100vh - 200px);
        min-height: 300px;
        height: auto !important;
        -webkit-overflow-scrolling: touch;
    }

    @media (min-width: 320px) and (max-width: 767px) {
        #docView {
            display: none;
        }

        .margin-left {
            margin-left: auto;
        }
    }

    .bg-light {
        background-color: var(--color-bg-body-content) !important;
        color: var(--color-body-content) !important;
    }

    /* ── Actions dropdown ─────────────────────────────────────────── */
    .analyze-actions-dropdown {
        position: relative;
        display: inline-flex;
    }

    .analyze-actions-btn {
        display: inline-flex;
        align-items: center;
        gap: 5px;
        padding: 0.28rem 0.65rem;
        font-size: 0.8rem;
        font-weight: 500;
        border: 1px solid var(--color-border-form-control, #dee2e6);
        border-radius: 6px;
        background: var(--bs-body-bg, #fff);
        color: var(--bs-body-color, #212529);
        cursor: pointer;
        transition: border-color 0.13s, box-shadow 0.13s, background-color 0.13s;
        white-space: nowrap;
    }

    .analyze-actions-btn:hover {
        border-color: #0d6efd;
        color: #0d6efd;
        box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.08);
    }

    .analyze-actions-dropdown.open .analyze-actions-btn {
        border-color: #0d6efd;
        color: #0d6efd;
        box-shadow: 0 0 0 3px rgba(13, 110, 253, 0.08);
    }

    .analyze-actions-btn-icon {
        opacity: 0.7;
    }

    .analyze-actions-chevron {
        opacity: 0.55;
        transition: transform 0.18s ease;
    }

    .analyze-actions-dropdown.open .analyze-actions-chevron {
        transform: rotate(180deg);
    }

    .analyze-actions-menu {
        display: none;
        position: absolute;
        top: calc(100% + 6px);
        left: 0;
        min-width: 180px;
        background: var(--bs-body-bg, #fff);
        border: 1px solid var(--color-border-form-control, #dee2e6);
        border-radius: 8px;
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.10), 0 2px 6px rgba(0, 0, 0, 0.06);
        z-index: 1050;
        overflow: hidden;
        animation: analyze-menu-pop 0.15s ease;
    }

    .analyze-actions-dropdown.open .analyze-actions-menu {
        display: block;
    }

    @keyframes analyze-menu-pop {
        from { opacity: 0; transform: translateY(-4px) scale(0.98); }
        to   { opacity: 1; transform: translateY(0) scale(1); }
    }

    .analyze-actions-item {
        display: flex;
        align-items: center;
        gap: 9px;
        width: 100%;
        padding: 0.52rem 0.85rem;
        font-size: 0.82rem;
        font-weight: 450;
        border: none;
        background: transparent;
        color: var(--bs-body-color, #212529);
        cursor: pointer;
        text-align: left;
        transition: background-color 0.1s, color 0.1s;
    }

    .analyze-actions-item:hover:not(:disabled) {
        background-color: var(--bs-tertiary-bg, rgba(0, 0, 0, 0.04));
    }

    .analyze-actions-item:disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }

    .analyze-actions-item-icon {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 24px;
        height: 24px;
        border-radius: 5px;
        background-color: var(--bs-secondary-bg, rgba(0, 0, 0, 0.06));
        color: var(--bs-secondary-color, #6c757d);
        flex-shrink: 0;
    }

    .analyze-actions-item-icon--export {
        background-color: rgba(25, 135, 84, 0.1);
        color: #198754;
    }

    .analyze-actions-divider {
        height: 1px;
        background-color: var(--color-border-form-control, #dee2e6);
        margin: 0.2rem 0;
    }

    .analyze-actions-spinner {
        display: inline-block;
        width: 12px;
        height: 12px;
        border: 2px solid rgba(25, 135, 84, 0.3);
        border-top-color: #198754;
        border-radius: 50%;
        animation: analyze-spin 0.7s linear infinite;
        margin-left: auto;
    }

    @keyframes analyze-spin {
        to { transform: rotate(360deg); }
    }
</style>
