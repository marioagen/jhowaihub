<template>
    <button 
        v-if="showMultiDelete" 
        class="btn btn-outline-danger btn-sm mb-2 ms-2" 
        @click="openConfirmationMultiple"
    >
        <LucideIcon icon="Trash2" size="15" />
        {{ $t("labelDelete") }}
    </button>
    <div>
        <TableComponent
            modalName="quizz.tableTitle"
            emptyMessage="quizz.notFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
            :pagination="table.pagination"
            @selectedRows="selectedRows"
            @change-page="changePage"
        >
            <template #cell-questions="{ data }">
                <BadgeComponent 
                    :text="questionsNumber(data.row.questions)"
                    :clickable="false"
                    variant="primary"
                />
            </template>
            <template #cell-created="{ data }">
                {{ formatDate(data.row.created) }}
            </template>
            <template #cell-actions="{ data }">
                <button 
                    class="btn btn-outline-success btn-sm table-btn"
                    @click="redirectToEdit(data.row)"
                >
                    <LucideIcon icon="SquarePen" />
                </button>
                <button 
                    class="btn btn-outline-danger btn-sm ms-2 table-btn"
                    @click="openConfirmation(data.row)"
                >
                    <LucideIcon icon="Trash2" />
                </button>
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
        @confirm="deleteQuizz"
    />
</template>

<script>
    import dates from "@/helpers/date";
    import QuizzesService from "@/services/quizzes/QuizzesService";
    import TableComponent from "@/components/global/TableComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import BadgeComponent from "@/components/global/BadgeComponent.vue";

    export default {
        name: "QuizzesTable",
        components: {
            TableComponent,
            ConfirmModal,
            BadgeComponent
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "Id" },
                    { key: "title", label: "quizzes.name" },
                    { key: "typeDocName", label: "quizzes.type" },
                    { key: "questions", label: "quizzes.questions" },
                    { key: "created", label: "quizzes.createdDate" },
                    { key: "emailCreator", label: "quizzes.owner" },
                    { key: "actions", label: "quizzes.actions" },
                ],
                data: [],
                pagination: {
                    currentPage: 1,
                    totalPages: 100,
                    itemsPerPage: 10,
                    totalItems: 2000,
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
            getQuizzes(obj) {
                this.table.isLoading = true;
                this.searchInput = obj.search;
                var paramsReq = {
                    search: this.searchInput.trim() ? this.searchInput.trim() : "",
                    page: obj.page,
                    pageSize: this.selectedOption,
                    isAscending: this.isAscending,
                    colType: this.colType,
                };

                QuizzesService.getQuizzes(paramsReq)
                    .then((response) => {
                        this.table.data = response.content;
                        this.table.pagination = response.pagination;
                    })
                    .finally(() => {
                        if (obj.type === "search") this.searching = true;
                        this.table.isLoading = false;
                    });
            },
            formatDate(date) {
                return dates.formatDate(date);
            },
            questionsNumber(questions) {
                return questions.length;
            },
            orderList: function (col) {
                if (this.isAscending) {
                    this.isAscending = false;
                } else {
                    this.isAscending = true;
                }
                this.colType = col;
                this.getQuizzes({ search: "", page: this.queryPage, type: null });
            },
            selectedRows(selectedRows) {
                this.table.selectedRows = selectedRows;
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
            openConfirmationMultiple() {
                const ids = this.table.selectedRows.map((item) => item.id);
                this.selectedQuizz = ids;
                this.$refs.DeleteDialog.open();
            },
            deleteQuizz() {
                this.isDeleting = true;
                QuizzesService.deleteQuizzById(this.selectedQuizz)
                    .then((success) => {
                        if (success) {
                            this.$refs.DeleteDialog.close();
                            this.getQuizzes({ search: "", page: 1, type: null });
                            this.$notify({
                                title: 'quizzes.removeTitle',
                                message: this.$t("quizzes.removeSuccess"),
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                        } else {
                            this.$notify({
                                title: 'quizzes.removeTitle',
                                message: this.$t("quizzes.removeError"),
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
            filterList(input) {
                this.searchInput = input;
                this.getQuizzes({ search: input, page: this.queryPage, type: null });
            },
            reload() {
                this.$refs.QuizzesModal.close();
                this.getQuizzes({ search: "", page: this.queryPage, type: null });
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getQuizzes({ search: "", page: this.queryPage, type: null });
        },
        computed: {
            showMultiDelete() {
                return this.table.selectedRows.length > 1;
            },
        },
    };
</script>