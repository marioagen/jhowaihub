<template>
    <section class="global-variables-settings">
        <header class="global-variables-settings__header">
            <div>
                <h6 class="global-variables-settings__title mb-1 fw-bold">
                    {{ $t("settings.globalVariables.title") }}
                </h6>
                <p class="text-muted small mb-0">{{ $t("settings.globalVariables.subtitle") }}</p>
            </div>
            <button type="button" class="btn btn-primary btn-sm" @click="openCreateModal">
                <LucideIcon icon="Plus" :size="14" />
                {{ $t("settings.globalVariables.add") }}
            </button>
        </header>

        <div class="alert alert-primary global-variables-settings__notice" role="note">
            <LucideIcon icon="Info" :size="16" />
            <span>
                {{ $t("settings.globalVariables.noticePrefix") }}
                <code>{{ globalVariableSyntax }}</code>
                {{ $t("settings.globalVariables.noticeSuffix") }}
            </span>
        </div>

        <div class="global-variables-settings__toolbar">
            <div class="input-group input-group-sm global-variables-settings__search">
                <span class="input-group-text"><LucideIcon icon="Search" :size="15" /></span>
                <input
                    v-model="searchTerm"
                    type="search"
                    class="form-control"
                    :placeholder="$t('settings.globalVariables.search')"
                    :aria-label="$t('settings.globalVariables.search')"
                />
            </div>
            <span class="global-variables-settings__result-count">
                {{ $t("settings.globalVariables.resultsCount", { count: filteredVariables.length }) }}
            </span>
        </div>

        <div class="global-variables-settings__table-wrap">
            <table class="table global-variables-table mb-0">
                <thead>
                    <tr>
                        <th>{{ $t("settings.globalVariables.columns.name") }}</th>
                        <th>{{ $t("settings.globalVariables.columns.placeholder") }}</th>
                        <th>{{ $t("settings.globalVariables.columns.value") }}</th>
                        <th>{{ $t("settings.globalVariables.columns.description") }}</th>
                        <th>{{ $t("settings.globalVariables.columns.createdBy") }}</th>
                        <th class="global-variables-table__actions"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="variable in paginatedVariables" :key="variable.id">
                        <td class="global-variables-table__name">{{ variable.name }}</td>
                        <td><code class="global-variables-table__placeholder">{{ placeholder(variable) }}</code></td>
                        <td><span class="global-variables-table__masked">••••••••••••</span></td>
                        <td class="global-variables-table__description">
                            {{ variable.description || $t("settings.globalVariables.noDescription") }}
                        </td>
                        <td class="global-variables-table__owner">
                            <span class="global-variables-table__owner-badge">
                                <LucideIcon icon="User" :size="13" />
                                {{ formatUser(variable.createdBy) }}
                            </span>
                        </td>
                        <td class="global-variables-table__actions">
                            <button
                                type="button"
                                class="global-variables-table__action"
                                :class="{ 'global-variables-table__action--locked': !canEdit(variable) }"
                                :title="$t(canEdit(variable) ? 'common.edit' : 'settings.globalVariables.editRestricted')"
                                :aria-label="$t(canEdit(variable) ? 'common.edit' : 'settings.globalVariables.editRestricted')"
                                :disabled="!canEdit(variable)"
                                @click="openEditModal(variable)"
                            >
                                <LucideIcon :icon="canEdit(variable) ? 'Pencil' : 'LockKeyhole'" :size="16" />
                            </button>
                            <button
                                type="button"
                                class="global-variables-table__action global-variables-table__action--danger"
                                :title="$t('common.delete')"
                                @click="confirmDelete(variable)"
                            >
                                <LucideIcon icon="Trash2" :size="16" />
                            </button>
                        </td>
                    </tr>
                    <tr v-if="!filteredVariables.length">
                        <td colspan="6" class="global-variables-table__empty">
                            <LucideIcon icon="Braces" :size="22" />
                            <span>{{ $t(searchTerm ? "settings.globalVariables.noResults" : "settings.globalVariables.empty") }}</span>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <PaginationComponent
            v-if="filteredVariables.length"
            class="global-variables-settings__pagination"
            :current-page="currentPage"
            :total-pages="totalPages"
            :items-per-page="itemsPerPage"
            :total-items="filteredVariables.length"
            @change-page="changePage"
        />

        <GlobalVariableFormModal ref="formModal" @saved="onSaved" />
        <ConfirmModal
            ref="deleteModal"
            id="delete-global-variable-modal"
            :is-loading="false"
            title="settings.globalVariables.deleteTitle"
            message="settings.globalVariables.deleteMessage"
            @confirm="deleteSelected"
        />
    </section>
