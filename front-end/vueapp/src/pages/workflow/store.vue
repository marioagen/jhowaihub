<template>
    <main>
        <div class="container-fluid">
            <div class="mt-3 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <div class="d-flex align-items-center gap-3">
                        <button
                            class="btn btn-link p-0"
                            @click="goBack"
                        >
                            <LucideIcon
                                icon="ArrowLeft"
                                :size="24"
                            />
                        </button>
                        <div>
                            <h5 class="mb-0 fw-bold">
                                {{ $t("workflow.storeTitle") }}
                            </h5>
                            <p class="mb-0">
                                <small class="text-muted">
                                    {{ $t("workflow.storeSubtitle") }}
                                </small>
                            </p>
                        </div>
                    </div>
                    <div class="d-flex gap-2">
                        <button
                            class="btn btn-outline-secondary btn-sm"
                            @click="goBack"
                        >
                            {{ $t("common.cancel") }}
                        </button>
                        <button
                            class="btn btn-primary btn-sm"
                            @click="importSelected"
                            :disabled="selectedTemplates.length === 0 || importing"
                        >
                            <span
                                v-if="importing"
                                class="spinner-border spinner-border-sm me-2"
                                role="status"
                            ></span>
                            {{ $t("workflow.storeImportButton") }}
                            ({{ selectedTemplates.length }})
                        </button>
                    </div>
                </div>

                <div class="card mb-3">
                    <div class="card-body">
                        <div class="row g-3">
                            <div class="col-md-8">
                                <input
                                    type="text"
                                    class="form-control"
                                    v-model="filterQuery"
                                    :placeholder="$t('workflow.storeSearch')"
                                    @keyup.enter="loadTemplates"
                                />
                            </div>
                            <div class="col-md-4">
                                <select
                                    class="form-select"
                                    v-model="orderBy"
                                    @change="loadTemplates"
                                >
                                    <option value="created_desc">
                                        {{ $t("filters.mostRecent") }}
                                    </option>
                                    <option value="created_asc">
                                        {{ $t("filters.mostOld") }}
                                    </option>
                                    <option value="name_asc">
                                        {{ $t("filters.nameAZ") }}
                                    </option>
                                    <option value="name_desc">
                                        {{ $t("filters.nameZA") }}
                                    </option>
                                </select>
                            </div>
                        </div>
                    </div>
                </div>

                <div
                    class="mb-3"
                    v-if="filteredTemplates.length > 0"
                >
                    <div class="form-check">
                        <input
                            class="form-check-input"
                            type="checkbox"
                            id="selectAllWorkflows"
                            :checked="allSelected"
                            @change="toggleSelectAll"
                        />
                        <label
                            class="form-check-label"
                            for="selectAllWorkflows"
                        >
                            {{
                                $t("workflow.storeSelectAll").replace(
                                    "{count}",
                                    filteredTemplates.length
                                )
                            }}
                        </label>
                    </div>
                </div>

                <div
                    class="row loading-container"
                    v-if="loading"
                >
                    <div class="data-load">
                        <i class="fas fa-sync-alt fa-spin text-secondary"></i>
                        &nbsp;{{ $t("common.loading") }}..
                    </div>
                </div>

                <div
                    class="row loading-container"
                    v-if="!loading && templates.length === 0"
                >
                    <div class="data-load">
                        <i class="fas fa-exclamation-circle text-secondary"></i>
                        &nbsp;{{ $t("workflow.storeEmpty") }}
                    </div>
                </div>

                <div
                    class="row g-3"
                    v-if="!loading && filteredTemplates.length > 0"
                >
                    <div
                        v-for="template in filteredTemplates"
                        :key="template.id"
                        class="col-md-4"
                    >
                        <div
                            class="card h-100 template-card"
                            :class="{ selected: isSelected(template.id) }"
                            @click="toggleSelection(template.id)"
                        >
                            <div class="card-body">
                                <div class="d-flex align-items-start mb-2">
                                    <input
                                        class="form-check-input me-2 mt-1"
                                        type="checkbox"
                                        :checked="isSelected(template.id)"
                                        @click.stop
                                        @change="toggleSelection(template.id)"
                                    />
                                    <div class="flex-grow-1">
                                        <div class="d-flex align-items-center gap-2">
                                            <LucideIcon
                                                icon="Store"
                                                :size="16"
                                                class="text-primary"
                                            />
                                            <h6 class="mb-0 fw-bold">
                                                {{ template.name }}
                                            </h6>
                                        </div>
                                        <small
                                            v-if="template.category"
                                            class="text-muted"
                                        >
                                            {{ template.category }} · v{{ template.version }}
                                        </small>
                                    </div>
                                </div>
                                <p class="text-muted small mb-2">
                                    {{ template.description }}
                                </p>
                                <div class="d-flex flex-wrap gap-1 mb-2">
                                    <span class="badge bg-light text-dark border">
                                        {{ template.stepCount }}
                                        {{ $t("workflow.storeSteps") }}
                                    </span>
                                    <span
                                        v-for="team in template.teamNames"
                                        :key="team"
                                        class="badge bg-primary-subtle text-primary border"
                                    >
                                        {{ team }}
                                    </span>
                                    <span
                                        v-if="template.requiredSecrets?.length"
                                        class="badge bg-warning-subtle text-dark border"
                                    >
                                        {{ template.requiredSecrets.length }}
                                        {{ $t("workflow.storeSecretsRequired") }}
                                    </span>
                                </div>
                                <a
                                    href="#"
                                    class="small text-primary"
                                    @click.prevent.stop="viewDetail(template)"
                                >
                                    <LucideIcon
                                        icon="Eye"
                                        :size="14"
                                    />
                                    {{ $t("workflow.storeViewDetail") }}
                                </a>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <ModalComponent
            v-if="showModal"
            id="workflowTemplateDetailModal"
            :title="selectedTemplate?.name || ''"
            saveText="labelClose"
            :cancelText="''"
            @save="closeModal"
            ref="modalRef"
        >
            <template #body>
                <div class="modal-body-content m-3">
                    <p class="text-muted mb-2">
                        {{ selectedTemplate?.description }}
                    </p>
                    <p class="small mb-3">
                        <strong>{{ $t("workflow.storeCategory") }}:</strong>
                        {{ selectedTemplate?.category || "-" }}
                        ·
                        <strong>{{ $t("workflow.storeVersion") }}:</strong>
                        {{ selectedTemplate?.version }}
                        ·
                        <strong>{{ $t("workflow.storeSteps") }}:</strong>
                        {{ selectedTemplate?.stepCount }}
                    </p>
                    <div
                        v-if="selectedTemplate?.requiredSecrets?.length"
                        class="alert alert-warning py-2 small mb-0"
                    >
                        {{ $t("workflow.storeSecretsHint") }}
                        <ul class="mb-0 mt-2">
                            <li
                                v-for="secret in selectedTemplate.requiredSecrets"
                                :key="secret"
                            >
                                {{ secret }}
                            </li>
                        </ul>
                    </div>
                </div>
            </template>
            <template #footer>
                <div class="modal-footer justify-content-center">
                    <button
                        type="button"
                        class="btn btn-secondary"
                        @click="closeModal"
                    >
                        {{ $t("common.close") }}
                    </button>
                </div>
            </template>
        </ModalComponent>

        <ModalComponent
            v-if="showSecretsModal"
            id="workflowSecretsModal"
            :title="$t('workflow.storeSecretsTitle')"
            saveText="workflow.storeImportButton"
            cancelText="common.cancel"
            @save="confirmImportWithSecrets"
            ref="secretsModalRef"
        >
            <template #body>
                <div class="modal-body-content m-3">
                    <p class="text-muted small">
                        {{ $t("workflow.storeSecretsHint") }}
                    </p>
                    <div
                        v-for="secret in pendingSecretKeys"
                        :key="secret"
                        class="mb-3"
                    >
                        <label
                            class="form-label small fw-semibold"
                            :for="`secret-${secret}`"
                        >
                            {{ secret }}
                        </label>
                        <input
                            :id="`secret-${secret}`"
                            type="password"
                            class="form-control form-control-sm"
                            v-model="secretValues[secret]"
                            autocomplete="off"
                        />
                    </div>
                </div>
            </template>
        </ModalComponent>
    </main>
