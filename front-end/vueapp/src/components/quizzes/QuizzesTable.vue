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
                {{ $t("quizzes.selectToDelete") }}
            </small>
        </div>
    </div>
    <div>
        <TableComponent
            modalName="quizzes.title"
            emptyMessage="quizzes.notFound"
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
                <ActionTableListComponent v-slot="{ actionClass }">
                    <a
                        :class="actionClass"
                        @click="redirectToEdit(data.row)"
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
    <ConfirmModal
        id="deleteConfirm"
        title="questions.removeTitle"
        message="common.thisActionCannotBeUndone"
        cancelText="common.cancel"
        confirmText="common.confirm"
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
    import ActionTableListComponent from "@/components/global/ActionTableListComponent.vue";

    export default {
        name: "QuizzesTable",
        components: {
            ActionTableListComponent,
            TableComponent,
            ConfirmModal,
            BadgeComponent,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "common.id" },
                    { key: "title", label: "common.name" },
                    {
                        key: "typeDocName",
                        label: "quizzes.type",
                    },
                    {
                        key: "questions",
                        label: "quizzes.questions",
                    },
                    {
                        key: "created",
                        label: "quizzes.createdDate",
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
                this.getQuizzes({
                    search: "",
                    page: this.queryPage,
                    type: null,
                });
            },
            selectedRows(selectedRows) {
                this.table.selectedRows = selectedRows;
            },

            redirectToEdit(quizz) {
                this.$router.push({
                    name: "EditQuizz",
                    params: { id: quizz.id },
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
                            this.getQuizzes({
                                search: "",
                                page: 1,
                                type: null,
                            });
                            this.$notify({
                                title: "quizzes.title",
                                message: "quizzes.removeSuccess",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "quizzes.title",
                                message: "quizzes.errors.removeError",
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
                this.getQuizzes({
                    search: input,
                    page: this.queryPage,
                    type: null,
                });
            },
            reload() {
                this.$refs.QuizzesModal.close();
                this.getQuizzes({
                    search: "",
                    page: this.queryPage,
                    type: null,
                });
            },
            changePage(page) {
                this.getQuizzes({
                    search: "",
                    page: page,
                    type: null,
                });
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getQuizzes({
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
