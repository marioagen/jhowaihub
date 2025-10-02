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
            <template #cell-team="{ data }">
                {{ data.row.team.name }}
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
                            {{ $t("labelEdit") }}
                        </a>
                    </li>
                    <li>
                        <a
                            class="dropdown-item d-flex align-items-center gap-2"
                            @click="openConfirmation(data.row)"
                        >
                            <LucideIcon icon="Trash2" />
                            {{ $t("labelDelete") }}
                        </a>
                    </li>
                </DropdownComponent>
            </template>
        </TableComponent>
    </div>
    <ConfirmModal
        id="deleteConfirm"
        title="questions.removeTitle"
        message="labelThisActionCannotBeUndone"
        cancelText="labelCancel"
        confirmText="labelConfirm"
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
    export default {
        name: "WorkflowTable",
        components: {
            DropdownComponent,
            TableComponent,
            ConfirmModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "id" },
                    { key: "name", label: "workflow.name" },
                    { key: "team", label: "workflow.teams" },
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
                input: "",
                isAsc: true,
                isAllUsers: false,
            },
            isDeleting: false,
        }),
        methods: {
            getWorkflowList() {
                this.table.isLoading = true;
                var email = this.$store.state.userProfile.login;
                WorkflowService.getWorkflowList(email)
                    .then((response) => {
                        if(response.error !== undefined) {
                            this.$notify({
                                title: 'Error',
                                message: 'Dados salvos com erro com sucesso!',
                                variant: 'danger',
                                icon: 'CircleX',
                            });
                        }
                        this.table.data = response;
                    })
                    .finally(() => {
                        this.table.isLoading = false;
                    });
            },
            redirectToIndex(workflow) {
                this.$router.push({
                    name: 'EditQuizz',
                    params: {
                        id: workflow.id,
                    },
                });
            },
            redirectToEdit(quizz) {
                this.$router.push({
                    name: 'EditQuizz',
                    params: {
                        id: quizz.id,
                    },
                });
            },
            openConfirmation(workflow) {
                this.selectedWorkflow = [workflow.id];
                this.$refs.DeleteDialog.open();
            },
            deleteWorkflow() {
            },
            changePage(page) {
                console.log("Change page" + page)
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