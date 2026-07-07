<template>
    <div class="api-keys-settings">
        <div class="api-keys-settings__header">
            <div>
                <h6 class="mb-1 fw-bold api-keys-settings__title">{{ $t("settings.apiKeys.title") }}</h6>
                <p class="text-muted small mb-0">{{ $t("settings.apiKeys.subtitle") }}</p>
            </div>
            <button
                type="button"
                class="btn btn-primary btn-sm"
                @click="openAddModal"
            >
                <LucideIcon
                    icon="Plus"
                    :size="14"
                />
                {{ $t("settings.apiKeys.add") }}
            </button>
        </div>

        <div class="alert alert-primary small api-keys-settings__notice">
            <LucideIcon
                icon="Info"
                :size="16"
            />
            <span>
                {{ $t("settings.apiKeys.apiUsageNotice") }}
                <a
                    href="https://api.woopi.ai"
                    target="_blank"
                    rel="noopener noreferrer"
                    class="api-keys-settings__api-link"
                >
                    https://api.woopi.ai
                </a>
            </span>
        </div>

        <div
            v-if="selectedIds.length"
            class="api-keys-settings__bulk"
        >
            <span class="small text-muted">
                {{ $t("settings.apiKeys.selectedCount", { count: selectedIds.length }) }}
            </span>
            <button
                type="button"
                class="btn btn-outline-danger btn-sm"
                @click="removeSelected"
            >
                <LucideIcon
                    icon="Trash2"
                    :size="14"
                />
                {{ $t("common.delete") }}
            </button>
        </div>

        <div class="api-keys-settings__table-wrap">
            <table class="table api-keys-table mb-0">
                <thead>
                    <tr>
                        <th class="api-keys-table__check">
                            <input
                                type="checkbox"
                                class="form-check-input"
                                :checked="allSelected"
                                :indeterminate.prop="someSelected && !allSelected"
                                @change="toggleSelectAll"
                            />
                        </th>
                        <th>{{ $t("settings.apiKeys.columns.name") }}</th>
                        <th>{{ $t("settings.apiKeys.columns.value") }}</th>
                        <th class="api-keys-table__actions"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr
                        v-for="key in keys"
                        :key="key.id"
                    >
                        <td class="api-keys-table__check">
                            <input
                                type="checkbox"
                                class="form-check-input"
                                :checked="selectedIds.includes(key.id)"
                                @change="toggleSelect(key.id)"
                            />
                        </td>
                        <td class="api-keys-table__name">{{ key.name }}</td>
                        <td>
                            <span class="api-keys-table__value-pill">
                                {{ maskValue(key.value) }}
                            </span>
                        </td>
                        <td class="api-keys-table__actions">
                            <button
                                type="button"
                                class="btn btn-link btn-sm p-0 api-keys-table__copy"
                                :title="$t('common.copy')"
                                @click="copyValue(key.value)"
                            >
                                <LucideIcon
                                    icon="Copy"
                                    :size="16"
                                />
                            </button>
                        </td>
                    </tr>
                    <tr v-if="!keys.length">
                        <td
                            colspan="4"
                            class="text-center text-muted small py-4"
                        >
                            {{ $t("settings.apiKeys.empty") }}
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <ApiKeyFormModal
            ref="addModal"
            @created="onKeyCreated"
        />
    </div>
</template>

