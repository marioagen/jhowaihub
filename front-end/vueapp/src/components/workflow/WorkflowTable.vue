<template>
    <div>
        <TableComponent modalName="workflow.index" emptyMessage="workflow.notFound" :data="table.data"
            :columns="table.columns" :isLoading="table.isLoading" :pagination="table.pagination" :hasSelection="false"
            @change-page="changePage">
            <template #cell-teams="{ data }">
                <div v-if="data.row.teams.length > 0">
                    <BadgeOutlinedComponent v-for="team in data.row.teams" :key="team.id" :text="team.name"
                        :clickable="false" class="ms-2" variant="primary" />
                </div>
                <span v-else>-</span>
            </template>
            <template #cell-actions="{ data }">
                <DropdownComponent>
                    <li>
                        <a class="dropdown-item d-flex align-items-center gap-2" @click="redirectToIndex(data.row)">
                            <LucideIcon icon="ExternalLink" />
                            {{ $t("workflow.access") }}
                        </a>
                    </li>
                    <li>
                        <a class="dropdown-item d-flex align-items-center gap-2" @click="redirectToEdit(data.row)">
                            <LucideIcon icon="SquarePen" />
                            {{ $t("common.edit") }}
                        </a>
                    </li>
                    <li>
                        <a class="dropdown-item d-flex align-items-center gap-2" @click="openConfirmation(data.row)">
                            <LucideIcon icon="Trash2" />
                            {{ $t("common.delete") }}
                        </a>
                    </li>
                </DropdownComponent>
            </template>
        </TableComponent>
    </div>
    <ConfirmModal
        id="deleteConfirm"
        title="questions.removeTitle"
        message="common.thisActionCannotBeUndone"
        cancelText="common.cancel"
        confirmText="common.confirm"
        confirmVariant="primary"
        ref="DeleteDialog"
        :isLoading="isDeleting"
        @confirm="deleteWorkflow"
    />
</template>

<script>
import TableComponent from "@/components/global/TableComponent.vue";
import ConfirmModal from "@/components/global/ConfirmModal.vue";
import DropdownComponent from "@/components/global/DropdownComponent.vue";
import WorkflowService from "@/services/workflow/WorkflowService";
import BadgeOutlinedComponent from "@/components/global/BadgeOutlinedComponent.vue";
export default {
    name: "WorkflowTable",
    components: {
        BadgeOutlinedComponent,
        DropdownComponent,
        TableComponent,
        ConfirmModal,
    },
    data: () => ({
        table: {
            isLoading: true,
            columns: [
                { key: "id", label: "common.id" },
                { key: "name", label: "workflow.name" },
                { key: "teams", label: "workflow.teams" },
                { key: "actions", label: "common.actions" },
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
            orderBy: "",
            input: "",
            isAsc: true,
            isAllUsers: true,
            teamId: "",
            userId: "",
        },
        isDeleting: false,
    }),
    methods: {
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
                            title: 'workflow.index',
                            message: 'workflow.error',
                            variant: 'danger',
                            icon: 'CircleX',
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
                name: 'Workflow',
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
            this.$refs.DeleteDialog.open();
        },
        deleteWorkflow() {
            this.isDeleting = true;
            WorkflowService.deleteWorkflowById(this.selectedWorkflow)
                .then((result) => {
                    if (result.error === undefined) {
                        this.$refs.DeleteDialog.close();
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
                            message: result.error.response.data.labelError ?? "workflow.removeError",
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