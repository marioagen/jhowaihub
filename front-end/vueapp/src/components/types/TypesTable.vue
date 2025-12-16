<template>
    <button v-if="showMultiDelete" class="btn btn-outline-danger btn-sm mb-2 ms-2" @click="openConfirmationMultiple">
        <LucideIcon icon="Trash2" :size="15" />
        {{ $t("common.delete") }}
    </button>
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
    <TypesModal :isEdit="true" @reload="reload" ref="TypesModal" />
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
                    { key: "created", label: "documents.inclusionDate" },
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
                this.getTypes({ search: "", page: this.queryPage, type: null });
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
                            this.getTypes({ search: "", page: 1, type: null });
                            this.$notify({
                                title: "Tipos",
                                message: this.$t("types.removeSuccess"),
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "Tipos",
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
                this.getTypes({ search: input, page: this.queryPage, type: null });
            },
            changePage(page) {
                this.getTypes({ search: "", page: page, type: null });
            },
            reload() {
                this.$refs.TypesModal.close();
                this.getTypes({ search: "", page: this.queryPage, type: null });
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getTypes({ search: "", page: this.queryPage, type: null });
        },
        computed: {
            showMultiDelete() {
                return this.table.selectedRows.length > 1;
            },
        },
    };
</script>