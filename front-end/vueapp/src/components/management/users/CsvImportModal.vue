<template>
    <div
        id="csv-import-modal"
        class="modal fade"
        tabindex="-1"
        ref="modalEl"
        aria-hidden="true"
    >
        <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
            <div class="modal-content csv-modal">
                <div class="csv-modal__header">
                    <div class="d-flex align-items-center justify-content-between mb-3">
                        <h5 class="mb-0 fw-bold csv-modal__title">
                            <LucideIcon icon="FileSpreadsheet" :size="18" class="me-2" />
                            {{ $t("management.users.csvImport.title") }}
                        </h5>
                        <button
                            type="button"
                            class="btn-close"
                            :aria-label="$t('common.closeModal')"
                            :disabled="importing"
                            @click="close"
                        />
                    </div>

                    <div class="csv-steps">
                        <div class="csv-steps__item">
                            <div
                                class="csv-steps__node"
                                :class="{
                                    'csv-steps__node--active': step === 1,
                                    'csv-steps__node--done': step > 1,
                                }"
                            >
                                <LucideIcon v-if="step > 1" icon="Check" :size="13" />
                                <span v-else>1</span>
                            </div>
                            <span
                                class="csv-steps__label"
                                :class="{ 'csv-steps__label--active': step === 1 }"
                            >
                                {{ $t("management.users.csvImport.stepUpload") }}
                            </span>
                        </div>

                        <div class="csv-steps__line" :class="{ 'csv-steps__line--done': step > 1 }" />

                        <div class="csv-steps__item">
                            <div
                                class="csv-steps__node"
                                :class="{
                                    'csv-steps__node--active': step === 2,
                                    'csv-steps__node--done': step > 2,
                                }"
                            >
                                <LucideIcon v-if="step > 2" icon="Check" :size="13" />
                                <span v-else>2</span>
                            </div>
                            <span
                                class="csv-steps__label"
                                :class="{ 'csv-steps__label--active': step === 2 }"
                            >
                                {{ $t("management.users.csvImport.stepPreview") }}
                            </span>
                        </div>

                        <div class="csv-steps__line" :class="{ 'csv-steps__line--done': step > 2 }" />

                        <div class="csv-steps__item">
                            <div
                                class="csv-steps__node"
                                :class="{ 'csv-steps__node--active': step === 3 }"
                            >
                                <span>3</span>
                            </div>
                            <span
                                class="csv-steps__label"
                                :class="{ 'csv-steps__label--active': step === 3 }"
                            >
                                {{ $t("management.users.csvImport.stepConfirm") }}
                            </span>
                        </div>
                    </div>
                </div>

                <div class="modal-body csv-modal__body">
                    <!-- ── Step 1: Upload ─────────────────── -->
                    <template v-if="step === 1">
                        <div
                            class="csv-dropzone"
                            :class="{
                                'csv-dropzone--over': isDragging,
                                'csv-dropzone--has-file': !!file,
                            }"
                            role="button"
                            tabindex="0"
                            :aria-label="$t('management.users.csvImport.dropzoneText')"
                            @dragover.prevent="isDragging = true"
                            @dragleave.prevent="isDragging = false"
                            @drop.prevent="onDrop"
                            @click="$refs.fileInput.click()"
                            @keydown.enter="$refs.fileInput.click()"
                            @keydown.space.prevent="$refs.fileInput.click()"
                        >
                            <input
                                ref="fileInput"
                                type="file"
                                accept=".csv"
                                class="d-none"
                                @change="onFileSelected"
                            />

                            <template v-if="!file">
                                <div class="csv-dropzone__icon-wrap">
                                    <LucideIcon icon="FileUp" :size="36" />
                                </div>
                                <p class="csv-dropzone__text mb-1">
                                    {{ $t("management.users.csvImport.dropzoneText") }}
                                </p>
                                <p class="csv-dropzone__hint mb-0">
                                    {{ $t("management.users.csvImport.dropzoneHint") }}
                                </p>
                            </template>

                            <template v-else>
                                <div class="csv-dropzone__icon-wrap csv-dropzone__icon-wrap--success">
                                    <LucideIcon icon="FileCheck2" :size="36" />
                                </div>
                                <p class="csv-dropzone__filename mb-0">{{ file.name }}</p>
                                <p class="csv-dropzone__filesize text-muted mb-1">
                                    {{ (file.size / 1024).toFixed(1) }} KB
                                </p>
                                <button
                                    type="button"
                                    class="btn btn-link btn-sm csv-dropzone__change-btn"
                                    @click.stop="clearFile"
                                >
                                    {{ $t("management.users.csvImport.changeFile") }}
                                </button>
                            </template>
                        </div>

                        <div class="csv-template-row mt-3">
                            <LucideIcon icon="Download" :size="14" class="flex-shrink-0" />
                            <a
                                href="#"
                                class="csv-template-link"
                                @click.prevent="downloadTemplate"
                            >
                                {{ $t("management.users.csvImport.downloadTemplate") }}
                            </a>
                            <span class="text-muted mx-1">—</span>
                            <code class="csv-template-format text-muted">
                                {{ $t("management.users.csvImport.templateColumns") }}
                            </code>
                        </div>
                    </template>

                    <!-- ── Step 2: Preview & Validation ──── -->
                    <template v-else-if="step === 2">
                        <div class="csv-preview-summary mb-3">
                            <span class="csv-badge csv-badge--total">
                                {{ rows.length }}
                                {{ $t("management.users.csvImport.rows") }}
                            </span>
                            <span class="csv-badge csv-badge--valid">
                                <LucideIcon icon="CircleCheck" :size="12" />
                                {{ validRows.length }}
                                {{ $t("management.users.csvImport.validCount") }}
                            </span>
                            <span
                                v-if="errorRows.length"
                                class="csv-badge csv-badge--error"
                            >
                                <LucideIcon icon="CircleX" :size="12" />
                                {{ errorRows.length }}
                                {{ $t("management.users.csvImport.withErrors") }}
                            </span>
                        </div>

                        <div class="csv-preview-table-wrap">
                            <table class="table table-sm csv-preview-table mb-0">
                                <thead>
                                    <tr>
                                        <th class="csv-col-num">#</th>
                                        <th>{{ $t("common.name") }}</th>
                                        <th>{{ $t("management.users.email") }}</th>
                                        <th class="csv-col-status">
                                            {{ $t("management.users.csvImport.status") }}
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr
                                        v-for="(row, i) in rows"
                                        :key="i"
                                        :class="row.errors.length ? 'csv-row--error' : 'csv-row--valid'"
                                    >
                                        <td class="csv-col-num text-muted">{{ i + 1 }}</td>
                                        <td>
                                            <span :class="{ 'text-danger': !row.nome }">
                                                {{ row.nome || "—" }}
                                            </span>
                                        </td>
                                        <td>
                                            <span :class="{ 'text-danger': !isValidEmail(row.email) }">
                                                {{ row.email || "—" }}
                                            </span>
                                        </td>
                                        <td class="csv-col-status">
                                            <span
                                                class="csv-status-chip"
                                                :class="
                                                    row.errors.length
                                                        ? 'csv-status-chip--error'
                                                        : 'csv-status-chip--valid'
                                                "
                                            >
                                                <LucideIcon
                                                    :icon="row.errors.length ? 'CircleX' : 'CircleCheck'"
                                                    :size="11"
                                                />
                                                {{
                                                    row.errors.length
                                                        ? $t("management.users.csvImport.invalidLabel")
                                                        : $t("management.users.csvImport.validLabel")
                                                }}
                                            </span>
                                            <div
                                                v-if="row.errors.length"
                                                class="csv-row-errors"
                                            >
                                                <span
                                                    v-for="err in row.errors"
                                                    :key="err"
                                                    class="csv-row-error-item"
                                                >
                                                    {{ err }}
                                                </span>
                                            </div>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </template>

                    <!-- ── Step 3: Confirmation ───────────── -->
                    <template v-else>
                        <div class="csv-confirm">
                            <div class="csv-confirm__stats-row">
                                <div class="csv-confirm__stat">
                                    <div class="csv-confirm__num csv-confirm__num--valid">
                                        {{ validRows.length }}
                                    </div>
                                    <div class="csv-confirm__label">
                                        {{ $t("management.users.csvImport.willBeCreated") }}
                                    </div>
                                </div>
                                <div
                                    v-if="errorRows.length"
                                    class="csv-confirm__stat"
                                >
                                    <div class="csv-confirm__num csv-confirm__num--error">
                                        {{ errorRows.length }}
                                    </div>
                                    <div class="csv-confirm__label">
                                        {{ $t("management.users.csvImport.willBeIgnored") }}
                                    </div>
                                </div>
                            </div>

                            <div class="csv-confirm__bar-wrap">
                                <div class="csv-confirm__bar">
                                    <div
                                        class="csv-confirm__bar-fill"
                                        :style="{ width: validPct + '%' }"
                                    />
                                </div>
                                <div class="csv-confirm__bar-labels">
                                    <span class="csv-confirm__bar-label csv-confirm__bar-label--valid">
                                        {{ validPct }}%
                                        {{ $t("management.users.csvImport.validCount") }}
                                    </span>
                                    <span
                                        v-if="errorRows.length"
                                        class="csv-confirm__bar-label csv-confirm__bar-label--error"
                                    >
                                        {{ 100 - validPct }}%
                                        {{ $t("management.users.csvImport.withErrors") }}
                                    </span>
                                </div>
                            </div>

                            <p class="csv-confirm__hint text-muted small text-center">
                                {{ $t("management.users.csvImport.confirmHint") }}
                            </p>

                            <div
                                v-if="validRows.length === 0"
                                class="alert alert-warning d-flex align-items-center gap-2 small w-100"
                            >
                                <LucideIcon icon="TriangleAlert" :size="16" class="flex-shrink-0" />
                                {{ $t("management.users.csvImport.noValidRows") }}
                            </div>
                        </div>
                    </template>
                </div>

                <!-- ── Footer ──────────────────────────────── -->
                <div class="csv-modal__footer">
                    <button
                        v-if="step > 1"
                        type="button"
                        class="btn btn-outline-secondary btn-sm"
                        :disabled="importing"
                        @click="step--"
                    >
                        <LucideIcon icon="ChevronLeft" :size="15" />
                        {{ $t("common.back") }}
                    </button>

                    <button
                        type="button"
                        class="btn btn-outline-secondary btn-sm ms-auto"
                        :disabled="importing"
                        @click="close"
                    >
                        {{ $t("common.cancel") }}
                    </button>

                    <button
                        v-if="step === 1"
                        type="button"
                        class="btn btn-primary btn-sm"
                        :disabled="!file"
                        @click="parseAndNext"
                    >
                        {{ $t("management.users.csvImport.btnValidate") }}
                        <LucideIcon icon="ChevronRight" :size="15" />
                    </button>

                    <button
                        v-else-if="step === 2"
                        type="button"
                        class="btn btn-primary btn-sm"
                        :disabled="rows.length === 0"
                        @click="step = 3"
                    >
                        {{ $t("management.users.csvImport.btnConfirm") }}
                        <LucideIcon icon="ChevronRight" :size="15" />
                    </button>

                    <button
                        v-else
                        type="button"
                        class="btn btn-primary btn-sm"
                        :disabled="importing || validRows.length === 0"
                        @click="doImport"
                    >
                        <span
                            v-if="importing"
                            class="spinner-border spinner-border-sm me-1"
                            role="status"
                        />
                        <LucideIcon
                            v-else
                            icon="Upload"
                            :size="15"
                        />
                        {{ $t("management.users.csvImport.importBtn") }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import api from "@/services/api";

    const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    const TEMPLATE_CSV = [
        "nome,email",
        "João Silva,joao.silva@empresa.com",
        "Maria Santos,maria.santos@empresa.com",
    ].join("\r\n");

    function parseCsvRow(line) {
        const cols = [];
        let current = "";
        let inQuotes = false;
        for (let i = 0; i < line.length; i++) {
            const ch = line[i];
            if (ch === '"') {
                inQuotes = !inQuotes;
            } else if (ch === "," && !inQuotes) {
                cols.push(current.trim());
                current = "";
            } else {
                current += ch;
            }
        }
        cols.push(current.trim());
        return cols;
    }

    function parseCsv(text) {
        const lines = text
            .replace(/\r\n/g, "\n")
            .replace(/\r/g, "\n")
            .split("\n")
            .filter((l) => l.trim());

        if (lines.length < 2) return [];

        const headers = parseCsvRow(lines[0]).map((h) => h.toLowerCase().trim());

        return lines.slice(1).map((line) => {
            const values = parseCsvRow(line);
            const row = {};
            headers.forEach((h, i) => {
                row[h] = (values[i] || "").trim();
            });
            return row;
        });
    }

    export default {
        name: "CsvImportModal",
        emits: ["imported"],
        data() {
            return {
                bsModal: null,
                step: 1,
                file: null,
                isDragging: false,
                rows: [],
                importing: false,
            };
        },
        computed: {
            validRows() {
                return this.rows.filter((r) => r.errors.length === 0);
            },
            errorRows() {
                return this.rows.filter((r) => r.errors.length > 0);
            },
            validPct() {
                if (!this.rows.length) return 0;
                return Math.round((this.validRows.length / this.rows.length) * 100);
            },
        },
        mounted() {
            const Bootstrap = window.bootstrap;
            if (Bootstrap?.Modal) {
                this.bsModal = new Bootstrap.Modal(this.$refs.modalEl, {
                    backdrop: "static",
                    keyboard: false,
                });
            }
        },
        beforeUnmount() {
            this.bsModal?.dispose();
        },
        methods: {
            open() {
                this.reset();
                this.bsModal?.show();
            },
            close() {
                this.bsModal?.hide();
            },
            reset() {
                this.step = 1;
                this.file = null;
                this.rows = [];
                this.importing = false;
                this.isDragging = false;
                if (this.$refs.fileInput) {
                    this.$refs.fileInput.value = "";
                }
            },
            onDrop(e) {
                this.isDragging = false;
                const dropped = e.dataTransfer.files?.[0];
                if (dropped?.name?.endsWith(".csv")) {
                    this.file = dropped;
                }
            },
            onFileSelected(e) {
                this.file = e.target.files?.[0] || null;
            },
            clearFile() {
                this.file = null;
                if (this.$refs.fileInput) {
                    this.$refs.fileInput.value = "";
                }
            },
            downloadTemplate() {
                const blob = new Blob([TEMPLATE_CSV], { type: "text/csv;charset=utf-8;" });
                const url = URL.createObjectURL(blob);
                const anchor = document.createElement("a");
                anchor.href = url;
                anchor.download = "template-importacao-usuarios.csv";
                anchor.click();
                URL.revokeObjectURL(url);
            },
            async parseAndNext() {
                if (!this.file) return;
                const text = await this.file.text();
                const parsed = parseCsv(text);

                const seenEmails = new Set();

                this.rows = parsed.map((row) => {
                    const errors = [];

                    if (!row.nome || row.nome.length < 2) {
                        errors.push(this.$t("management.users.csvImport.errEmptyName"));
                    }

                    if (!row.email || !EMAIL_RE.test(row.email)) {
                        errors.push(this.$t("management.users.csvImport.errInvalidEmail"));
                    } else if (seenEmails.has(row.email.toLowerCase())) {
                        errors.push(this.$t("management.users.csvImport.errDuplicateEmail"));
                    } else {
                        seenEmails.add(row.email.toLowerCase());
                    }

                    return { ...row, errors };
                });

                this.step = 2;
            },
            isValidEmail(email) {
                return email && EMAIL_RE.test(email);
            },
            async doImport() {
                if (!this.validRows.length) return;
                this.importing = true;
                try {
                    await api.post("/User/BulkImport", { users: this.validRows });
                    this.$notify({
                        title: "management.users.csvImport.title",
                        message: "management.users.csvImport.successMsg",
                        variant: "success",
                        icon: "CircleCheck",
                    });
                    this.$emit("imported");
                    this.close();
                } catch {
                    this.$notify({
                        title: "management.users.csvImport.title",
                        message: "management.users.csvImport.errorMsg",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.importing = false;
                }
            },
        },
    };
</script>

<style scoped>
/* ── Modal shell ──────────────────────────────────── */
.csv-modal {
    border-radius: 0.875rem;
    overflow: hidden;
    border: none;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
}

.csv-modal__header {
    padding: 1.25rem 1.5rem 1rem;
    border-bottom: 1px solid var(--color-border-form-control);
    background: var(--color-bg-navbar);
}

.csv-modal__title {
    font-size: 0.95rem;
    display: flex;
    align-items: center;
    color: var(--color-body-content);
}

/* ── Steps ────────────────────────────────────────── */
.csv-steps {
    display: flex;
    align-items: center;
}

.csv-steps__item {
    display: flex;
    align-items: center;
    gap: 0.4rem;
    flex-shrink: 0;
}

.csv-steps__node {
    width: 26px;
    height: 26px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.75rem;
    font-weight: 700;
    border: 2px solid var(--color-border-form-control);
    color: var(--color-text-muted);
    background: var(--color-bg-navbar);
    transition: border-color 0.2s, background 0.2s, color 0.2s;
    flex-shrink: 0;
}

.csv-steps__node--active {
    border-color: var(--color-bg-btn-primary);
    color: var(--color-bg-btn-primary);
    background: color-mix(in srgb, var(--color-bg-btn-primary) 8%, var(--color-bg-navbar));
}

.csv-steps__node--done {
    border-color: var(--color-bg-btn-primary);
    background: var(--color-bg-btn-primary);
    color: #fff;
}

.csv-steps__label {
    font-size: 0.78rem;
    color: var(--color-text-muted);
    white-space: nowrap;
}

.csv-steps__label--active {
    color: var(--color-bg-btn-primary);
    font-weight: 600;
}

.csv-steps__line {
    flex: 1;
    height: 2px;
    background: var(--color-border-form-control);
    margin: 0 0.6rem;
    min-width: 20px;
    transition: background 0.25s;
}

.csv-steps__line--done {
    background: var(--color-bg-btn-primary);
}

/* ── Body ─────────────────────────────────────────── */
.csv-modal__body {
    padding: 1.5rem;
    min-height: 270px;
    background: var(--color-bg-body-content);
}

/* ── Step 1: Dropzone ─────────────────────────────── */
.csv-dropzone {
    border: 2px dashed var(--color-border-form-control);
    border-radius: 0.75rem;
    padding: 2.5rem 1.5rem;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    text-align: center;
    cursor: pointer;
    transition: border-color 0.2s, background-color 0.2s;
    background-color: var(--color-bg-navbar);
    background-image: radial-gradient(
        circle,
        var(--color-border-form-control) 1px,
        transparent 1px
    );
    background-size: 22px 22px;
    min-height: 210px;
    user-select: none;
}

.csv-dropzone:hover,
.csv-dropzone--over {
    border-color: var(--color-bg-btn-primary);
    background-color: color-mix(in srgb, var(--color-bg-btn-primary) 5%, var(--color-bg-navbar));
}

.csv-dropzone--has-file {
    border-color: #22c55e;
    border-style: solid;
    background-color: color-mix(in srgb, #22c55e 5%, var(--color-bg-navbar));
    background-image: none;
    cursor: default;
}

.csv-dropzone__icon-wrap {
    width: 64px;
    height: 64px;
    border-radius: 50%;
    background: color-mix(in srgb, var(--color-bg-btn-primary) 10%, transparent);
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--color-bg-btn-primary);
    margin-bottom: 1rem;
}

.csv-dropzone__icon-wrap--success {
    background: color-mix(in srgb, #22c55e 12%, transparent);
    color: #22c55e;
}

.csv-dropzone__text {
    font-weight: 600;
    font-size: 0.9rem;
    color: var(--color-body-content);
}

.csv-dropzone__hint {
    font-size: 0.78rem;
    color: var(--color-text-muted);
}

.csv-dropzone__filename {
    font-weight: 600;
    font-size: 0.9rem;
    color: var(--color-body-content);
}

.csv-dropzone__filesize {
    font-size: 0.78rem;
}

.csv-dropzone__change-btn {
    font-size: 0.78rem;
    padding: 0;
    cursor: pointer;
    color: var(--color-bg-btn-primary);
}

.csv-template-row {
    display: flex;
    align-items: center;
    gap: 0.35rem;
    font-size: 0.8rem;
    color: var(--color-text-muted);
    flex-wrap: wrap;
}

.csv-template-link {
    color: var(--color-bg-btn-primary);
    text-decoration: none;
    font-weight: 500;
}

.csv-template-link:hover {
    text-decoration: underline;
}

.csv-template-format {
    font-size: 0.76rem;
    background: transparent;
}

/* ── Step 2: Preview table ────────────────────────── */
.csv-preview-summary {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    flex-wrap: wrap;
}

.csv-badge {
    display: inline-flex;
    align-items: center;
    gap: 0.28rem;
    font-size: 0.76rem;
    font-weight: 600;
    padding: 0.18rem 0.55rem;
    border-radius: 999px;
}

.csv-badge--total {
    background: var(--color-bg-navbar);
    border: 1px solid var(--color-border-form-control);
    color: var(--color-body-content);
}

.csv-badge--valid {
    background: color-mix(in srgb, #22c55e 14%, transparent);
    color: #15803d;
}

.csv-badge--error {
    background: color-mix(in srgb, #ef4444 14%, transparent);
    color: #b91c1c;
}

.csv-preview-table-wrap {
    max-height: 330px;
    overflow-y: auto;
    border: 1px solid var(--color-border-form-control);
    border-radius: 0.5rem;
    background: var(--color-bg-navbar);
}

.csv-preview-table {
    font-size: 0.82rem;
    margin-bottom: 0;
}

.csv-preview-table thead th {
    position: sticky;
    top: 0;
    z-index: 1;
    background: var(--color-bg-body-content);
    font-weight: 600;
    font-size: 0.73rem;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    color: var(--color-text-muted);
    border-bottom: 2px solid var(--color-border-form-control);
    padding: 0.5rem 0.6rem;
}

.csv-preview-table tbody td {
    padding: 0.42rem 0.6rem;
    vertical-align: top;
}

.csv-row--error {
    background: color-mix(in srgb, #ef4444 5%, transparent);
}

.csv-col-num {
    width: 36px;
    text-align: center;
    font-size: 0.73rem;
}

.csv-col-status {
    width: 120px;
}

.csv-status-chip {
    display: inline-flex;
    align-items: center;
    gap: 0.22rem;
    font-size: 0.7rem;
    font-weight: 600;
    padding: 0.12rem 0.42rem;
    border-radius: 999px;
    white-space: nowrap;
}

.csv-status-chip--valid {
    background: color-mix(in srgb, #22c55e 14%, transparent);
    color: #15803d;
}

.csv-status-chip--error {
    background: color-mix(in srgb, #ef4444 14%, transparent);
    color: #b91c1c;
}

.csv-row-errors {
    display: flex;
    flex-direction: column;
    gap: 2px;
    margin-top: 3px;
}

.csv-row-error-item {
    font-size: 0.7rem;
    color: #b91c1c;
}

/* ── Step 3: Confirmation ─────────────────────────── */
.csv-confirm {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 1.25rem;
    padding: 0.5rem 0;
}

.csv-confirm__stats-row {
    display: flex;
    gap: 3rem;
    justify-content: center;
    flex-wrap: wrap;
}

.csv-confirm__stat {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.25rem;
}

.csv-confirm__num {
    font-size: 3rem;
    font-weight: 800;
    line-height: 1;
    letter-spacing: -0.04em;
}

.csv-confirm__num--valid {
    color: #22c55e;
}

.csv-confirm__num--error {
    color: #ef4444;
}

.csv-confirm__label {
    font-size: 0.82rem;
    color: var(--color-text-muted);
    font-weight: 500;
    text-align: center;
}

.csv-confirm__bar-wrap {
    width: 100%;
    max-width: 400px;
}

.csv-confirm__bar {
    width: 100%;
    height: 8px;
    border-radius: 999px;
    background: color-mix(in srgb, #ef4444 20%, var(--color-border-form-control));
    overflow: hidden;
}

.csv-confirm__bar-fill {
    height: 100%;
    background: #22c55e;
    border-radius: 999px;
    transition: width 0.6s cubic-bezier(0.25, 0.46, 0.45, 0.94);
}

.csv-confirm__bar-labels {
    display: flex;
    justify-content: space-between;
    margin-top: 0.35rem;
}

.csv-confirm__bar-label {
    font-size: 0.73rem;
    font-weight: 600;
}

.csv-confirm__bar-label--valid {
    color: #15803d;
}

.csv-confirm__bar-label--error {
    color: #b91c1c;
}

.csv-confirm__hint {
    max-width: 380px;
    line-height: 1.6;
}

/* ── Footer ───────────────────────────────────────── */
.csv-modal__footer {
    padding: 0.85rem 1.5rem;
    border-top: 1px solid var(--color-border-form-control);
    background: var(--color-bg-navbar);
    display: flex;
    align-items: center;
    gap: 0.5rem;
}
</style>
