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
            modalName="questions.title"
            emptyMessage="questions.notFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
            :pagination="table.pagination"
            @selectedRows="selectedRows"
            @change-page="changePage"
        >
            <template #cell-created="{ data }">
                {{ formatDate(data.row.created) }}
            </template>
            <template #cell-actions="{ data }">
                <button 
                    class="btn btn-outline-success btn-sm table-btn" 
                    @click="openEditModal(data.row)"
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

    <QuestionsModal
        :isEdit="true"
        @reload="reload"
        ref="QuestionsModal"
    />

    <ConfirmModal
        id="deleteConfirm"
        title="questions.removeTitle"
        message="labelThisActionCannotBeUndone"
        cancelText="labelCancel"
        confirmText="labelConfirm"
        confirmVariant="primary"
        ref="DeleteDialog"
        :isLoading="isDeleting"
        @confirm="deleteQuestion"
    />
</template>

<script>
    import dates from "@/helpers/date";
    import QuestionsService from "@/services/questions/QuestionsService";
    import TableComponent from "@/components/global/TableComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import QuestionsModal from "@/components/questions/QuestionsModal.vue";

    export default {
        name: "QuestionsTable",
        components: {
            TableComponent,
            ConfirmModal,
            QuestionsModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "Id" },
                    { key: "description", label: "questions.description" },
                    { key: "created", label: "questions.createdData" },
                    { key: "emailCreator", label: "questions.owner" },
                    { key: "actions", label: "questions.actions" },
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
            selectedQuestion: {},
            queryPage: 1,
            selectedOption: 10,
            isAscending: false,
            colType: 2,
            modalTypeShow: false,
            modalAlertShow: false,
            toastShow: false,
            toastColor: "",
            toastMessage: "",
            searchInput: "",
            isDeleting: false,
        }),
        methods: {
            getQuestions(obj) {
                this.table.isLoading = true;
                this.searchInput = obj.search;
                var paramsReq = {
                    search: this.searchInput.trim() ? this.searchInput.trim() : "",
                    page: obj.page,
                    pageSize: this.selectedOption,
                    isAscending: this.isAscending,
                    colType: this.colType,
                };

                QuestionsService.getQuestions(paramsReq)
                    .then((resposne) => {
                        this.table.data = resposne.content;
                        this.table.pagination = resposne.pagination;
                    })
                    .finally(() => {
                        if (obj.type === "search") this.searching = true;
                        this.table.isLoading = false;
                    });
            },
            formatDate(date) {
                return dates.formatDate(date);
            },
            orderList: function (col) {
                if (this.isAscending) {
                    this.isAscending = false;
                } else {
                    this.isAscending = true;
                }
                this.colType = col;
                this.getQuestions({ search: "", page: this.queryPage, type: null });
            },
            selectedRows(selectedRows) {
                this.table.selectedRows = selectedRows;
            },
            openEditModal(question) {
                this.$refs.QuestionsModal.open(question);
            },
            openConfirmation(question) {
                this.selectedQuestion = [question.id];
                this.$refs.DeleteDialog.open();
            },
            openConfirmationMultiple() {
                const ids = this.table.selectedRows.map((item) => item.id);
                this.selectedQuestion = ids;
                this.$refs.DeleteDialog.open();
            },
            deleteQuestion() {
                this.isDeleting = true;
                QuestionsService.deleteQuestionById(this.selectedQuestion)
                    .then((success) => {
                        if (success) {
                            this.$refs.DeleteDialog.close();
                            this.getQuestions({ search: "", page: 1, type: null });
                            this.$notify({
                                title: 'Perguntas',
                                message: this.$t("labelQuestionRemoveSuccess"),
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                        } else {
                            this.$notify({
                                title: 'Perguntas',
                                message: this.$t("labelQuestionRemoveError"),
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
                this.getQuestions({ search: input, page: this.queryPage, type: null });
            },
            reload() {
                this.$refs.QuestionsModal.close();
                this.getQuestions({ search: "", page: this.queryPage, type: null });
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getQuestions({ search: "", page: this.queryPage, type: null });
        },
        computed: {
            showMultiDelete() {
                return this.table.selectedRows.length > 1;
            },
        },
    };
</script>