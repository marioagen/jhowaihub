<template>
    <button 
        v-if="showMultiDelete" 
        class="btn btn-outline-danger btn-sm mb-2 ms-2" 
        @click="openConfirmationMultiple"
    >
        <LucideIcon icon="Trash2" :size="15" />
        {{ $t("common.delete") }}
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
                <DropdownComponent>
                    <li>
                        <a class="dropdown-item d-flex align-items-center gap-2" @click="openEditModal(data.row)">
                            <LucideIcon icon="SquarePen" />
                            {{ $t("common.edit") }}
                        </a>
                    </li>
                    <li>
                        <a
                            class="dropdown-item d-flex align-items-center gap-2"
                            @click="openConfirmation(data.row)"
                        >
                            <LucideIcon icon="Trash2" />
                            {{ $t("common.delete") }}
                        </a>
                    </li>
                </DropdownComponent>
            </template>
        </TableComponent>
    </div>
    <QuestionsModal :isEdit="true" @reload="reload" ref="QuestionsModal" />
    <ConfirmModal
        id="deleteConfirm"
        title="questions.removeTitle"
        message="common.thisActionCannotBeUndone"
        cancelText="common.cancel"
        confirmText="common.confirm"
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
    import DropdownComponent from "@/components/global/DropdownComponent.vue";

    export default {
        name: "QuestionsTable",
        components: {
            DropdownComponent,
            TableComponent,
            ConfirmModal,
            QuestionsModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "common.id" },
                    { key: "description", label: "common.description" },
                    { key: "created", label: "questions.createdData" },
                    { key: "emailCreator", label: "common.owner" },
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
                    .then((response) => {
                        this.table.data = response.content;
                        this.table.pagination = response.pagination;
                    })
                    .finally(() => {
                        if (obj.type === "search") this.searching = true;
                        this.table.isLoading = false;
                        this.searchInput = "";
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
                                title: this.$t("questions.title"),
                                message: this.$t("questions.removeSuccess"),
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                        } else {
                            this.$notify({
                                title: this.$t("questions.title"),
                                message: this.$t("questions.errors.removeError"),
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
            changePage(page) {
                this.getQuestions({ search: "", page: page, type: null });
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