<template>
    <button 
        v-if="showMultiDelete" 
        class="btn btn-outline-danger btn-sm mb-2 ms-2" 
        @click="openConfirmationMultiple"
    >
        <LucideIcon icon="Trash2" :size="15" />
        {{ $t("labelDelete") }}
    </button>
    <div>
        <TableComponent
            modalName="workflow.index"
            emptyMessage="workflow.notFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
            :pagination="table.pagination"
            :hasSelection="false"
            @selectedRows="selectedRows"
            @change-page="changePage"
        >
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
    import QuizzesService from "@/services/quizzes/QuizzesService";
    import TableComponent from "@/components/global/TableComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import DropdownComponent from "@/components/global/DropdownComponent.vue";

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
            selectedQuizz: {},
            queryPage: 1,
            selectedOption: 10,
            isAscending: false,
            colType: 2,
            searchInput: "",
            isDeleting: false,
        }),
        methods: {
            getWorkflows() {
            },
            selectedRows(selectedRows) {
                this.table.selectedRows = selectedRows;
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
            openConfirmation(quizz) {
                this.selectedQuizz = [quizz.id];
                this.$refs.DeleteDialog.open();
            },
            deleteWorkflow() {
                this.isDeleting = true;
                QuizzesService.deleteQuizzById(this.selectedQuizz)
                    .then((success) => {
                        if (success) {
                            this.$refs.DeleteDialog.close();
                            this.getQuizzes({ search: "", page: 1, type: null });
                            this.$notify({
                                title: "quizzes.title",
                                message: "quizzes.removeSuccess",
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                        } else {
                            this.$notify({
                                title: "quizzes.title",
                                message: "quizzes.removeError",
                                variant: 'danger',
                                icon: 'CircleX',
                            });
                        }
                    })
                    .finally(() => {
                        this.listIds = [];
                        this.table.selectedRows = [];
                        this.isDeleting = false;
                    })
            },
            changePage(page) {
                this.getQuizzes({ search: "", page: page, type: null });
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getWorkflows();
        },
        computed: {
            showMultiDelete() {
                return this.table.selectedRows.length > 1;
            },
        },
    };
</script>