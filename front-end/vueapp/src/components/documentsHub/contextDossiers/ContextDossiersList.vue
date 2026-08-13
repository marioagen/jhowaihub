<template>
    <section class="dossiers">
        <header class="dossiers__header">
            <div>
                <h6 class="fw-bold mb-1">{{ $t("contextDossiers.title") }}</h6>
                <p class="text-muted small mb-0">{{ $t("contextDossiers.subtitle") }}</p>
            </div>
            <div class="d-flex gap-2">
                <button type="button" class="btn btn-outline-secondary btn-sm" @click="resetDemo">
                    <LucideIcon icon="RotateCcw" :size="15" />
                    {{ $t("contextDossiers.reset") }}
                </button>
                <button type="button" class="btn btn-primary btn-sm" @click="$refs.createModal.open()">
                    <LucideIcon icon="Plus" :size="15" />
                    {{ $t("contextDossiers.new") }}
                </button>
            </div>
        </header>

        <div class="dossiers__filters">
            <div class="dossiers__filter-controls">
                <div class="input-group input-group-sm dossiers__search">
                    <span class="input-group-text"><LucideIcon icon="Search" :size="15" /></span>
                    <input v-model="search" class="form-control" :placeholder="$t('contextDossiers.search')" :aria-label="$t('contextDossiers.search')" />
                </div>
                <select v-model="statusFilter" class="form-select form-select-sm dossiers__status-filter" :aria-label="$t('contextDossiers.filterStatus')">
                    <option value="">{{ $t("contextDossiers.allStatuses") }}</option>
                    <option v-for="status in statuses" :key="status" :value="status">
                        {{ statusLabel(status) }}
                    </option>
                </select>
            </div>
            <span class="dossiers__result-count">{{ $t("contextDossiers.resultsCount", { count: filteredDossiers.length }) }}</span>
        </div>

        <div class="dossiers__table-wrap">
            <table class="table dossiers__table mb-0">
                <thead><tr>
                    <th>{{ $t("contextDossiers.columns.name") }}</th>
                    <th>{{ $t("contextDossiers.columns.status") }}</th>
                    <th>{{ $t("contextDossiers.columns.files") }}</th>
                    <th>{{ $t("contextDossiers.columns.version") }}</th>
                    <th>{{ $t("contextDossiers.columns.updated") }}</th>
                    <th></th>
                </tr></thead>
                <tbody>
                    <tr v-for="dossier in paginatedDossiers" :key="dossier.id">
                        <td :data-label="$t('contextDossiers.columns.name')">
                            <button class="btn btn-link p-0 dossiers__name" @click="openDossier(dossier.id)">
                                {{ dossier.name }}
                                <LucideIcon icon="ArrowUpRight" :size="14" />
                            </button>
                            <span v-if="dossier.description" class="d-block text-muted small">{{ dossier.description }}</span>
                        </td>
                        <td :data-label="$t('contextDossiers.columns.status')"><span class="dossiers__badge" :class="`dossiers__badge--${status(dossier)}`">
                            <LucideIcon :icon="statusIcon(status(dossier))" :size="13" />
                            {{ statusLabel(status(dossier)) }}
                        </span></td>
                        <td :data-label="$t('contextDossiers.columns.files')">{{ dossier.files.length }}</td>
                        <td :data-label="$t('contextDossiers.columns.version')">{{ dossier.currentVersion ? `v${dossier.currentVersion}` : "-" }}</td>
                        <td :data-label="$t('contextDossiers.columns.updated')" class="text-muted small">{{ formatDate(dossier.updatedAt) }}</td>
                        <td class="text-end text-nowrap dossiers__actions">
                            <button class="btn btn-link btn-sm dossiers__row-action" :title="$t('contextDossiers.duplicate')" :aria-label="$t('contextDossiers.duplicate')" @click="duplicate(dossier.id)">
                                <LucideIcon icon="Copy" :size="16" />
                            </button>
                            <button class="btn btn-link btn-sm text-danger dossiers__row-action" :title="$t('common.delete')" :aria-label="$t('common.delete')" @click="remove(dossier.id)">
                                <LucideIcon icon="Trash2" :size="16" />
                            </button>
                        </td>
                    </tr>
                    <tr v-if="!filteredDossiers.length"><td colspan="6" class="text-center py-5 text-muted">
                        <LucideIcon icon="FolderSearch" :size="28" class="mb-2" />
                        <span class="d-block">{{ $t("contextDossiers.empty") }}</span>
                    </td></tr>
                </tbody>
            </table>
        </div>

        <PaginationComponent
            v-if="filteredDossiers.length"
            class="dossiers__pagination"
            :current-page="currentPage"
            :total-pages="totalPages"
            :items-per-page="itemsPerPage"
            :total-items="filteredDossiers.length"
            @change-page="changePage"
        />

        <ModalComponent ref="createModal" id="new-context-dossier" title="contextDossiers.createTitle" @save="create">
            <div class="mb-3">
                <label for="dossier-name" class="form-label">{{ $t("contextDossiers.form.name") }}</label>
                <input id="dossier-name" v-model="form.name" class="form-control" @keyup.enter="create" />
                <div v-if="showNameError" class="text-danger small mt-1">{{ $t("contextDossiers.form.nameRequired") }}</div>
            </div>
            <div>
                <label for="dossier-description" class="form-label">{{ $t("contextDossiers.form.description") }}</label>
                <textarea id="dossier-description" v-model="form.description" class="form-control" rows="3"></textarea>
            </div>
        </ModalComponent>
    </section>
