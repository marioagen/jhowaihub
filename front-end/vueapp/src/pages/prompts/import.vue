<template>
    <main>
        <div class="container-fluid">
            <div class="mt-3 mb-3">
                <!-- Header Section -->
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <div class="d-flex align-items-center gap-3">
                        <button class="btn btn-link p-0" @click="goBack">
                            <LucideIcon icon="ArrowLeft" :size="24" />
                        </button>
                        <div>
                            <h5 class="mb-0 fw-bold">{{ $t("prompts.importTitle") }}</h5>
                            <p class="mb-0">
                                <small class="text-muted">{{ $t("prompts.importSubtitle") }}</small>
                            </p>
                        </div>
                    </div>
                    <div class="d-flex gap-2">
                        <button class="btn btn-outline-secondary btn-sm" @click="goBack">
                            {{ $t("labelCancel") }}
                        </button>
                        <button class="btn btn-primary btn-sm" @click="importSelected"
                            :disabled="selectedTemplates.length === 0 || importing">
                            <span v-if="importing" class="spinner-border spinner-border-sm me-2" role="status"></span>
                            {{ $t("prompts.importButton") }} ({{ selectedTemplates.length }})
                        </button>
                    </div>
                </div>

                <!-- Filter and Sort Section -->
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="row g-3">
                            <div class="col-md-8">
                                <input type="text" class="form-control" v-model="filterQuery"
                                    :placeholder="$t('prompts.searchPrompts')" />
                            </div>
                            <div class="col-md-4">
                                <select class="form-select" v-model="orderBy" @change="loadTemplates">
                                    <option value="created_desc">{{ $t("filters.mostRecent") }}</option>
                                    <option value="created_asc">{{ $t("filters.mostOld") }}</option>
                                    <option value="name_asc">{{ $t("filters.nameAZ") }}</option>
                                    <option value="name_desc">{{ $t("filters.nameZA") }}</option>
                                </select>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Select All Section -->
                <div class="mb-3" v-if="filteredTemplates.length > 0">
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" id="selectAll" :checked="allSelected"
                            @change="toggleSelectAll" />
                        <label class="form-check-label" for="selectAll">
                            {{ $t("prompts.selectAllTemplates").replace('{count}', filteredTemplates.length) }}
                        </label>
                    </div>
                </div>

                <div class="row loading-container" v-if="loading">
                    <div class="data-load">
                        <i class="fas fa-sync-alt fa-spin text-secondary"></i>&nbsp;{{ $t('labelLoading') }}..
                    </div>
                </div>
                <div class="row loading-container" v-if="!loading && templates.length === 0">
                    <div class="data-load">
                        <i class="fas fa-exclamation-circle text-secondary"></i>&nbsp;{{
                            $t('prompts.noPromptsListWereFound') }}.
                    </div>
                </div>

                <div class="row g-3" v-if="!loading && filteredTemplates.length > 0">
                    <div v-for="template in filteredTemplates" :key="template.id" class="col-md-4">
                        <div class="card h-100 template-card" :class="{ 'selected': isSelected(template.id) }">
                            <div class="card-body">
                                <div class="d-flex align-items-start mb-2">
                                    <input class="form-check-input me-2 mt-1" type="checkbox"
                                        :id="`template-${template.id}`" :checked="isSelected(template.id)"
                                        @change="toggleSelection(template.id)" />
                                    <div class="flex-grow-1">
                                        <div class="d-flex align-items-center gap-2">
                                            <LucideIcon icon="Globe" :size="16" class="text-primary" />
                                            <h6 class="mb-0 fw-bold">{{ template.name }}</h6>
                                        </div>
                                    </div>
                                </div>
                                <p class="text-muted small mb-2">{{ template.description }}</p>
                                <div class="prompt-preview mb-2 ">
                                    <div class="text-muted small">
                                        {{ template.text }}
                                    </div>
                                    <a href="#" class="small text-primary" @click.prevent="viewComplete(template)">
                                        <LucideIcon icon="Eye" :size="14" />
                                        {{ $t("prompts.viewComplete") }}
                                    </a>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <ModalComponent v-if="showModal" id="viewCompleteModal" :title="selectedTemplate?.name || ''"
            :saveText="'labelClose'" :cancelText="''" @save="closeModal" ref="modalRef">
            <template #body>
                <div class="modal-body-content m-3">
                    <p class="text-muted mb-3">{{ selectedTemplate?.description }}</p>
                    <label>Conteúdo do prompt</label>
                    <div class="prompt-content-full">
                        {{ selectedTemplate?.text }}
                    </div>
                    <div class="text-end">
                        <small class="text-muted">
                            {{ $t("labelCreated") }} {{ formatDate(selectedTemplate?.created) }}
                        </small>
                    </div>
                </div>
            </template>
            <template #footer>
                <div class="modal-footer justify-content-center">
                    <button type="button" class="btn btn-secondary" @click="closeModal">
                        {{ $t("labelClose") }}
                    </button>
                </div>
            </template>
        </ModalComponent>
    </main>
