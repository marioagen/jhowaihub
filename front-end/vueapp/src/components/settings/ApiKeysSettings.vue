<template>
    <div class="api-keys-settings">
        <div class="api-keys-settings__header">
            <div>
                <h6 class="mb-1 fw-bold api-keys-settings__title">
                    {{ $t("settings.apiKeys.titleWithCount", { count: keys.length }) }}
                </h6>
                <p class="text-muted small mb-0">{{ $t("settings.apiKeys.subtitle") }}</p>
            </div>
            <button
                type="button"
                class="btn btn-primary btn-sm"
                @click="openAddModal"
            >
                <LucideIcon icon="Plus" :size="14" />
                {{ $t("settings.apiKeys.add") }}
            </button>
        </div>

        <div class="alert alert-primary small api-keys-settings__notice">
            <LucideIcon icon="Info" :size="16" />
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

        <div class="api-keys-settings__toolbar">
            <div class="input-group input-group-sm api-keys-settings__search">
                <span class="input-group-text">
                    <LucideIcon icon="Search" :size="15" />
                </span>
                <input
                    v-model="searchTerm"
                    type="search"
                    class="form-control"
                    :placeholder="$t('settings.apiKeys.search')"
                    :aria-label="$t('settings.apiKeys.search')"
                />
                <button
                    v-if="searchTerm"
                    type="button"
                    class="btn btn-outline-secondary"
                    :aria-label="$t('common.clear')"
                    @click="searchTerm = ''"
                >
                    <LucideIcon icon="X" :size="14" />
                </button>
            </div>
            <span class="api-keys-settings__result-count">
                {{ $t("settings.apiKeys.resultsCount", { count: filteredKeys.length }) }}
            </span>
        </div>

        <div
            v-if="selectedIds.length"
            class="api-keys-settings__bulk"
        >
            <span class="small text-muted">
                {{ $t("settings.apiKeys.selectedCount", { count: selectedIds.length }) }}
            </span>
            <div class="d-flex gap-2 flex-wrap">
                <button
                    v-if="canBulkRevoke"
                    type="button"
                    class="btn btn-outline-warning btn-sm"
                    @click="confirmBulkRevoke"
                >
                    <LucideIcon icon="Ban" :size="14" />
                    {{ $t("settings.apiKeys.revoke") }}
                </button>
                <button
                    type="button"
                    class="btn btn-outline-danger btn-sm"
                    @click="confirmBulkDelete"
                >
                    <LucideIcon icon="Trash2" :size="14" />
                    {{ $t("common.delete") }}
                </button>
            </div>
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
                                :disabled="!paginatedKeys.length"
                                @change="toggleSelectAll"
                            />
                        </th>
                        <th>{{ $t("settings.apiKeys.columns.name") }}</th>
                        <th>{{ $t("settings.apiKeys.columns.value") }}</th>
                        <th class="api-keys-table__date">{{ $t("settings.apiKeys.columns.createdAt") }}</th>
                        <th class="api-keys-table__date">{{ $t("settings.apiKeys.columns.lastUsedAt") }}</th>
                        <th class="api-keys-table__created-by">{{ $t("settings.apiKeys.columns.createdBy") }}</th>
                        <th class="api-keys-table__status-col">{{ $t("settings.apiKeys.columns.status") }}</th>
                        <th class="api-keys-table__actions">{{ $t("settings.apiKeys.columns.actions") }}</th>
                    </tr>
                </thead>
                <tbody>
                    <tr
                        v-for="key in paginatedKeys"
                        :key="key.id"
                        :class="{
                            'api-keys-table__row--removing': removingIds.includes(key.id),
                            'api-keys-table__row--selected': selectedIds.includes(key.id),
                            'api-keys-table__row--revoked': key.status === 'revoked',
                        }"
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
                        <td class="api-keys-table__value">
                            <span
                                class="api-keys-table__value-text"
                                :class="{ 'api-keys-table__value-text--revoked': key.status === 'revoked' }"
                            >
                                {{ maskValue(key.value) }}
                            </span>
                        </td>
                        <td class="api-keys-table__date">{{ formatDate(key.createdAt) }}</td>
                        <td class="api-keys-table__date">{{ formatDate(key.lastUsedAt) }}</td>
                        <td class="api-keys-table__created-by">{{ formatUser(key.createdBy) }}</td>
                        <td class="api-keys-table__status-col">
                            <span
                                class="api-keys-table__status"
                                :class="`api-keys-table__status--${key.status}`"
                            >
                                {{ $t(`settings.apiKeys.status.${key.status}`) }}
                            </span>
                        </td>
                        <td class="api-keys-table__actions">
                            <button
                                v-if="key.status === 'active'"
                                type="button"
                                class="btn btn-link btn-sm p-0 api-keys-table__revoke"
                                v-tooltip="$t('settings.apiKeys.revoke')"
                                :aria-label="$t('settings.apiKeys.revoke')"
                                @click="confirmRevoke(key)"
                            >
                                <LucideIcon icon="CircleSlash" :size="16" />
                            </button>
                            <button
                                v-else
                                type="button"
                                class="btn btn-link btn-sm p-0 api-keys-table__delete"
                                v-tooltip="$t('common.delete')"
                                :aria-label="$t('common.delete')"
                                @click="confirmDelete(key)"
                            >
                                <LucideIcon icon="Trash2" :size="16" />
                            </button>
                        </td>
                    </tr>
                    <tr v-if="!paginatedKeys.length">
                        <td colspan="8" class="text-center text-muted small py-4">
                            {{ $t(searchTerm ? "settings.apiKeys.noResults" : "settings.apiKeys.empty") }}
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <PaginationComponent
            v-if="filteredKeys.length"
            class="api-keys-settings__pagination"
            :current-page="currentPage"
            :total-pages="totalPages"
            :items-per-page="itemsPerPage"
            :total-items="filteredKeys.length"
            @change-page="changePage"
        />

        <ApiKeyFormModal ref="addModal" @created="onKeyCreated" />

        <ConfirmModal
            ref="revokeModal"
            id="revoke-api-key-modal"
            :is-loading="isProcessing"
            title="settings.apiKeys.revokeTitle"
            :message="revokeMessage"
            :message-params="revokeMessageParams"
            confirm-text="settings.apiKeys.revokeConfirm"
            confirm-variant="warning"
            icone-name="Ban"
            icon-variant="warning"
            @confirm="executeRevoke"
        />

        <ConfirmModal
            ref="deleteModal"
            id="delete-api-key-modal"
            :is-loading="isProcessing"
            :title="deleteTitle"
            :message="deleteMessage"
            :message-params="deleteMessageParams"
            @confirm="executeDelete"
        />
    </div>