</template>

<script>
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import PaginationComponent from "@/components/global/PaginationComponent.vue";
    import GlobalVariableFormModal from "@/components/settings/GlobalVariableFormModal.vue";
    import {
        deleteGlobalVariable,
        canEditGlobalVariable,
        loadGlobalVariables,
    } from "@/services/settings/globalVariablesSettings";

    export default {
        name: "GlobalVariablesSettings",
        components: { ConfirmModal, GlobalVariableFormModal, PaginationComponent },
        data() {
            return {
                variables: [],
                selectedVariable: null,
                searchTerm: "",
                currentPage: 1,
                itemsPerPage: 5,
            };
        },
        computed: {
            globalVariableSyntax() {
                return "{{global:nome}}";
            },
            filteredVariables() {
                const search = this.searchTerm.trim().toLowerCase();
                if (!search) return this.variables;
                return this.variables.filter((variable) =>
                    [variable.name, variable.description, variable.createdBy, this.formatUser(variable.createdBy), this.placeholder(variable)]
                        .some((value) => String(value || "").toLowerCase().includes(search)),
                );
            },
            totalPages() {
                return Math.max(1, Math.ceil(this.filteredVariables.length / this.itemsPerPage));
            },
            paginatedVariables() {
                const start = (this.currentPage - 1) * this.itemsPerPage;
                return this.filteredVariables.slice(start, start + this.itemsPerPage);
            },
        },
        watch: {
            searchTerm() {
                this.currentPage = 1;
            },
            totalPages(totalPages) {
                if (this.currentPage > totalPages) this.currentPage = totalPages;
            },
        },
        mounted() {
            this.load();
        },
        methods: {
            load() {
                this.variables = loadGlobalVariables();
            },
            placeholder(variable) {
                return `{{global:${variable.name}}}`;
            },
            openCreateModal() {
                this.$refs.formModal?.open();
            },
            openEditModal(variable) {
                if (!this.canEdit(variable)) return;
                this.$refs.formModal?.open(variable);
            },
            canEdit(variable) {
                return canEditGlobalVariable(variable);
            },
            formatUser(value) {
                if (!value) return this.$t("common.notAvailable");
                if (!value.includes("@")) return value;
                return value.split("@")[0].replace(/\./g, " ").replace(/\b\w/g, (letter) => letter.toUpperCase());
            },
            changePage(page) {
                this.currentPage = page;
            },
            confirmDelete(variable) {
                this.selectedVariable = variable;
                this.$refs.deleteModal?.open();
            },
            deleteSelected() {
                if (!this.selectedVariable) return;
                deleteGlobalVariable(this.selectedVariable.id);
                this.selectedVariable = null;
                this.$refs.deleteModal?.close();
                this.load();
                this.notify("settings.globalVariables.deleted");
            },
            onSaved(variable, wasEditing) {
                this.load();
                this.notify(
                    wasEditing
                        ? "settings.globalVariables.updated"
                        : "settings.globalVariables.created",
                );
            },
            notify(message) {
                this.$notify({
                    title: "settings.globalVariables.title",
                    message,
                    variant: "success",
                    icon: "Check",
                });
            },
        },
    };
</script>