</template>

<script>
import PromptService from "@/services/prompts/PromptsService";
import ModalComponent from "@/components/global/ModalComponent.vue";

export default {
    name: "PromptImportPage",
    components: {
        ModalComponent
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
            selectedTemplate: null
        };
    },
    computed: {
        filteredTemplates() {
            if (!this.filterQuery) {
                return this.templates;
            }
            const query = this.filterQuery.toLowerCase();
            return this.templates.filter(t =>
                t.name.toLowerCase().includes(query) ||
                t.description.toLowerCase().includes(query) ||
                t.text.toLowerCase().includes(query)
            );
        },
        allSelected() {
            return this.filteredTemplates.length > 0 &&
                this.filteredTemplates.every(t => this.isSelected(t.id));
        }
    },
    methods: {
        async loadTemplates() {
            this.loading = true;
            try {
                const result = await PromptService.findPromptTemplates(this.filterQuery, this.orderBy);
                if (result.error) {
                    this.$notify({
                        title: 'prompts.title',
                        message: 'prompts.importError',
                        variant: 'danger',
                        icon: 'CircleX',
                    });
                    this.templates = [];
                } else {
                    this.templates = result;
                }
            } catch (error) {
                this.$notify({
                    title: 'prompts.title',
                    message: 'prompts.importError',
                    variant: 'danger',
                    icon: 'CircleX',
                });
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
                this.filteredTemplates.forEach(t => {
                    const index = this.selectedTemplates.indexOf(t.id);
                    if (index > -1) {
                        this.selectedTemplates.splice(index, 1);
                    }
                });
            } else {
                this.filteredTemplates.forEach(t => {
                    if (!this.isSelected(t.id)) {
                        this.selectedTemplates.push(t.id);
                    }
                });
            }
        },
        async importSelected() {
            this.importing = true;
            try {
                const result = await PromptService.importPrompts(this.selectedTemplates);
                if (result.error || !result) {
                    this.$notify({
                        title: 'prompts.title',
                        message: 'prompts.importError',
                        variant: 'danger',
                        icon: 'CircleX',
                    });
                } else {
                    this.$notify({
                        title: 'prompts.title',
                        message: 'prompts.importSuccess',
                        variant: 'success',
                        icon: 'CircleCheckBig',
                    });
                    this.$router.push({ name: "Prompts" });
                }
            } catch (error) {
                this.$notify({
                    title: 'prompts.title',
                    message: 'prompts.importError',
                    variant: 'danger',
                    icon: 'CircleX',
                });
            } finally {
                this.importing = false;
            }
        },
        viewComplete(template) {
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
            this.$router.back();
        },
        formatDate(dateStr) {
            if (!dateStr) return '';
            const date = new Date(dateStr);
            return `${String(date.getDate()).padStart(2, '0')}/${String(date.getMonth() + 1).padStart(2, '0')}/${date.getFullYear()}`;
        }
    },
    mounted() {
        this.loadTemplates();
    }
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

.prompt-preview {
    max-height: 90px;
    font-size: 0.875rem;
    color: #666;
    background-color: #f8f9fa;
    padding: 0.5rem;
    border-radius: 0.25rem;
    border: 1px solid #dee2e6;
}

.prompt-preview div {
    max-height: 60px;
    overflow: hidden;
    text-overflow: ellipsis;
    display: -webkit-box;
    -webkit-line-clamp: 3;
    -webkit-box-orient: vertical;
}

.prompt-content-full {
    white-space: pre-wrap;
    font-size: 0.875rem;
    background-color: #f8f9fa;
    padding: 1rem;
    border-radius: 0.25rem;
    border: 1px solid #dee2e6;
    max-height: 400px;
    overflow-y: auto;
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