</template>

<script>
    import WorkflowService from "@/services/workflow/WorkflowService";
    import ModalComponent from "@/components/global/ModalComponent.vue";

    export default {
        name: "WorkflowStorePage",
        components: {
            ModalComponent,
        },
        data() {
            return {
                templates: [],
                selectedTemplates: [],
                filterQuery: "",
                orderBy: "created_desc",
                loading: false,
                importing: false,
                showModal: false,
                showSecretsModal: false,
                selectedTemplate: null,
                secretValues: {},
                pendingSecretKeys: [],
            };
        },
        computed: {
            filteredTemplates() {
                if (!this.filterQuery) {
                    return this.templates;
                }
                const query = this.filterQuery.toLowerCase();
                return this.templates.filter(
                    (t) =>
                        t.name.toLowerCase().includes(query) ||
                        (t.description && t.description.toLowerCase().includes(query)) ||
                        (t.category && t.category.toLowerCase().includes(query))
                );
            },
            allSelected() {
                return (
                    this.filteredTemplates.length > 0 &&
                    this.filteredTemplates.every((t) => this.isSelected(t.id))
                );
            },
            requiredSecretsForSelection() {
                const keys = new Set();
                this.templates
                    .filter((t) => this.isSelected(t.id))
                    .forEach((t) => {
                        (t.requiredSecrets || []).forEach((s) => keys.add(s));
                    });
                return Array.from(keys);
            },
        },
        methods: {
            async loadTemplates() {
                this.loading = true;
                try {
                    const result = await WorkflowService.findWorkflowTemplates(
                        this.filterQuery,
                        this.orderBy
                    );
                    if (result.error) {
                        this.$notify({
                            title: "workflow.index",
                            message: "workflow.storeLoadError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                        this.templates = [];
                    } else {
                        this.templates = result;
                    }
                } catch {
                    this.templates = [];
                } finally {
                    this.loading = false;
                }
            },
            isSelected(id) {
                return this.selectedTemplates.includes(id);
            },
            toggleSelection(id) {
                const index = this.selectedTemplates.indexOf(id);
                if (index > -1) {
                    this.selectedTemplates.splice(index, 1);
                } else {
                    this.selectedTemplates.push(id);
                }
            },
            toggleSelectAll() {
                if (this.allSelected) {
                    this.filteredTemplates.forEach((t) => {
                        const index = this.selectedTemplates.indexOf(t.id);
                        if (index > -1) {
                            this.selectedTemplates.splice(index, 1);
                        }
                    });
                } else {
                    this.filteredTemplates.forEach((t) => {
                        if (!this.isSelected(t.id)) {
                            this.selectedTemplates.push(t.id);
                        }
                    });
                }
            },
            async importSelected() {
                if (this.requiredSecretsForSelection.length > 0) {
                    this.pendingSecretKeys = this.requiredSecretsForSelection;
                    this.secretValues = {};
                    this.pendingSecretKeys.forEach((key) => {
                        this.secretValues[key] = "";
                    });
                    this.showSecretsModal = true;
                    this.$nextTick(() => {
                        this.$refs.secretsModalRef?.open();
                    });
                    return;
                }
                await this.runImport({});
            },
            async confirmImportWithSecrets() {
                const missing = this.pendingSecretKeys.filter(
                    (key) => !this.secretValues[key]?.trim()
                );
                if (missing.length > 0) {
                    this.$notify({
                        title: "workflow.index",
                        message: "workflow.storeSecretsMissing",
                        variant: "warning",
                        icon: "CircleAlert",
                    });
                    return;
                }
                this.$refs.secretsModalRef?.close();
                this.showSecretsModal = false;
                await this.runImport(this.secretValues);
            },
            async runImport(secretValues) {
                this.importing = true;
                try {
                    const result = await WorkflowService.importWorkflowTemplates(
                        this.selectedTemplates,
                        secretValues
                    );
                    if (result.error || !result?.length) {
                        this.$notify({
                            title: "workflow.index",
                            message: "workflow.storeImportError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    } else {
                        this.$notify({
                            title: "workflow.index",
                            message: "workflow.storeImportSuccess",
                            variant: "success",
                            icon: "CircleCheckBig",
                        });
                        this.goBack();
                    }
                } catch {
                    this.$notify({
                        title: "workflow.index",
                        message: "workflow.storeImportError",
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.importing = false;
                }
            },
            viewDetail(template) {
                this.selectedTemplate = template;
                this.showModal = true;
                this.$nextTick(() => {
                    this.$refs.modalRef?.open();
                });
            },
            closeModal() {
                this.$refs.modalRef?.close();
                this.showModal = false;
                this.selectedTemplate = null;
            },
            goBack() {
                this.$router.push({ name: "WorkflowPage" });
            },
        },
        mounted() {
            this.loadTemplates();
        },
    };
</script>

<style scoped>
    .template-card {
        transition: all 0.2s ease;
        cursor: pointer;
    }

    .template-card:hover {
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    }

    .template-card.selected {
        border-color: #0073ea;
        background-color: #f0f7ff;
    }

    .loading-container {
        padding-left: 10px;
        padding-right: 10px;
    }

    .data-load {
        background-color: var(--color-bg-loading-content) !important;
        border-color: var(--color-bg-loading-content) !important;
        color: var(--color-body-content) !important;
        text-align: center;
        padding: 9px;
        border-bottom-width: 2px;
        border-radius: 10px;
    }
</style>
