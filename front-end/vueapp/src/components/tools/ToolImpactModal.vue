<template>
    <div
        class="modal fade"
        tabindex="-1"
        aria-hidden="true"
        ref="modalEl"
        id="toolImpactModal"
        aria-labelledby="toolImpactModalLabel"
    >
        <div class="modal-dialog modal-dialog-centered modal-impact">
            <div class="modal-content impact-modal-content">

                <!-- Header -->
                <div class="modal-header impact-modal-header">
                    <div class="impact-modal-header__icon-wrap">
                        <LucideIcon icon="TriangleAlert" :size="18" />
                    </div>
                    <h5 class="modal-title impact-modal-header__title" id="toolImpactModalLabel">
                        {{ $t("tools.impact.title", { count: workflows.length }) }}
                    </h5>
                    <button
                        type="button"
                        class="btn-close impact-modal-header__close"
                        @click="cancel"
                        :aria-label="$t('common.closeModal')"
                        :disabled="isSaving"
                    />
                </div>

                <!-- Body -->
                <div class="modal-body impact-modal-body">
                    <p class="impact-modal-body__intro">
                        {{ $t("tools.impact.intro", { count: workflows.length }) }}
                    </p>

                    <div class="impact-modal-body__scroll-wrap">
                        <ul class="impact-modal-body__list">
                            <li
                                v-for="wf in workflows"
                                :key="wf.workflowId"
                                class="impact-modal-body__list-item"
                            >
                                <span class="impact-modal-body__list-icon">
                                    <LucideIcon icon="GitBranch" :size="14" />
                                </span>
                                <span class="impact-modal-body__list-name">{{ wf.workflowName }}</span>
                                <button
                                    type="button"
                                    class="impact-modal-body__list-link"
                                    v-tooltip="$t('tools.impact.openWorkflow')"
                                    @click.stop="openWorkflowPhase3(wf.workflowId)"
                                >
                                    <LucideIcon icon="ExternalLink" :size="13" />
                                    <span class="impact-modal-body__list-link-label">
                                        {{ $t("tools.impact.configure") }}
                                    </span>
                                </button>
                            </li>
                        </ul>
                        <div
                            v-if="workflows.length > 5"
                            class="impact-modal-body__scroll-fade"
                        />
                    </div>

                    <div class="impact-modal-body__warning">
                        <LucideIcon icon="OctagonAlert" :size="15" class="impact-modal-body__warning-icon" />
                        <span>{{ $t("tools.impact.warning") }}</span>
                    </div>
                </div>

                <!-- Footer -->
                <div class="impact-modal-footer">
                    <button
                        type="button"
                        class="btn btn-outline-secondary btn-sm"
                        @click="cancel"
                        :disabled="isSaving"
                    >
                        {{ $t("common.cancel") }}
                    </button>
                    <button
                        type="button"
                        class="btn btn-sm impact-modal-footer__confirm"
                        @click="$emit('confirm')"
                        :disabled="isSaving"
                    >
                        <span
                            v-if="isSaving"
                            class="spinner-border spinner-border-sm me-1"
                            role="status"
                        />
                        <LucideIcon v-else icon="Save" :size="13" />
                        {{ $t("tools.impact.saveAndNotify") }}
                    </button>
                </div>

            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: "ToolImpactModal",
        emits: ["confirm", "cancel"],
        props: {
            workflows: {
                type: Array,
                default: () => [],
            },
            isSaving: {
                type: Boolean,
                default: false,
            },
        },
        mounted() {
            this.modalInstance = new window.bootstrap.Modal(this.$refs.modalEl, {
                backdrop: "static",
                keyboard: false,
            });
        },
        methods: {
            open() {
                this.modalInstance?.show();
            },
            close() {
                this.modalInstance?.hide();
            },
            cancel() {
                this.close();
                this.$emit("cancel");
            },
            openWorkflowPhase3(workflowId) {
                window.open(`/workflow/edit/${workflowId}/3`, "_blank", "noopener,noreferrer");
            },
        },
    };
</script>