<style scoped>
    .global-variables-settings__header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 1rem;
        margin-bottom: 1rem;
    }

    .global-variables-settings__title {
        color: var(--color-heading-title, var(--color-body-content));
    }

    .global-variables-settings__notice {
        display: flex;
        align-items: flex-start;
        gap: 0.55rem;
        margin-bottom: 1rem;
        font-size: 0.78rem;
        line-height: 1.5;
    }

    .global-variables-settings__notice svg {
        flex: 0 0 auto;
        margin-top: 0.1rem;
    }

    .global-variables-settings__notice code {
        color: inherit;
        font-weight: 600;
    }

    .global-variables-settings__toolbar {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 1rem;
        margin-bottom: 0.75rem;
    }

    .global-variables-settings__search {
        width: min(380px, 100%);
    }

    .global-variables-settings__result-count {
        color: var(--color-text-muted);
        font-size: 0.75rem;
        white-space: nowrap;
    }

    .global-variables-settings__table-wrap {
        overflow-x: auto;
        border: 1px solid var(--color-border-form-control);
        border-radius: 8px;
        background: var(--color-card-content);
    }

    .global-variables-table {
        min-width: 760px;
    }

    .global-variables-table thead th {
        padding: 0.7rem 0.75rem;
        border-bottom: 1px solid var(--color-border-form-control);
        background: var(--color-bg-body-content);
        color: var(--color-text-muted);
        font-size: 0.76rem;
        font-weight: 600;
        vertical-align: middle;
    }

    .global-variables-table tbody td {
        padding: 0.75rem;
        border-bottom: 1px solid var(--color-border-form-control);
        background: var(--color-card-content);
        color: var(--color-body-content);
        font-size: 0.8rem;
        vertical-align: middle;
    }

    .global-variables-table tbody tr:last-child td {
        border-bottom: 0;
    }

    .global-variables-table__name {
        width: 15%;
        font-weight: 500;
    }

    .global-variables-table__placeholder {
        color: #e54782;
        font-size: 0.76rem;
        white-space: nowrap;
    }

    .global-variables-table__masked {
        display: inline-block;
        min-width: 110px;
        padding: 0.25rem 0.55rem;
        border-radius: 6px;
        background: var(--color-bg-body-content);
        color: var(--color-text-muted);
        letter-spacing: 2px;
        line-height: 1;
    }

    .global-variables-table__description {
        width: 27%;
        color: var(--color-text-muted) !important;
    }

    .global-variables-table__owner {
        width: 15%;
        min-width: 135px;
    }

    .global-variables-table__owner-badge {
        display: inline-flex;
        align-items: center;
        gap: 0.35rem;
        max-width: 180px;
        padding: 0.2rem 0.5rem;
        overflow: hidden;
        border: 1px solid var(--color-border-form-control);
        border-radius: 999px;
        color: var(--color-text-muted);
        font-size: 0.72rem;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .global-variables-table__actions {
        width: 76px;
        text-align: right;
        white-space: nowrap;
    }

    .global-variables-table__action {
        display: inline-grid;
        width: 30px;
        height: 30px;
        place-items: center;
        padding: 0;
        border: 0;
        background: transparent;
        color: var(--color-btn-outline-primary, #0d6efd);
    }

    .global-variables-table__action:hover {
        background: var(--color-bg-body-content);
    }

    .global-variables-table__action--danger {
        color: var(--bs-danger, #dc3545);
    }

    .global-variables-table__action--locked {
        color: var(--color-text-muted);
        cursor: not-allowed;
        opacity: 0.55;
    }

    .global-variables-settings__pagination {
        display: flex;
        justify-content: center;
        margin-top: 1rem;
    }

    .global-variables-table__empty {
        display: table-cell;
        padding: 2rem !important;
        text-align: center;
        color: var(--color-text-muted) !important;
    }

    .global-variables-table__empty svg {
        display: block;
        margin: 0 auto 0.5rem;
    }

    @media (max-width: 576px) {
        .global-variables-settings__header {
            align-items: stretch;
            flex-direction: column;
        }
        .global-variables-settings__toolbar {
            align-items: stretch;
            flex-direction: column;
        }
    }
</style>
