<template>
    <div
        class="d-flex flex-column justify-content-between align-items-start mb-2"
    >
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
                {{ $t("types.selectToDelete") }}
            </small>
        </div>
    </div>
    <div>
        <TableComponent
            modalName="types.title"
            emptyMessage="types.noDocumentTypeWasFound"
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
                        <a
                            class="dropdown-item d-flex align-items-center gap-2"
                            @click="openEditModal(data.row)"
                        >
                            <LucideIcon icon="SquarePen" />
                            {{ $t("common.edit") }}
                        </a>
                    </li>
                    <li>
                        <a
                            class="dropdown-item d-flex align-items-center gap-2"
                            @click="
                                openConfirmation(data.row)
                            "
                        >
                            <LucideIcon icon="Trash2" />
                            {{ $t("common.delete") }}
                        </a>
                    </li>
                </DropdownComponent>
            </template>
        </TableComponent>
    </div>
    <TypesModal
        :isEdit="true"
        @reload="reload"
        ref="TypesModal"
    />
    <ConfirmModal
        id="deleteConfirm"
        title="types.youAreAboutToDeleteType"
        message="common.thisActionCannotBeUndone"
        cancelText="common.cancel"
        confirmText="common.confirm"
        confirmVariant="primary"
        ref="DeleteDialog"
        :isLoading="isDeleting"
        @confirm="deleteType"
    />
</template>
<script>
    import date from "@/helpers/date";
    import TypesService from "@/services/types/TypesService";
    import TableComponent from "@/components/global/TableComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import TypesModal from "@/components/types/TypesModal.vue";
    import DropdownComponent from "@/components/global/DropdownComponent.vue";

    export default {
        name: "TypesTable",
        components: {
            DropdownComponent,
            TableComponent,
            ConfirmModal,
            TypesModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "common.id" },
                    { key: "name", label: "common.name" },
                    {
                        key: "created",
                        label: "documents.inclusionDate",
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
            selectedType: {},
            queryPage: 1,
            selectedOption: 10,
            isAscending: false,
            colType: 2,
            modalTypeShow: false,
            modalAlertShow: false,
            searchInput: "",
            isDeleting: false,
        }),
        methods: {
            getTypes(obj) {
                this.table.isLoading = true;
                this.searching = false;
                let params = {
                    search: this.searchInput.trim()
                        ? this.searchInput.trim()
                        : "",
                    page: obj.page,
                    pageSize: this.selectedOption,
                    isAscending: this.isAscending,
                    colType: this.colType,
                };

                TypesService.getTypes(params)
                    .then((response) => {
                        const content =
                            response?.content || [];
                        const pagination =
                            response?.pagination || {};

                        this.table.data = content;
                        this.table.pagination = pagination;
                    })
                    .finally(() => {
                        if (obj.type === "search")
                            this.searching = true;
                        this.table.isLoading = false;
                        this.searchInput = "";
                    });
            },
            formatDate(str) {
                return date.formatDate(str);
            },
            orderList: function (col) {
                if (this.isAscending) {
                    this.isAscending = false;
                } else {
                    this.isAscending = true;
                }
                this.colType = col;
                this.getTypes({
                    search: "",
                    page: this.queryPage,
                    type: null,
                });
            },
            selectedRows(selectedRows) {
                this.table.selectedRows = selectedRows;
            },
            openEditModal(type) {
                this.$refs.TypesModal.open(type);
            },
            openConfirmation(type) {
                this.selectedType = [type.id];
                this.$refs.DeleteDialog.open();
            },
            openConfirmationMultiple() {
                const ids = this.table.selectedRows.map(
                    (item) => item.id
                );
                this.selectedType = ids;
                this.$refs.DeleteDialog.open();
            },
            deleteType() {
                this.isDeleting = true;
                TypesService.deleteTypeById(
                    this.selectedType
                )
                    .then((success) => {
                        if (success) {
                            this.$refs.DeleteDialog.close();
                            this.getTypes({
                                search: "",
                                page: 1,
                                type: null,
                            });
                            this.$notify({
                                title: "Tipos",
                                message: this.$t(
                                    "types.removeSuccess"
                                ),
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "Tipos",
                                message: this.$t(
                                    "types.errors.removeError"
                                ),
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
                this.getTypes({
                    search: input,
                    page: this.queryPage,
                    type: null,
                });
            },
            changePage(page) {
                this.getTypes({
                    search: "",
                    page: page,
                    type: null,
                });
            },
            reload() {
                this.$refs.TypesModal.close();
                this.getTypes({
                    search: "",
                    page: this.queryPage,
                    type: null,
                });
            },
        },
        created() {
            this.queryPage = this.$route.query.page
                ? this.$route.query.page
                : 1;
            this.getTypes({
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