<style scoped>
    .modal-impact {
        max-width: 500px;
    }

    .impact-modal-content {
        border: none;
        border-radius: 12px;
        overflow: hidden;
        box-shadow: 0 8px 40px rgba(0, 0, 0, 0.18);
    }

    .impact-modal-header {
        display: flex;
        align-items: center;
        gap: 0.6rem;
        padding: 1rem 1.25rem;
        border-bottom: 1px solid rgba(255, 193, 7, 0.25);
        background: rgba(255, 193, 7, 0.06);
    }

    .impact-modal-header__icon-wrap {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 32px;
        height: 32px;
        border-radius: 8px;
        background: rgba(255, 193, 7, 0.15);
        color: #d97706;
        flex-shrink: 0;
    }

    .impact-modal-header__title {
        font-size: 0.95rem;
        font-weight: 600;
        margin: 0;
        flex: 1;
        color: var(--bs-body-color, #212529);
    }

    .impact-modal-header__close {
        flex-shrink: 0;
    }

    .impact-modal-body {
        padding: 1.25rem;
    }

    .impact-modal-body__intro {
        font-size: 0.875rem;
        color: var(--bs-body-color, #495057);
        margin-bottom: 0.75rem;
    }

    .impact-modal-body__scroll-wrap {
        position: relative;
        max-height: 230px;
        overflow: hidden;
        border-radius: 8px;
        border: 1px solid var(--bs-border-color, #dee2e6);
        margin-bottom: 1rem;
    }

    .impact-modal-body__list {
        list-style: none;
        padding: 0.35rem;
        margin: 0;
        display: flex;
        flex-direction: column;
        gap: 0.35rem;
        max-height: 228px;
        overflow-y: auto;
        scrollbar-width: thin;
        scrollbar-color: rgba(108, 117, 125, 0.35) transparent;
    }

    .impact-modal-body__list::-webkit-scrollbar {
        width: 5px;
    }

    .impact-modal-body__list::-webkit-scrollbar-track {
        background: transparent;
    }

    .impact-modal-body__list::-webkit-scrollbar-thumb {
        background: rgba(108, 117, 125, 0.35);
        border-radius: 3px;
    }

    .impact-modal-body__list-item {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.5rem 0.65rem;
        border-radius: 6px;
        background: var(--bs-tertiary-bg, #f8f9fa);
        border: 1px solid var(--bs-border-color, #e9ecef);
        color: var(--bs-body-color, #212529);
        transition: background 0.12s ease, border-color 0.12s ease;
    }

    .impact-modal-body__list-item:hover {
        background: var(--bs-secondary-bg, #e9ecef);
        border-color: #c9cdd3;
    }

    .impact-modal-body__list-icon {
        color: #6c757d;
        display: flex;
        align-items: center;
        flex-shrink: 0;
    }

    .impact-modal-body__list-name {
        flex: 1;
        font-size: 0.85rem;
        font-weight: 500;
        min-width: 0;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .impact-modal-body__list-link {
        display: inline-flex;
        align-items: center;
        gap: 0.25rem;
        flex-shrink: 0;
        padding: 0.2rem 0.5rem;
        border-radius: 5px;
        font-size: 0.75rem;
        font-weight: 500;
        color: #0d6efd;
        background: transparent;
        border: 1px solid transparent;
        cursor: pointer;
        transition: background 0.12s ease, border-color 0.12s ease, color 0.12s ease;
        white-space: nowrap;
        line-height: 1;
    }

    .impact-modal-body__list-link:hover {
        background: rgba(13, 110, 253, 0.08);
        border-color: rgba(13, 110, 253, 0.25);
        color: #0a58ca;
    }

    .impact-modal-body__list-link-label {
        font-size: 0.7rem;
        letter-spacing: 0.01em;
    }

    .impact-modal-body__scroll-fade {
        position: absolute;
        bottom: 0;
        left: 0;
        right: 0;
        height: 36px;
        background: linear-gradient(to bottom, transparent, rgba(255, 255, 255, 0.92));
        pointer-events: none;
        border-radius: 0 0 7px 7px;
    }

    .impact-modal-body__warning {
        display: flex;
        align-items: flex-start;
        gap: 0.5rem;
        padding: 0.7rem 0.9rem;
        border-radius: 8px;
        background: #fff8e1;
        border: 1px solid #ffe082;
        font-size: 0.8rem;
        line-height: 1.5;
        color: #6d4c06;
    }

    .impact-modal-body__warning-icon {
        flex-shrink: 0;
        margin-top: 1px;
        color: #e6a817;
    }

    .impact-modal-footer {
        display: flex;
        justify-content: flex-end;
        gap: 0.5rem;
        padding: 0.9rem 1.25rem;
        border-top: 1px solid var(--bs-border-color, #dee2e6);
    }

    .impact-modal-footer__confirm {
        display: flex;
        align-items: center;
        gap: 0.35rem;
        color: #ffffff;
        background-color: #ff6900;
        border-color: #ff6900;
    }

    .impact-modal-footer__confirm:hover,
    .impact-modal-footer__confirm:focus {
        color: #ffffff;
        background-color: #e65f00;
        border-color: #e65f00;
    }
</style>