</template>

<script>
    import ApiKeyFormModal from "@/components/settings/ApiKeyFormModal.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import PaginationComponent from "@/components/global/PaginationComponent.vue";
    import {
        API_KEY_STATUS,
        deleteApiKeys,
        loadApiKeys,
        maskApiKeyValue,
        revokeApiKeys,
    } from "@/services/settings/apiKeysSettings";
    import dateHelper from "@/helpers/date";

    export default {
        name: "ApiKeysSettings",
        components: { ApiKeyFormModal, ConfirmModal, PaginationComponent },
        data() {
            return {
                keys: [],
                selectedIds: [],
                searchTerm: "",
                currentPage: 1,
                itemsPerPage: 10,
                pendingRevokeIds: [],
                pendingDeleteIds: [],
                pendingActionName: "",
                isProcessing: false,
                removingIds: [],
            };
        },
        computed: {
            filteredKeys() {
                const search = this.searchTerm.trim().toLowerCase();
                return this.keys.filter((key) => {
                    if (!search) return true;
                    const statusLabel = this.$t(`settings.apiKeys.status.${key.status}`).toLowerCase();
                    return [
                        key.name,
                        key.status,
                        statusLabel,
                        key.createdBy,
                        this.formatUser(key.createdBy),
                        this.formatDate(key.createdAt),
                        this.formatDate(key.lastUsedAt),
                    ].some((value) => String(value || "").toLowerCase().includes(search));
                });
            },
            totalPages() {
                return Math.max(1, Math.ceil(this.filteredKeys.length / this.itemsPerPage));
            },
            paginatedKeys() {
                const start = (this.currentPage - 1) * this.itemsPerPage;
                return this.filteredKeys.slice(start, start + this.itemsPerPage);
            },
            allSelected() {
                return (
                    this.paginatedKeys.length > 0 &&
                    this.paginatedKeys.every((key) => this.selectedIds.includes(key.id))
                );
            },
            someSelected() {
                return this.selectedIds.length > 0;
            },
            selectedKeys() {
                return this.keys.filter((key) => this.selectedIds.includes(key.id));
            },
            canBulkRevoke() {
                return (
                    this.selectedKeys.length > 0 &&
                    this.selectedKeys.some((key) => key.status === API_KEY_STATUS.ACTIVE)
                );
            },
            revokeMessage() {
                return this.pendingRevokeIds.length > 1
                    ? "settings.apiKeys.revokeBulkMessage"
                    : "settings.apiKeys.revokeMessage";
            },
            revokeMessageParams() {
                return {
                    name: this.pendingActionName,
                    count: this.pendingRevokeIds.length,
                };
            },
            deleteTitle() {
                return this.pendingDeleteIds.length > 1
                    ? "settings.apiKeys.deleteBulkTitle"
                    : "settings.apiKeys.deleteTitle";
            },
            deleteMessage() {
                return this.pendingDeleteIds.length > 1
                    ? "settings.apiKeys.deleteBulkMessage"
                    : "settings.apiKeys.deleteMessage";
            },
            deleteMessageParams() {
                return {
                    name: this.pendingActionName,
                    count: this.pendingDeleteIds.length,
                };
            },
        },
        watch: {
            searchTerm() {
                this.currentPage = 1;
                this.syncSelection();
            },
            totalPages(totalPages) {
                if (this.currentPage > totalPages) {
                    this.currentPage = totalPages;
                }
            },
        },
        mounted() {
            this.load();
        },
        methods: {
            load() {
                this.keys = loadApiKeys();
                this.syncSelection();
            },
            syncSelection() {
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
                this.currentPage = 1;
                this.load();
                this.$notify({
                    title: "settings.apiKeys.title",
                    message: "settings.apiKeys.created",
                    variant: "success",
                    icon: "CircleCheck",
                });
            },
            changePage(page) {
                this.currentPage = page;
            },
            toggleSelect(id) {
                if (this.selectedIds.includes(id)) {
                    this.selectedIds = this.selectedIds.filter((item) => item !== id);
                } else {
                    this.selectedIds = [...this.selectedIds, id];
                }
            },
            toggleSelectAll(event) {
                const pageIds = this.paginatedKeys.map((key) => key.id);
                if (event.target.checked) {
                    this.selectedIds = [...new Set([...this.selectedIds, ...pageIds])];
                } else {
                    const pageIdSet = new Set(pageIds);
                    this.selectedIds = this.selectedIds.filter((id) => !pageIdSet.has(id));
                }
            },
            confirmRevoke(key) {
                this.pendingRevokeIds = [key.id];
                this.pendingActionName = key.name;
                this.$refs.revokeModal?.open();
            },
            confirmBulkRevoke() {
                if (!this.canBulkRevoke) return;
                this.pendingRevokeIds = this.selectedKeys
                    .filter((key) => key.status === API_KEY_STATUS.ACTIVE)
                    .map((key) => key.id);
                this.pendingActionName = "";
                this.$refs.revokeModal?.open();
            },
            confirmDelete(key) {
                this.pendingDeleteIds = [key.id];
                this.pendingActionName = key.name;
                this.$refs.deleteModal?.open();
            },
            confirmBulkDelete() {
                if (!this.selectedIds.length) return;
                this.pendingDeleteIds = [...this.selectedIds];
                this.pendingActionName = "";
                this.$refs.deleteModal?.open();
            },
            executeRevoke() {
                if (!this.pendingRevokeIds.length) return;
                this.isProcessing = true;
                revokeApiKeys(this.pendingRevokeIds);
                this.$refs.revokeModal?.close();
                this.selectedIds = this.selectedIds.filter((id) => !this.pendingRevokeIds.includes(id));
                this.pendingRevokeIds = [];
                this.pendingActionName = "";
                this.isProcessing = false;
                this.load();
                this.$notify({
                    title: "settings.apiKeys.title",
                    message: "settings.apiKeys.revoked",
                    variant: "warning",
                    icon: "Ban",
                });
            },
            executeDelete() {
                if (!this.pendingDeleteIds.length) return;
                this.isProcessing = true;
                const idsToRemove = [...this.pendingDeleteIds];
                deleteApiKeys(idsToRemove);
                this.removingIds = idsToRemove;
                this.$refs.deleteModal?.close();
                this.selectedIds = this.selectedIds.filter((id) => !idsToRemove.includes(id));
                this.pendingDeleteIds = [];
                this.pendingActionName = "";
                this.isProcessing = false;
                this.$notify({
                    title: "settings.apiKeys.title",
                    message: "settings.apiKeys.deleted",
                    variant: "success",
                    icon: "CircleCheck",
                });
                window.setTimeout(() => {
                    this.keys = loadApiKeys();
                    this.removingIds = [];
                    if (this.currentPage > this.totalPages) {
                        this.currentPage = this.totalPages;
                    }
                }, 220);
            },
            formatUser(value) {
                if (!value) return "—";
                if (value.includes("@")) {
                    return value.split("@")[0].replace(/\./g, " ").replace(/\b\w/g, (c) => c.toUpperCase());
                }
                return value;
            },
            formatDate(value) {
                return dateHelper.formatDate(value) || "—";
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

    .api-keys-settings__toolbar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 1rem;
        margin-bottom: 0.85rem;
    }

    .api-keys-settings__search {
        width: min(420px, 100%);
    }

    .api-keys-settings__result-count {
        color: var(--color-text-muted);
        font-size: 0.75rem;
        white-space: nowrap;
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
        overflow-x: auto;
        background: var(--color-card-content);
    }

    .api-keys-settings__pagination {
        display: flex;
        justify-content: center;
        margin-top: 1rem;
    }

    .api-keys-table {
        min-width: 920px;
    }

    .api-keys-table thead th {
        background: var(--color-bg-body-content);
        color: var(--color-text-muted);
        font-size: 0.8rem;
        font-weight: 600;
        border-bottom: 1px solid var(--color-border-form-control);
        padding: 0.7rem 0.85rem;
        vertical-align: middle;
        white-space: nowrap;
    }

    .api-keys-table tbody td {
        padding: 0.75rem 0.85rem;
        vertical-align: middle;
        border-bottom: 1px solid var(--color-border-form-control);
        color: var(--color-body-content);
        background: var(--color-card-content);
        font-size: 0.84rem;
    }

    .api-keys-table tbody tr {
        transition: opacity 0.2s ease, transform 0.2s ease, background-color 0.15s ease;
    }

    .api-keys-table tbody tr:last-child td {
        border-bottom: none;
    }

    .api-keys-table__row--selected td {
        background: rgba(9, 105, 218, 0.04);
    }

    .api-keys-table__row--revoked td {
        color: var(--color-text-muted);
    }

    .api-keys-table__row--removing {
        opacity: 0;
        transform: translateX(8px);
        pointer-events: none;
    }

    .api-keys-table__check {
        width: 40px;
    }

    .api-keys-table__actions {
        width: 56px;
        text-align: center;
        white-space: nowrap;
    }

    .api-keys-table__status-col {
        width: 90px;
    }

    .api-keys-table__name {
        font-weight: 500;
        min-width: 120px;
    }

    .api-keys-table__value {
        min-width: 180px;
    }

    .api-keys-table__value-text {
        font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
        font-size: 0.8rem;
        color: var(--color-body-content);
        letter-spacing: 0.02em;
    }

    .api-keys-table__value-text--revoked {
        opacity: 0.6;
    }

    .api-keys-table__revoke,
    .api-keys-table__delete {
        min-width: 32px;
        min-height: 32px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
    }

    .api-keys-table__revoke {
        color: #dc3545;
    }

    .api-keys-table__revoke:hover {
        color: #b02a37;
    }

    .api-keys-table__delete {
        color: var(--color-text-muted);
    }

    .api-keys-table__delete:hover {
        color: var(--bs-danger, #dc3545);
    }

    .api-keys-table__status {
        display: inline-flex;
        align-items: center;
        padding: 0.2rem 0.6rem;
        border-radius: 999px;
        font-size: 0.72rem;
        font-weight: 600;
        letter-spacing: 0.01em;
        white-space: nowrap;
    }

    .api-keys-table__status--active {
        color: #fff;
        background: #198754;
    }

    .api-keys-table__status--revoked {
        color: #fff;
        background: #495057;
    }

    .api-keys-table__created-by {
        min-width: 110px;
        white-space: nowrap;
    }

    .api-keys-table__date {
        min-width: 96px;
        white-space: nowrap;
        color: var(--color-body-content);
    }

    @media (max-width: 576px) {
        .api-keys-settings__header,
        .api-keys-settings__toolbar {
            flex-direction: column;
            align-items: stretch;
        }

        .api-keys-settings__search {
            width: 100%;
        }
    }

    @media (prefers-reduced-motion: reduce) {
        .api-keys-table tbody tr,
        .api-keys-table__row--removing {
            transition: none;
            transform: none;
        }
    }
</style>
