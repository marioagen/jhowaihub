<template>
    <div class="d-flex flex-column justify-content-between align-items-start mb-2">
        <div class="delete-container">
            <button
                class="btn btn-outline-danger btn-sm delete-button"
                @click="openConfirmationMultiple"
                :disabled="!showMultiDelete"
            >
                <LucideIcon
                    icon="Trash2"
                    :size="15"
                />
                {{ $t("common.delete") }}
            </button>
            <small
                v-if="!showMultiDelete"
                class="text-danger delete-tooltip"
            >
                {{ $t("questions.selectToDelete") }}
            </small>
        </div>
    </div>
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
                <ActionTableListComponent v-slot="{ actionClass }">
                    <a
                        :class="actionClass"
                        @click="openEditModal(data.row)"
                        v-tooltip="$t('common.edit')"
                    >
                        <LucideIcon icon="SquarePen" />
                    </a>
                    <a
                        :class="actionClass"
                        class="text-danger"
                        @click="openConfirmation(data.row)"
                        v-tooltip="$t('common.delete')"
                    >
                        <LucideIcon icon="Trash2" />
                    </a>
                </ActionTableListComponent>
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
    import ActionTableListComponent from "@/components/global/ActionTableListComponent.vue";

    export default {
        name: "QuestionsTable",
        components: {
            ActionTableListComponent,
            TableComponent,
            ConfirmModal,
            QuestionsModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "common.id" },
                    {
                        key: "description",
                        label: "common.description",
                    },
                    {
                        key: "created",
                        label: "questions.createdData",
                    },
                    {
                        key: "emailCreator",
                        label: "common.owner",
                    },
                    {
                        key: "actions",
                        label: "common.actions",
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
            selectedQuestion: {},
            queryPage: 1,
            selectedOption: 10,
            isAscending: true,
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
                this.getQuestions({
                    search: "",
                    page: this.queryPage,
                    type: null,
                });
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
                            this.getQuestions({
                                search: "",
                                page: 1,
                                type: null,
                            });
                            this.$notify({
                                title: this.$t("questions.title"),
                                message: this.$t("questions.removeSuccess"),
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: this.$t("questions.title"),
                                message: this.$t("questions.errors.removeError"),
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                    })
                    .finally(() => {
                        this.listIds = [];
                        this.table.selectedRows = [];
                        this.isDeleting = false;
                    });
            },
            filterList(input) {
                this.searchInput = input;
                this.getQuestions({
                    search: input,
                    page: this.queryPage,
                    type: null,
                });
            },
            reload() {
                this.$refs.QuestionsModal.close();
                this.getQuestions({
                    search: "",
                    page: this.queryPage,
                    type: null,
                });
            },
            changePage(page) {
                this.getQuestions({
                    search: "",
                    page: page,
                    type: null,
                });
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getQuestions({
                search: "",
                page: this.queryPage,
                type: null,
            });
        },
        computed: {
            showMultiDelete() {
                return this.table.selectedRows.length > 0;
            },
        },
    };
</script>
<style scoped>
    .delete-container {
        position: relative;
        display: inline-block;
    }

    .delete-button {
        position: relative;
    }

    .delete-tooltip {
        opacity: 0;
        pointer-events: none;
        visibility: hidden;
        transition:
            opacity 0.2s ease,
            visibility 0.2s ease;
        position: absolute;
        top: calc(100% + 8px);
        left: 0;
        white-space: nowrap;
        background-color: #fff;
        border: 1px solid #dc3545;
        border-radius: 6px;
        padding: 6px 12px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
        z-index: 1000;
    }

    .delete-tooltip::before {
        content: "";
        position: absolute;
        bottom: 100%;
        left: 20px;
        border: 6px solid transparent;
        border-bottom-color: #dc3545;
    }

    .delete-tooltip::after {
        content: "";
        position: absolute;
        bottom: 100%;
        left: 21px;
        border: 5px solid transparent;
        border-bottom-color: #fff;
    }

    .delete-container:hover .delete-tooltip {
        opacity: 1;
        visibility: visible;
    }

    .delete-button:not(:disabled) ~ .delete-tooltip {
        display: none !important;
    }
</style>
