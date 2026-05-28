<template>
    <div>
        <TableComponent
            modalName="workflow.index"
            emptyMessage="workflow.notFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
            :pagination="table.pagination"
            :hasSelection="false"
            @change-page="changePage"
        >
            <template #cell-description="{ data }">
                <span
                    v-if="!data.row.description || !String(data.row.description).trim()"
                    class="text-muted"
                >
                    -
                </span>
                <div
                    v-else
                    class="d-flex align-items-center gap-2"
                >
                    <span
                        class="text-truncate flex-grow-1"
                        style="min-width: 0"
                    >
                        {{ descriptionPreview(data.row.description) }}
                    </span>
                    <a
                        v-if="String(data.row.description).length > 50"
                        href="#"
                        class="text-primary flex-shrink-0"
                        @click.prevent="openDescriptionModal(data.row)"
                        v-tooltip="$t('workflow.viewFullDescription')"
                    >
                        <LucideIcon icon="Eye" />
                    </a>
                </div>
            </template>
            <template #cell-teams="{ data }">
                <div v-if="data.row.teams.length > 0">
                    <BadgeOutlinedComponent
                        v-for="team in data.row.teams"
                        :key="team.id"
                        :text="team.name"
                        :clickable="false"
                        class="ms-2"
                        variant="primary"
                    />
                </div>
                <span v-else>-</span>
            </template>
            <template #cell-actions="{ data }">
                <ActionTableListComponent v-slot="{ actionClass }">
                    <a
                        :class="actionClass"
                        class="text-primary"
                        @click="redirectToIndex(data.row)"
                        v-tooltip="$t('workflow.access')"
                    >
                        <LucideIcon icon="ExternalLink" />
                    </a>
                    <a
                        :class="actionClass"
                        @click="redirectToEdit(data.row)"
                        v-tooltip="$t('common.edit')"
                    >
                        <LucideIcon icon="SquarePen" />
                    </a>
                    <a
                        :class="actionClass"
                        class="text-primary"
                        @click="openCloneModal(data.row)"
                        v-tooltip="$t('workflow.clone')"
                    >
                        <LucideIcon icon="Copy" />
                    </a>
                    <a
                        :class="actionClass"
                        class="text-danger"
                        style="color: red"
                        @click="openConfirmation(data.row)"
                        v-tooltip="$t('common.delete')"
                    >
                        <LucideIcon icon="Trash2" />
                    </a>
                </ActionTableListComponent>
            </template>
        </TableComponent>
    </div>
    <ConfirmModal
        id="deleteConfirm"
        title="documents.deleteValidationTitle"
        message="common.thisActionCannotBeUndone"
        cancelText="common.cancel"
        confirmText="common.confirm"
        confirmVariant="danger"
        ref="DeleteDialog"
        :isLoading="isDeleting"
        @confirm="deleteWorkflow"
    />
    <ConfirmModalValidationInput
        id="deleteValidationConfirm"
        title="documents.deleteValidationTitle"
        messageKey="documents.deleteValidationMessage"
        :messageParams="{
            count: documentCountToDel,
            name: selectedWorkflowName,
        }"
        cancelText="common.cancel"
        confirmText="documents.confirmPermanentDelete"
        :placeholder="$t('documents.deleteValidationPlaceholder', { name: selectedWorkflowName })"
        :validationText="selectedWorkflowName"
        confirmVariant="danger"
        iconeName="Trash2"
        iconVariant="danger"
        ref="DeleteValidationDialog"
        :isLoading="isDeleting"
        @confirm="deleteWorkflow"
    />
    <ModalComponent
        id="cloneWorkflowModal"
        ref="CloneModal"
        :title="'workflow.cloneTitle'"
        :saveText="'workflow.cloneConfirm'"
        :isLoading="isCloning"
        @save="confirmClone"
        @cancel="closeCloneModal"
    >
        <template #body>
            <div class="modal-body">
                <div class="mb-3">
                    <label
                        for="cloneWorkflowName"
                        class="form-label"
                    >
                        {{ $t("workflow.cloneNameLabel") }}
                    </label>
                    <input
                        id="cloneWorkflowName"
                        v-model="cloneWorkflowName"
                        type="text"
                        class="form-control"
                        :placeholder="$t('workflow.namePlaceholder')"
                        @keyup.enter="confirmClone"
                    />
                </div>
            </div>
        </template>
    </ModalComponent>
    <ModalComponent
        id="workflowDescriptionModal"
        ref="DescriptionModal"
        title="workflow.fullDescriptionTitle"
    >
        <template #body>
            <div class="modal-body">
                <p class="mb-0 text-break workflow-description-modal-text">
                    {{ descriptionModalText }}
                </p>
            </div>
        </template>
        <template #footer>
            <div class="modal-footer justify-content-center">
                <button
                    type="button"
                    class="btn btn-primary"
                    data-bs-dismiss="modal"
                >
                    {{ $t("common.close") }}
                </button>
            </div>
        </template>
    </ModalComponent>