</template>

<script>
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import PaginationComponent from "@/components/global/PaginationComponent.vue";
    import {
        createDossier,
        deleteDossier,
        deriveDossierStatus,
        duplicateDossier,
        loadDossiers,
        resetContextDossierDemo,
        DOSSIER_STATUS,
    } from "@/services/documents/contextDossierStorage";

    export default {
        name: "ContextDossiersList",
        components: { ModalComponent, PaginationComponent },
        data() {
            return {
                dossiers: [],
                search: "",
                statusFilter: "",
                form: { name: "", description: "" },
                showNameError: false,
                currentPage: 1,
                itemsPerPage: 5,
            };
        },
        computed: {
            statuses() { return Object.values(DOSSIER_STATUS); },
            filteredDossiers() {
                const term = this.search.trim().toLowerCase();
                return this.dossiers.filter((dossier) =>
                    (!term || dossier.name.toLowerCase().includes(term)) &&
                    (!this.statusFilter || this.status(dossier) === this.statusFilter));
            },
            totalPages() {
                return Math.max(1, Math.ceil(this.filteredDossiers.length / this.itemsPerPage));
            },
            paginatedDossiers() {
                const start = (this.currentPage - 1) * this.itemsPerPage;
                return this.filteredDossiers.slice(start, start + this.itemsPerPage);
            },
        },
        watch: {
            search() { this.currentPage = 1; },
            statusFilter() { this.currentPage = 1; },
            totalPages(totalPages) {
                if (this.currentPage > totalPages) {
                    this.currentPage = totalPages;
                }
            },
        },
        mounted() { this.reload(); },
        methods: {
            reload() { this.dossiers = loadDossiers(); },
            status: deriveDossierStatus,
            statusLabel(status) { return this.$t(`contextDossiers.status.${status}`); },
            statusIcon(status) {
                return { draft: "FilePlus2", processing: "LoaderCircle", review: "CircleAlert", ready: "CircleCheck", prepared: "BadgeCheck", stale: "RefreshCw", failed: "CircleX" }[status];
            },
            formatDate(value) { return new Intl.DateTimeFormat(this.$i18n.locale, { dateStyle: "short", timeStyle: "short" }).format(new Date(value)); },
            openDossier(id) { this.$router.push({ name: "ContextDossier", params: { id } }); },
            create() {
                this.showNameError = !this.form.name.trim();
                if (this.showNameError) return;
                const dossier = createDossier(this.form);
                this.$refs.createModal.close();
                this.form = { name: "", description: "" };
                this.openDossier(dossier.id);
            },
            duplicate(id) { duplicateDossier(id); this.reload(); },
            remove(id) { if (window.confirm(this.$t("contextDossiers.deleteConfirm"))) { deleteDossier(id); this.reload(); } },
            resetDemo() { if (window.confirm(this.$t("contextDossiers.resetConfirm"))) { resetContextDossierDemo(); this.reload(); } },
            changePage(page) { this.currentPage = page; },
        },
    };
</script>

