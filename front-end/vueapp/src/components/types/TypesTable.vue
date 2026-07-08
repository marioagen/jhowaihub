<template>
    <div v-if="showMultiDelete" class="bulk-selection-bar mb-2">
        <div class="d-flex align-items-center justify-content-between flex-wrap gap-2">
            <div class="d-flex align-items-center gap-2">
                <span class="badge rounded-pill bg-primary bulk-count-badge">
                    {{ table.selectedRows.length }}
                </span>
                <span class="text-secondary small">
                    {{ $t("common.itemsSelected") }}
                </span>
            </div>
            <button
                class="btn btn-outline-danger btn-sm d-inline-flex align-items-center gap-1"
                @click="openConfirmationMultiple"
            >
                <LucideIcon icon="Trash2" :size="14" />
                {{ $t("common.delete") }}
            </button>
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
    import ActionTableListComponent from "@/components/global/ActionTableListComponent.vue";

    export default {
        name: "TypesTable",
        components: {
            ActionTableListComponent,
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
            isAscending: true,
            colType: 1,
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
                    search: this.searchInput.trim() ? this.searchInput.trim() : "",
                    page: obj.page,
                    pageSize: this.selectedOption,
                    isAscending: this.isAscending,
                    colType: this.colType,
                };

                TypesService.getTypes(params)
                    .then((response) => {
                        const content = response?.content || [];
                        const pagination = response?.pagination || {};

                        this.table.data = content;
                        this.table.pagination = pagination;
                    })
                    .finally(() => {
                        if (obj.type === "search") this.searching = true;
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
                const ids = this.table.selectedRows.map((item) => item.id);
                this.selectedType = ids;
                this.$refs.DeleteDialog.open();
            },
            deleteType() {
                this.isDeleting = true;
                TypesService.deleteTypeById(this.selectedType)
                    .then((success) => {
                        if (success) {
                            this.$refs.DeleteDialog.close();
                            this.getTypes({
                                search: "",
                                page: 1,
                                type: null,
                            });
                            this.$notify({
                                title: "types.title",
                                message: this.$t("types.removeSuccess"),
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "types.title",
                                message: this.$t("types.errors.removeError"),
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
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
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
.bulk-selection-bar {
    padding: 0.5rem 0.75rem;
    background-color: var(--bs-white, #fff);
    border: 1px solid var(--bs-border-color, #dee2e6);
    border-radius: 0.375rem;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
}

.bulk-count-badge {
    font-size: 0.75rem;
    min-width: 1.5rem;
    padding: 0.25rem 0.5rem;
}
</style>
