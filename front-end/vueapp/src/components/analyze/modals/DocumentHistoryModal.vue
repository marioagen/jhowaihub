<template>
    <ModalComponent
        id="documentHistoryModal"
        :isLoading="isLoading"
        ref="documentHistoryModalRef"
    >
        <template #header>
            <div class="modal-header border-0 pb-0">
                <div class="d-flex align-items-center flex-grow-1">
                    <div class="document-history-modal-icon me-3">
                        <LucideIcon
                            icon="History"
                            :size="24"
                        />
                    </div>
                    <div class="min-w-0">
                        <h5 class="modal-title fw-bold mb-0">
                            {{ $t("analyze.documentHistoryModal.title") }}
                        </h5>
                        <p class="text-muted small mb-0 mt-1">
                            {{ $t("analyze.documentHistoryModal.subtitle") }}
                        </p>
                    </div>
                </div>
                <button
                    class="btn-close"
                    data-bs-dismiss="modal"
                    @click="close"
                    :aria-label="$t('common.close')"
                />
            </div>
        </template>
        <template #body>
            <div class="modal-body document-history-modal-body mt-1">
                <div
                    v-if="!documentId || !workflowId"
                    class="text-muted text-center py-5"
                >
                    {{ $t("analyze.documentHistoryModal.noDocument") }}
                </div>
                <template v-else>
                    <div class="document-history-toolbar row g-2 align-items-center mb-3">
                        <div class="col">
                            <div class="input-group input-group-sm">
                                <span class="input-group-text border-end-0">
                                    <LucideIcon
                                        icon="Search"
                                        :size="16"
                                    />
                                </span>
                                <input
                                    v-model="searchQuery"
                                    type="text"
                                    class="form-control form-control-sm border-start-0"
                                    :placeholder="
                                        $t('analyze.documentHistoryModal.searchPlaceholder')
                                    "
                                    @keyup.enter="getDocumentHistory"
                                />
                            </div>
                        </div>
                        <div class="col-auto">
                            <button
                                type="button"
                                class="btn btn-sm border py-1 px-2 d-flex align-items-center gap-1"
                                :class="orderDescending ? 'btn-primary' : 'btn-light'"
                                @click="toggleOrderAndRefresh"
                            >
                                <LucideIcon
                                    icon="ArrowUpDown"
                                    :size="14"
                                />
                                {{
                                    orderDescending
                                        ? $t("analyze.documentHistoryModal.sortNewest")
                                        : $t("analyze.documentHistoryModal.sortOldest")
                                }}
                            </button>
                        </div>
                    </div>

                    <div class="document-history-timeline">
                        <div
                            v-if="isLoading"
                            class="text-center py-5"
                        >
                            <LoadingComponent />
                        </div>
                        <template v-else>
                            <div
                                v-if="documentHistory.length === 0"
                                class="text-muted text-center"
                            >
                                {{ $t("analyze.documentHistoryModal.noEntries") }}
                            </div>
                            <div
                                v-else
                                class="document-history-list"
                            >
                                <div
                                    v-for="(entry, index) in documentHistory"
                                    :key="index"
                                    class="document-history-card"
                                >
                                    <div
                                        class="document-history-card-content card border rounded-2 p-2"
                                    >
                                        <div class="d-flex align-items-start gap-2 flex-wrap">
                                            <BadgeComponent
                                                variant="primary"
                                                size="sm"
                                                :clickable="false"
                                                icon-only
                                            >
                                                <LucideIcon
                                                    icon="User"
                                                    :size="12"
                                                />
                                            </BadgeComponent>
                                            <div class="min-w-0 flex-grow-1">
                                                <div
                                                    class="d-flex align-items-center flex-wrap gap-1 mb-1"
                                                >
                                                    <span class="small fw-semibold">
                                                        {{ entry.userName }}
                                                    </span>
                                                    <BadgeComponent
                                                        v-if="entry.actionName"
                                                        :text="auditActionDisplay(entry).title"
                                                        variant="primary"
                                                        size="sm"
                                                        :clickable="false"
                                                    />
                                                    <span
                                                        v-if="entry.stepName"
                                                        class="small text-muted"
                                                    >
                                                        {{ entry.stepName }}
                                                    </span>
                                                </div>
                                                <div
                                                    v-if="entry.actionName"
                                                    class="small text-muted document-history-description"
                                                >
                                                    {{ auditActionDisplay(entry).action }}
                                                </div>
                                                <div
                                                    class="small text-muted d-flex align-items-center gap-1 mt-1"
                                                >
                                                    <LucideIcon
                                                        icon="Clock"
                                                        :size="12"
                                                    />
                                                    {{ formatDateWithTime(entry.created) }}
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </template>
                    </div>
                </template>
            </div>
        </template>
        <template #footer>
            <div class="modal-footer border-0 pt-3 justify-content-between flex-nowrap">
                <span class="small text-muted">
                    {{ changesCountLabel }}
                </span>
                <button
                    type="button"
                    class="btn btn-primary btn-sm px-4"
                    @click="close"
                >
                    {{ $t("analyze.documentHistoryModal.close") }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>
<script>
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import BadgeComponent from "@/components/global/BadgeComponent.vue";
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import AuditorsService from "@/services/auditors/AuditorsService";
    import dateHelper from "@/helpers/date";
    import auditActionHelper from "@/helpers/auditActionHelper";

    export default {
        name: "DocumentHistoryModal",
        components: {
            ModalComponent,
            BadgeComponent,
            LoadingComponent,
        },
        data() {
            return {
                documentId: null,
                workflowId: null,
                documentName: null,
                isLoading: false,
                searchQuery: "",
                orderDescending: true,
                documentHistory: [],
            };
        },
        computed: {
            changesCountLabel() {
                const length = this.documentHistory.length;
                return this.$t("analyze.documentHistoryModal.changesCount", { count: length });
            },
        },
        methods: {
            open(documentId, workflowId) {
                this.documentId = documentId ?? null;
                this.workflowId = workflowId ?? null;
                this.$refs.documentHistoryModalRef?.open();
                if (this.documentId && this.workflowId) {
                    this.getDocumentHistory();
                }
            },
            close() {
                this.$refs.documentHistoryModalRef?.close();
            },
            toggleOrderAndRefresh() {
                this.orderDescending = !this.orderDescending;
                this.getDocumentHistory();
            },
            async getDocumentHistory() {
                if (!this.documentId || !this.workflowId) return;
                this.isLoading = true;
                const params = {
                    take: 50,
                    orderDescending: this.orderDescending,
                    ...(this.searchQuery.trim() && { search: this.searchQuery.trim() }),
                };
                try {
                    const response = await AuditorsService.getDocumentAuditDetails(
                        this.documentId,
                        this.workflowId,
                        params
                    );
                    if (response?.error) {
                        this.documentHistory = [];
                        return;
                    }
                    const list = response?.documentHistory ?? response?.DocumentHistory ?? [];
                    this.documentHistory = Array.isArray(list) ? list : [];
                    if (response?.documentName) this.documentName = response.documentName;
                    else if (response?.DocumentName) this.documentName = response.DocumentName;
                } catch {
                    this.documentHistory = [];
                } finally {
                    this.isLoading = false;
                }
            },
            formatDateWithTime(date) {
                return dateHelper.formatDateWithTime(date) || "—";
            },
            auditActionDisplay(entry) {
                return auditActionHelper.getAuditActionDisplay(entry?.actionName, {
                    t: this.$t,
                    stepName: entry?.stepName || this.$t("auditor.users.detail.nextStep"),
                });
            },
        },
    };
</script>
<style scoped>
    .document-history-modal-icon {
        width: 48px;
        height: 48px;
        border-radius: 12px;
        background: rgba(13, 110, 253, 0.12);
        color: var(--bs-primary);
        display: inline-flex;
        align-items: center;
        justify-content: center;
    }
    .document-history-modal-body {
        max-height: 60vh;
        overflow: hidden;
        display: flex;
        flex-direction: column;
    }
    .document-history-timeline {
        flex: 1 1 auto;
        min-height: 0;
        overflow-y: auto;
    }
    .document-history-list {
        padding-left: 0;
        padding-right: 0;
    }
    .document-history-card {
        position: relative;
        padding-bottom: 0.5rem;
    }
    .document-history-card-line {
        position: absolute;
        left: 7px;
        top: 24px;
        bottom: 0;
        width: 2px;
        background: var(--bs-border-color);
        border-radius: 1px;
    }
    .document-history-card:last-child .document-history-card-line {
        display: none;
    }
    .document-history-card-content {
        margin-left: 0;
        background: var(--bs-body-bg);
    }
    .document-history-user-icon {
        color: var(--bs-secondary);
        flex-shrink: 0;
    }
</style>
