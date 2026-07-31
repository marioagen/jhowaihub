<template>
    <div
        class="modal fade"
        tabindex="-1"
        aria-hidden="true"
        ref="modalEl"
        id="promptDependencyDeleteModal"
        aria-labelledby="promptDepDeleteLabel"
    >
        <div class="modal-dialog modal-dialog-centered modal-dep-delete">
            <div class="modal-content dep-modal-content">
                <div class="modal-header dep-modal-header">
                    <div class="dep-modal-header__icon-wrap">
                        <LucideIcon icon="TriangleAlert" :size="18" />
                    </div>
                    <h5 class="modal-title dep-modal-header__title" id="promptDepDeleteLabel">
                        {{ titleMessage }}
                    </h5>
                    <button
                        type="button"
                        class="btn-close dep-modal-header__close"
                        @click="cancel"
                        :aria-label="$t('common.closeModal')"
                        :disabled="isDeleting"
                    />
                </div>

                <div class="modal-body dep-modal-body">
                    <p class="dep-modal-body__intro">
                        {{ introMessage }}
                    </p>

                    <ul class="dep-modal-body__list">
                        <li
                            v-for="wf in workflows"
                            :key="wf.id"
                            class="dep-modal-body__list-item"
                        >
                            <span class="dep-modal-body__list-icon">
                                <LucideIcon icon="GitBranch" :size="14" />
                            </span>
                            {{ wf.name }}
                        </li>
                    </ul>

                    <div class="dep-modal-body__warning">
                        <LucideIcon icon="OctagonAlert" :size="15" class="dep-modal-body__warning-icon" />
                        <span>{{ $t("prompts.deleteDependency.warning") }}</span>
                    </div>
                </div>

                <div class="dep-modal-footer">
                    <button
                        type="button"
                        class="btn btn-outline-secondary btn-sm"
                        @click="cancel"
                        :disabled="isDeleting"
                    >
                        {{ $t("common.cancel") }}
                    </button>
                    <button
                        type="button"
                        class="btn btn-danger btn-sm dep-modal-footer__confirm"
                        @click="$emit('confirm')"
                        :disabled="isDeleting || workflows.length === 0"
                    >
                        <span
                            v-if="isDeleting"
                            class="spinner-border spinner-border-sm me-1"
                            role="status"
                        />
                        <LucideIcon v-else icon="Trash2" :size="13" />
                        {{ $t("prompts.deleteDependency.confirmAnyway") }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: "PromptDependencyDeleteModal",
        emits: ["confirm", "cancel"],
        props: {
            workflows: {
                type: Array,
                default: () => [],
            },
            agentCount: {
                type: Number,
                default: 1,
            },
            isDeleting: {
                type: Boolean,
                default: false,
            },
        },
        computed: {
            isMultipleAgents() {
                return this.agentCount > 1;
            },
            titleMessage() {
                return this.isMultipleAgents
                    ? this.$t("prompts.deleteDependency.titlePlural")
                    : this.$t("prompts.deleteDependency.title");
            },
            introMessage() {
                const key = this.isMultipleAgents
                    ? "prompts.deleteDependency.introPlural"
                    : "prompts.deleteDependency.intro";
                return this.$t(key, {
                    agentCount: this.agentCount,
                    count: this.workflows.length,
                });
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
        },
    };
</script>

<style scoped>
    .modal-dep-delete {
        max-width: 480px;
    }

    .dep-modal-content {
        border: none;
        border-radius: 12px;
        overflow: hidden;
        box-shadow: 0 8px 32px rgba(0, 0, 0, 0.18);
    }

    .dep-modal-header {
        display: flex;
        align-items: center;
        gap: 0.6rem;
        padding: 1rem 1.25rem;
        border-bottom: 1px solid rgba(220, 53, 69, 0.18);
        background: rgba(220, 53, 69, 0.04);
    }

    .dep-modal-header__icon-wrap {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 32px;
        height: 32px;
        border-radius: 8px;
        background: rgba(220, 53, 69, 0.12);
        color: #dc3545;
        flex-shrink: 0;
    }

    .dep-modal-header__title {
        font-size: 0.95rem;
        font-weight: 600;
        margin: 0;
        flex: 1;
        color: var(--bs-body-color, #212529);
    }

    .dep-modal-header__close {
        flex-shrink: 0;
    }

    .dep-modal-body {
        padding: 1.25rem;
    }

    .dep-modal-body__intro {
        font-size: 0.875rem;
        color: var(--bs-body-color, #495057);
        margin-bottom: 0.85rem;
    }

    .dep-modal-body__list {
        list-style: none;
        padding: 0;
        margin: 0 0 1rem;
        display: flex;
        flex-direction: column;
        gap: 0.4rem;
        max-height: 160px;
        overflow-y: auto;
    }

    .dep-modal-body__list-item {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        font-size: 0.875rem;
        font-weight: 500;
        padding: 0.45rem 0.75rem;
        border-radius: 7px;
        background: var(--bs-tertiary-bg, #f8f9fa);
        border: 1px solid var(--bs-border-color, #dee2e6);
        color: var(--bs-body-color, #212529);
    }

    .dep-modal-body__list-icon {
        color: #6c757d;
        display: flex;
        align-items: center;
        flex-shrink: 0;
    }

    .dep-modal-body__warning {
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

    .dep-modal-body__warning-icon {
        flex-shrink: 0;
        margin-top: 1px;
        color: #e6a817;
    }

    .dep-modal-footer {
        display: flex;
        justify-content: flex-end;
        gap: 0.5rem;
        padding: 0.9rem 1.25rem;
        border-top: 1px solid var(--bs-border-color, #dee2e6);
    }

    .dep-modal-footer__confirm {
        display: flex;
        align-items: center;
        gap: 0.35rem;
    }
</style>