<style scoped>
    .dossiers__header, .dossiers__filters { display: flex; align-items: flex-start; justify-content: space-between; gap: 1rem; }
    .dossiers__header { margin-bottom: 1rem; }
    .dossiers__filters { align-items: center; margin-bottom: .75rem; }
    .dossiers__filter-controls { display: flex; align-items: center; gap: .65rem; flex: 1; }
    .dossiers__search { max-width: 360px; }
    .dossiers__status-filter { max-width: 220px; }
    .dossiers__table-wrap { overflow-x: auto; border: 1px solid var(--color-border-form-control); border-radius: 8px; }
    .dossiers__table { min-width: 760px; }
    .dossiers__table th { background: var(--color-bg-body-content); color: var(--color-text-muted); font-size: .76rem; padding: .7rem .8rem; white-space: nowrap; }
    .dossiers__table th:first-child { width: 38%; min-width: 240px; }
    .dossiers__table th:nth-child(2) { width: 18%; min-width: 150px; }
    .dossiers__table th:nth-child(3), .dossiers__table th:nth-child(4) { width: 9%; text-align: center; }
    .dossiers__table th:nth-child(5) { width: 17%; min-width: 140px; }
    .dossiers__table td { background: var(--color-card-content); color: var(--color-body-content); padding: .8rem; vertical-align: middle; transition: background-color 150ms ease-out; }
    .dossiers__table tbody tr:hover td { background: color-mix(in srgb, var(--color-btn-outline-primary, #0d6efd) 4%, var(--color-card-content)); }
    .dossiers__table td:nth-child(3), .dossiers__table td:nth-child(4) { text-align: center; font-variant-numeric: tabular-nums; }
    .dossiers__name { display: inline-flex; align-items: center; gap: .3rem; max-width: 100%; color: var(--color-body-content); font-weight: 600; text-align: left; text-decoration: none; }
    .dossiers__name:hover { color: var(--color-btn-outline-primary); }
    .dossiers__result-count { color: var(--color-text-muted); font-size: .75rem; white-space: nowrap; }
    .dossiers__pagination { display: flex; justify-content: center; margin-top: 1rem; }
    .dossiers__row-action { display: inline-grid; width: 40px; height: 40px; place-items: center; padding: 0; }
    .dossiers__badge { display: inline-flex; align-items: center; gap: .3rem; border: 1px solid var(--color-border-form-control); border-radius: 999px; padding: .2rem .55rem; font-size: .72rem; white-space: nowrap; }
    .dossiers__badge--prepared, .dossiers__badge--ready { color: var(--bs-success); }
    .dossiers__badge--review, .dossiers__badge--stale { color: var(--bs-warning-text-emphasis, #9a6700); }
    .dossiers__badge--failed { color: var(--bs-danger); }
    .dossiers__badge--processing svg { animation: spin 1s linear infinite; }
    @keyframes spin { to { transform: rotate(360deg); } }
    @media (max-width: 991px) {
        .dossiers__table-wrap { overflow: visible; border: 0; background: transparent; }
        .dossiers__table { display: block; min-width: 0; }
        .dossiers__table thead { display: none; }
        .dossiers__table tbody { display: grid; gap: .75rem; }
        .dossiers__table tbody tr { display: grid; grid-template-columns: 1fr 1fr; border: 1px solid var(--color-border-form-control); border-radius: 8px; overflow: hidden; background: var(--color-card-content); }
        .dossiers__table tbody td { display: grid; grid-template-columns: minmax(92px, 38%) minmax(0, 1fr); align-items: center; min-width: 0; padding: .6rem .75rem; border-bottom: 1px solid var(--color-border-form-control); text-align: left !important; }
        .dossiers__table tbody td:first-child { grid-column: 1 / -1; display: block; padding: .8rem .75rem; }
        .dossiers__table tbody td:not(:first-child, .dossiers__actions)::before { content: attr(data-label); color: var(--color-text-muted); font-size: .7rem; font-weight: 600; }
        .dossiers__table tbody .dossiers__actions { grid-column: 1 / -1; display: flex; justify-content: flex-end; gap: .25rem; border-bottom: 0; }
        .dossiers__table tbody tr:hover td { background: var(--color-card-content); }
    }
    @media (max-width: 576px) {
        .dossiers__header, .dossiers__filters, .dossiers__filter-controls { align-items: stretch; flex-direction: column; }
        .dossiers__header > .d-flex, .dossiers__search, .dossiers__status-filter { max-width: none; width: 100%; }
        .dossiers__header .btn { flex: 1; min-height: 44px; }
        .dossiers__table tbody tr { grid-template-columns: 1fr; }
        .dossiers__table tbody td { grid-column: 1; }
        .dossiers__table tbody td:first-child, .dossiers__table tbody .dossiers__actions { grid-column: 1; }
    }
    @media (prefers-reduced-motion: reduce) { .dossiers__table td { transition: none; } }
</style>