<script>
    import ApiKeyFormModal from "@/components/settings/ApiKeyFormModal.vue";
    import {
        deleteApiKeys,
        loadApiKeys,
        maskApiKeyValue,
    } from "@/services/settings/apiKeysSettings";

    export default {
        name: "ApiKeysSettings",
        components: { ApiKeyFormModal },
        data() {
            return {
                keys: [],
                selectedIds: [],
            };
        },
        computed: {
            allSelected() {
                return this.keys.length > 0 && this.selectedIds.length === this.keys.length;
            },
            someSelected() {
                return this.selectedIds.length > 0;
            },
        },
        mounted() {
            this.load();
        },
        methods: {
            load() {
                this.keys = loadApiKeys();
                this.selectedIds = this.selectedIds.filter((id) =>
                    this.keys.some((key) => key.id === id),
                );
            },
            maskValue(value) {
                return maskApiKeyValue(value);
            },
            openAddModal() {
                this.$refs.addModal?.open();
            },
            onKeyCreated() {
                this.load();
                this.$notify({
                    title: "settings.apiKeys.title",
                    message: "settings.apiKeys.created",
                    variant: "success",
                    icon: "check",
                });
            },
            toggleSelect(id) {
                if (this.selectedIds.includes(id)) {
                    this.selectedIds = this.selectedIds.filter((item) => item !== id);
                } else {
                    this.selectedIds = [...this.selectedIds, id];
                }
            },
            toggleSelectAll(event) {
                if (event.target.checked) {
                    this.selectedIds = this.keys.map((key) => key.id);
                } else {
                    this.selectedIds = [];
                }
            },
            removeSelected() {
                if (!this.selectedIds.length) return;
                deleteApiKeys(this.selectedIds);
                this.selectedIds = [];
                this.load();
                this.$notify({
                    title: "settings.apiKeys.title",
                    message: "settings.apiKeys.deleted",
                    variant: "success",
                    icon: "check",
                });
            },
            async copyValue(value) {
                try {
                    await navigator.clipboard.writeText(value);
                    this.$notify({
                        title: "settings.apiKeys.title",
                        message: "settings.apiKeys.copied",
                        variant: "primary",
                        icon: "Copy",
                    });
                } catch {
                    this.$notify({
                        title: "settings.apiKeys.title",
                        message: "settings.apiKeys.copyFailed",
                        variant: "danger",
                        icon: "CircleX",
                    });
                }
            },
        },
    };
</script>

<style scoped>
    .api-keys-settings__header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 1rem;
        margin-bottom: 1rem;
    }

    .api-keys-settings__notice {
        display: flex;
        align-items: flex-start;
        gap: 0.5rem;
        margin-bottom: 1rem;
    }

    .api-keys-settings__api-link {
        color: inherit;
        font-weight: 600;
        text-decoration: underline;
    }

    .api-keys-settings__title {
        color: var(--color-heading-title, var(--color-body-content));
    }

    .api-keys-settings__bulk {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 0.75rem;
        margin-bottom: 0.75rem;
        padding: 0.5rem 0.75rem;
        border-radius: 8px;
        background: var(--color-bg-body-content);
        border: 1px solid var(--color-border-form-control);
    }

    .api-keys-settings__table-wrap {
        border: 1px solid var(--color-border-form-control);
        border-radius: 10px;
        overflow: hidden;
        background: var(--color-card-content);
    }

    .api-keys-table thead th {
        background: var(--color-bg-body-content);
        color: var(--color-text-muted);
        font-size: 0.8rem;
        font-weight: 600;
        border-bottom: 1px solid var(--color-border-form-control);
        padding: 0.75rem 1rem;
        vertical-align: middle;
    }

    .api-keys-table tbody td {
        padding: 0.85rem 1rem;
        vertical-align: middle;
        border-bottom: 1px solid var(--color-border-form-control);
        color: var(--color-body-content);
        background: var(--color-card-content);
    }

    .api-keys-table tbody tr:last-child td {
        border-bottom: none;
    }

    .api-keys-table__check {
        width: 44px;
    }

    .api-keys-table__actions {
        width: 48px;
        text-align: right;
    }

    .api-keys-table__name {
        font-weight: 500;
        width: 22%;
        min-width: 160px;
    }

    .api-keys-table__value-pill {
        display: block;
        width: 100%;
        max-width: none;
        padding: 0.35rem 0.65rem;
        border-radius: 6px;
        background: var(--color-bg-body-content);
        border: 1px solid var(--color-border-form-control);
        font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
        font-size: 0.78rem;
        color: var(--color-body-content);
        letter-spacing: 0.04em;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .api-keys-table__copy {
        color: var(--color-text-muted);
        min-width: 32px;
        min-height: 32px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
    }

    .api-keys-table__copy:hover {
        color: var(--color-btn-outline-primary);
    }

    @media (max-width: 576px) {
        .api-keys-settings__header {
            flex-direction: column;
            align-items: stretch;
        }
    }
</style>
