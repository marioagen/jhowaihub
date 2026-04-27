<template>
    <ModalComponent
        id="modalDocumentAnonymizations"
        ref="ModalDocumentAnonymizations"
        :isLoading="isLoading"
    >
        <template #header>
            <div class="modal-header border-0">
                <div class="d-flex align-items-center flex-grow-1">
                    <div class="anonymization-history-modal-icon me-3">
                        <LucideIcon
                            icon="ShieldCheck"
                            :size="24"
                            class="text-success"
                        />
                    </div>
                    <div class="min-w-0">
                        <h5 class="modal-title fw-bold mb-0">
                            {{ $t("analyze.anonymizationHistoryModal.title") }}
                        </h5>
                        <p class="text-muted small mb-0 mt-1">
                            {{ versionsCount }}
                            {{
                                versionsCount === 1
                                    ? $t("analyze.anonymizationHistoryModal.versionSingular")
                                    : $t("analyze.anonymizationHistoryModal.versionPlural")
                            }}
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
            <div class="modal-body document-anonymizations-modal-body mt-1">
                <div
                    v-if="!anonymizations || anonymizations.length === 0"
                    class="text-muted text-center py-5"
                >
                    {{ $t("analyze.anonymizationHistoryModal.noAnonymizations") }}
                </div>
                <template v-else>
                    <div class="anonymizations-timeline">
                        <div
                            v-for="(anonymization, index) in anonymizations"
                            :key="index"
                            class="anonymization-item mb-3"
                            @click="openDocumentUrl(anonymization)"
                        >
                            <div class="card border-success clickable-card">
                                <div class="card-body">
                                    <div
                                        class="d-flex align-items-start justify-content-between mb-2"
                                    >
                                        <div class="d-flex align-items-center gap-2">
                                            <span
                                                v-if="index === 0"
                                                class="badge bg-success text-white"
                                            >
                                                {{
                                                    $t(
                                                        "analyze.anonymizationHistoryModal.mostRecent"
                                                    )
                                                }}
                                            </span>
                                            <h6 class="card-title fw-bold mb-0">
                                                {{
                                                    anonymization.documentName ||
                                                    $t("analyze.anonymizationHistoryModal.unnamed")
                                                }}
                                            </h6>
                                        </div>
                                    </div>
                                    <div class="card-text">
                                        <div
                                            class="d-flex align-items-center gap-2 text-muted small"
                                        >
                                            <LucideIcon
                                                icon="Calendar"
                                                :size="14"
                                            />
                                            <span>{{ formatDate(anonymization.created) }}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </template>
            </div>
        </template>
        <template #footer>
            <div class="modal-footer justify-content-between border-top-0 pt-0">
                <span class="text-muted text-sm">
                    {{ $t("analyze.anonymizationHistoryModal.clickToView") }}
                </span>
                <button
                    type="button"
                    class="btn btn-light btn-sm"
                    @click="close"
                >
                    {{ $t("common.close") }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>

<script>
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import LucideIcon from "@/components/global/LucideIcon.vue";
    import date from "@/helpers/date";

    export default {
        name: "DocumentAnonymizationsModal",
        components: {
            ModalComponent,
            LucideIcon,
        },
        data() {
            return {
                anonymizations: [],
                isLoading: false,
            };
        },
        computed: {
            versionsCount() {
                return this.anonymizations ? this.anonymizations.length : 0;
            },
        },
        methods: {
            open(anonymizations) {
                this.anonymizations = anonymizations || [];
                this.$refs.ModalDocumentAnonymizations.open();
            },
            close() {
                this.$refs.ModalDocumentAnonymizations.close();
                this.$emit("close");
            },
            formatDate(value) {
                return date.formatDateWithTime(value);
            },
            openDocumentUrl(anonymization) {
                if (anonymization.documentUrl) {
                    window.open(anonymization.documentUrl, "_blank");
                }
            },
        },
    };
</script>

<style scoped>
    .anonymization-history-modal-icon {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 32px;
        height: 32px;
    }

    .document-anonymizations-modal-body {
        max-height: 60vh;
        overflow-y: auto;
    }

    .anonymizations-timeline {
        display: flex;
        flex-direction: column;
        gap: 0.5rem;
    }

    .anonymization-item {
        animation: slideIn 0.3s ease-in-out;
        cursor: pointer;
    }

    @keyframes slideIn {
        from {
            opacity: 0;
            transform: translateY(-10px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    .card {
        border-width: 2px;
        transition: all 0.3s cubic-bezier(0.25, 0.46, 0.45, 0.94);
    }

    .clickable-card:hover {
        transform: translateY(-2px);
        box-shadow: 0 6px 16px rgba(25, 135, 84, 0.2);
        background-color: rgba(25, 135, 84, 0.02);
    }

    .anonymization-item:hover .card {
        cursor: pointer;
    }

    .badge {
        font-size: 0.75rem;
        padding: 0.35rem 0.65rem;
    }

    .card-title {
        font-size: 0.95rem;
        word-break: break-word;
    }

    .card-text {
        font-size: 0.875rem;
    }

    .modal-body {
        padding: 1rem;
    }

    .modal-footer {
        padding: 1rem;
    }
</style>
