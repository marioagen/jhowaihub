<template>
    <ModalComponent
        id="documentWorkflowListModal"
        :isLoading="isLoading"
        ref="modal"
        :hideFooter="true"
    >
        <template #header>
            <div class="modal-header-custom">
                <div class="modal-info text-center w-100">
                    <h6 class="modal-title mb-1">
                        {{ $t("documents.workflowListModal.title") }}
                    </h6>
                    <p
                        class="modal-subtitle text-muted mb-0"
                        v-html="$t('documents.workflowListModal.titleMessage')"
                        v-if="workflowListByDocument.length != 0"
                    ></p>
                </div>
                <button
                    type="button"
                    class="btn-close position-absolute top-0 end-0 m-3"
                    :aria-label="$t('common.close')"
                    @click="close"
                ></button>
            </div>
        </template>
        <template #body>
            <div class="px-2">
                <!-- Search Input -->
                <div class="input-group mt-3 mb-3 search-container">
                    <span class="input-group-text bg-white border-end-0">
                        <LucideIcon
                            icon="Search"
                            size="18"
                            class="text-muted"
                        />
                    </span>
                    <input
                        type="text"
                        class="form-control border-start-0 border-end-0 ps-0 search-input"
                        :class="{
                            'params-filled': searchData.search,
                        }"
                        :placeholder="$t('documents.workflowListModal.searchPlaceholder')"
                        v-model="searchData.search"
                        @input="filterData"
                    />
                    <span
                        class="input-group-text bg-white border-start-0"
                        v-if="searchData.search"
                        @click="cleanInput"
                        style="cursor: pointer"
                    >
                        <LucideIcon
                            icon="X"
                            size="18"
                            class="text-muted"
                        />
                    </span>
                    <span
                        class="input-group-text bg-white border-start-0"
                        v-else
                    ></span>
                </div>

                <!-- Workflow List -->
                <div class="workflow-list d-flex flex-column gap-3">
                    <div
                        v-for="workflow in workflowListByDocument"
                        :key="workflow.id"
                        class="workflow-card p-3 d-flex align-items-center justify-content-between"
                        @click="redirectToConsult(workflow)"
                    >
                        <div class="d-flex align-items-center gap-3">
                            <div
                                class="icon-circle d-flex align-items-center justify-content-center"
                            >
                                <LucideIcon
                                    icon="Workflow"
                                    size="20"
                                    class="text-primary"
                                />
                            </div>
                            <div class="workflow-info">
                                <h6
                                    class="mb-0 fw-normal text-dark d-flex align-items-center gap-2 flex-wrap"
                                >
                                    {{ workflow.name }}
                                    <span
                                        v-if="workflowStatusLabel(workflow)"
                                        class="badge status-badge border border-primary text-primary"
                                    >
                                        {{ workflowStatusLabel(workflow) }}
                                    </span>
                                </h6>
                                <small class="text-muted">
                                    {{ $t("documents.workflowListModal.clickToView") }}
                                </small>
                            </div>
                        </div>
                        <div class="arrow-icon">
                            <LucideIcon
                                icon="ChevronRight"
                                size="20"
                                class="text-muted"
                            />
                        </div>
                    </div>

                    <div
                        v-if="workflowListByDocument.length === 0 && !isLoading"
                        class="text-center py-4 text-muted"
                    >
                        {{ $t("documents.workflowListModal.nothingFound") }}
                    </div>
                </div>
            </div>
        </template>
        <template #footer>
            <div class="w-100 d-flex justify-content-end py-2 px-2 border-top mt-3">
                <button
                    class="btn btn-outline-secondary btn-sm px-4"
                    @click="close"
                >
                    {{ $t("documents.workflowListModal.cancel") }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>
<script>
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import LogService from "@/services/log/logService";

    export default {
        name: "DocumentWorkflowListModal",
        components: {
            ModalComponent,
        },
        props: {
            documentId: {
                type: [Number, String],
                required: false,
            },
        },
        data() {
            return {
                isLoading: false,
                workflowListByDocument: [],
                searchData: {
                    documentId: this.documentId,
                    login: this.$store.state.userProfile.login,
                    search: "",
                },
            };
        },
        methods: {
            workflowStatusLabel(workflow) {
                const name = workflow.statusName || workflow.StatusName || "";
                if (!name) return "";
                const key = "workflow.statusList." + name.toLowerCase();
                const translated = this.$t(key);
                return translated !== key ? translated : name;
            },
            async filterData() {
                this.isLoading = true;
                try {
                    this.searchData.documentId = this.documentId;
                    const response = await WorkflowService.getWorkflowsByDocument(this.searchData);
                    if (response?.error) {
                        this.$notify({
                            title: this.$t("common.error"),
                            message: this.$t("documents.workflowListModal.errorToGetWorkflows"),
                            variant: "danger",
                            icon: "CircleX",
                        });
                        this.workflowListByDocument = [];
                    } else {
                        this.workflowListByDocument = response.content || response || [];
                    }
                } catch (error) {
                    LogService.showMessage("Error fetching workflows:", error);
                    this.$notify({
                        title: this.$t("common.error"),
                        message: this.$t("documents.workflowListModal.errorUnexpected"),
                        variant: "danger",
                        icon: "CircleX",
                    });
                } finally {
                    this.isLoading = false;
                }
            },
            cleanInput() {
                this.searchData.search = "";
                this.filterData();
            },
            redirectToConsult(workflow) {
                this.$router.push({
                    name: "Analyzer",
                    params: {
                        documentId: this.documentId,
                        cardId: workflow.cardId,
                    },
                    query: {
                        page: this.currentPage,
                    },
                });
                this.close();
            },
            async open() {
                await this.filterData();
                if (this.workflowListByDocument.length == 1) {
                    this.redirectToConsult(this.workflowListByDocument[0]);
                    return;
                }
                this.$refs.modal.open();
            },
            close() {
                this.$refs.modal.close();
            },
        },
    };
</script>
<style scoped>
    .modal-header-custom {
        position: relative;
        width: 100%;
    }

    .modal-subtitle {
        font-size: 0.85rem;
        line-height: 1.4;
    }

    .search-container .input-group-text,
    .search-input {
        border-color: #dee2e6;
    }

    .search-input:focus {
        box-shadow: none;
        border-color: #3b82f6;
        border-top: 1px solid #3b82f6;
        border-bottom: 1px solid #3b82f6;
    }

    .input-group:focus-within .input-group-text {
        border-color: #3b82f6;
    }

    .input-group:focus-within .form-control {
        border-color: #3b82f6 !important;
    }

    .workflow-card {
        border: 1px solid #e5e7eb;
        border-radius: 8px;
        background-color: #fff;
        transition: all 0.2s ease;
        cursor: pointer;
    }

    .workflow-card:hover {
        border-color: #3b82f6;
        background-color: #f8fafc;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
    }

    .icon-circle {
        width: 40px;
        height: 40px;
        border-radius: 50%;
        background-color: #eff6ff;
    }

    .text-primary {
        color: #3b82f6 !important;
    }

    .status-badge {
        font-size: 0.7rem;
        font-weight: 500;
    }
</style>
