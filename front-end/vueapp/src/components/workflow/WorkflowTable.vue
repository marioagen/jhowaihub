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
                    <a :class="actionClass" class="text-primary" @click="redirectToIndex(data.row)" v-tooltip="$t('workflow.access')">
                        <LucideIcon icon="ExternalLink" />
                    </a>
                    <a :class="actionClass" @click="redirectToEdit(data.row)" v-tooltip="$t('common.edit')">
                        <LucideIcon icon="SquarePen" />
                    </a>
                    <a :class="actionClass" class="text-danger"  style="color: red;" @click="openConfirmation(data.row)" v-tooltip="$t('common.delete')">
                        <LucideIcon icon="Trash2" />
                    </a>
                </ActionTableListComponent>
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
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "id" },
                    { key: "name", label: "workflow.name" },
                    { key: "teams", label: "workflow.teams" },
                    { key: "actions", label: "workflow.actions" },
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