</template>
<script>
    import TableComponent from "@/components/global/TableComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import ConfirmModalValidationInput from "@/components/global/ConfirmModalValidationInput.vue";
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import ActionTableListComponent from "@/components/global/ActionTableListComponent.vue";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import BadgeOutlinedComponent from "@/components/global/BadgeOutlinedComponent.vue";
    export default {
        name: "WorkflowTable",
        components: {
            BadgeOutlinedComponent,
            ActionTableListComponent,
            TableComponent,
            ConfirmModal,
            ConfirmModalValidationInput,
            ModalComponent,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "common.id" },
                    { key: "name", label: "workflow.name" },
                    { key: "description", label: "common.description" },
                    {
                        key: "teams",
                        label: "workflow.teams",
                    },
                    {
                        key: "actions",
                        label: "workflow.actions",
                    },
                ],
                data: [],
                pagination: {
                    currentPage: 1,
                    totalPages: 0,
                    itemsPerPage: 10,
                    totalItems: 0,
                },
                selectedRows: [],
            },
            filters: {
                orderBy: "name asc",
                input: "",
                isAsc: true,
                isAllUsers: true,
                teamId: "",
                userId: "",
            },
            isDeleting: false,
            isCheckingDocuments: false,
            isCloning: false,
            selectedWorkflowForClone: null,
            cloneWorkflowName: "",
            selectedWorkflowId: null,
            selectedWorkflowName: "",
            documentCountToDel: 0,
            descriptionModalText: "",
        }),
        methods: {
            descriptionPreview(text) {
                const s = String(text ?? "").trim();
                if (s.length <= 50) {
                    return s;
                }
                return `${s.slice(0, 50)}…`;
            },
            openDescriptionModal(row) {
                this.descriptionModalText = String(row.description ?? "").trim();
                this.$refs.DescriptionModal.open();
            },
            getWorkflowList() {
                this.table.isLoading = true;
                const params = {
                    login: this.$store.state.userProfile.login,
                    search: this.filters.input,
                    pageSize: 10,
                    page: this.table.pagination.currentPage,
                    isAllUsers: this.filters.isAllUsers,
                    workflowIds: this.filters.workflows,
                    orderBy: this.filters.orderBy,
                    teamId: this.filters.teamId,
                    userId: this.filters.userId,
                };

                WorkflowService.getWorkflows(params)
                    .then((response) => {
                        if (response.error !== undefined) {
                            this.$notify({
                                title: "workflow.index",
                                message: "workflow.error",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                        this.table.data = response.content;
                        this.table.pagination = response.pagination;
                    })
                    .finally(() => {
                        this.table.isLoading = false;
                    });
            },
            redirectToIndex(workflow) {
                this.$router.push({
                    name: "Workflow",
                    query: {
                        id: workflow.id,
                    },
                });
            },
            redirectToEdit(workflow) {
                this.$router.push({
                    name: "EditWorkflow",
                    params: {
                        id: workflow.id,
                    },
                });
            },
            openConfirmation(workflow) {
                this.selectedWorkflow = [workflow.id];
                this.selectedWorkflowId = workflow.id;
                this.selectedWorkflowName = workflow.name;
                this.isCheckingDocuments = true;

                WorkflowService.countDocuments(workflow.id)
                    .then((count) => {
                        if (count > 0) {
                            this.documentCountToDel = count;
                            this.$refs.DeleteValidationDialog.open();
                        } else {
                            this.$refs.DeleteDialog.open();
                        }
                    })
                    .catch(() => {
                        this.$notify({
                            title: "workflow.index",
                            message: "documents.errors.removeError",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    })
                    .finally(() => {
                        this.isCheckingDocuments = false;
                    });
            },
            openCloneModal(workflow) {
                this.selectedWorkflowForClone = workflow;
                this.cloneWorkflowName = `${workflow.name} - ${this.$t("workflow.cloneSuffix")}`;
                this.$refs.CloneModal.open();
            },
            closeCloneModal() {
                this.selectedWorkflowForClone = null;
                this.cloneWorkflowName = "";
                this.$refs.CloneModal.close();
            },
            confirmClone() {
                if (!this.selectedWorkflowForClone || !this.cloneWorkflowName?.trim()) {
                    return;
                }
                this.isCloning = true;
                WorkflowService.cloneWorkflow(
                    this.selectedWorkflowForClone.id,
                    this.cloneWorkflowName.trim()
                )
                    .then((result) => {
                        if (result.error === undefined) {
                            this.$refs.CloneModal.close();
                            this.closeCloneModal();
                            this.getWorkflowList();
                            this.$notify({
                                title: this.$t("workflow.index"),
                                message: this.$t("workflow.cloneSuccess"),
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: this.$t("workflow.index"),
                                message:
                                    result.error?.response?.data?.labelError ??
                                    this.$t("workflow.cloneError"),
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                    })
                    .finally(() => {
                        this.isCloning = false;
                    });
            },
            deleteWorkflow() {
                this.isDeleting = true;
                WorkflowService.deleteWorkflowById(this.selectedWorkflow)
                    .then((result) => {
                        if (result.error === undefined) {
                            this.$refs.DeleteDialog?.close();
                            this.$refs.DeleteValidationDialog?.close();
                            this.getWorkflowList();
                            this.$notify({
                                title: "workflow.index",
                                message: "workflow.removeSuccess",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "workflow.index",
                                message:
                                    result.error.response.data.labelError ?? "workflow.removeError",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                    })
                    .finally(() => {
                        this.isDeleting = false;
                    });
            },
            changePage(page) {
                this.table.pagination.currentPage = page;
                this.getWorkflowList();
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getWorkflowList();
        },
        computed: {
            showMultiDelete() {
                return this.table.selectedRows.length > 1;
            },
        },
    };
</script>
<style scoped>
    .workflow-description-modal-text {
        white-space: pre-wrap;
    }
</style>
