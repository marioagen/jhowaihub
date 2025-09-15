<template>
    <button v-if="showMultiDelete" class="btn btn-outline-danger btn-sm mb-2 ms-2" @click="openConfirmationMultiple">
        <LucideIcon icon="Trash2" :size="15" />
        {{ $t("labelDelete") }}
    </button>
    <div>
        <TableComponent
            modalName="tools.index"
            emptyMessage="tools.notFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
            :pagination="table.pagination"
            @selectedRows="selectedRows"
            @change-page="changePage"
        >
            <template #cell-actions="{ data }">
                <div class="dropdown">
                    <a class="btn p-0 border-0" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                        <LucideIcon icon="Ellipsis" />
                    </a>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li>
                            <a class="dropdown-item d-flex align-items-center gap-2" @click="openEditModal(data.row)">
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
                    </ul>
                </div>
            </template>
        </TableComponent>
    </div>
    <ToolsModal
        :isEdit="true" 
        @reload="reload" 
        ref="ToolsModal" 
    />
    <ConfirmModal
        id="deleteConfirm"
        title="tools.removeTitle"
        message="labelThisActionCannotBeUndone"
        cancelText="labelCancel"
        confirmText="labelConfirm"
        confirmVariant="primary"
        ref="DeleteDialog"
        :isLoading="isDeleting"
        @confirm="deleteType"
    />
</template>

<script>
    import date from "@/helpers/date";
    import ToolsService from '@/services/tools/ToolsServices';
    import TableComponent from "@/components/global/TableComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import ToolsModal from "@/components/tools/ToolsModal.vue"

    export default {
        name: "ToolsTable",
        components: {
            TableComponent,
            ConfirmModal,
            ToolsModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "id" },
                    { key: "name", label: "labelName" },
                    { key: "toolType", label: "tools.type" },
                    { key: "inputData", label: "tools.entry" },
                    { key: "outputData", label: "tools.output" },
                    { key: "actions", label: "labelAction" },
                ],
                data: [],
                pagination: {
                    currentPage: 1,
                    totalPages: 100,
                    totalItems: 2000,
                    itemsPerPage: 10,
                },
                selectedRows: [],
            },
            filters: {
                input: "",
                toolTypeId: "",
                isAsc: true,
            },
            isDeleting: false,
        }),
        methods: {
            getTools() {
                this.table.isLoading = true;
                let params = {
                    search: this.filters.input,
                    page: this.table.pagination.currentPage,
                    pageSize: this.table.pagination.itemsPerPage,
                    isAscending: this.filters.isAsc,
                    toolTypeId: this.filters.toolTypeId,
                };

                ToolsService.getTools(params)
                    .then((response) => {
                        const content = response?.content || [];
                        const pagination = response?.pagination || {};

                        this.table.data = content;
                        this.table.pagination = pagination;
                    })
                    .finally(() => {
                        this.table.isLoading = false;
                    });
            },
            formatDate(str) {
                return date.formatDate(str);
            },
            selectedRows(selectedRows) {
                this.table.selectedRows = selectedRows;
            },
            openEditModal(tool) {
                this.$refs.ToolsModal.open(tool);
            },
            openConfirmation(tool) {
                this.selectedTool = [tool.id];
                this.$refs.DeleteDialog.open();
            },
            openConfirmationMultiple() {
                const ids = this.table.selectedRows.map((item) => item.id);
                this.selectedTool = ids;
                this.$refs.DeleteDialog.open();
            },
            deleteType() {
                this.isDeleting = true;
                ToolsService.deleteTool(this.selectedTool)
                    .then((success) => {
                        if (success) {
                            this.$refs.DeleteDialog.close();
                            this.getTools();
                            this.$notify({
                                title: "tools.index",
                                message: "tools.removeSuccess",
                                variant: "success",
                                icon: "CircleCheckBig",
                            });
                        } else {
                            this.$notify({
                                title: "tools.index",
                                message: "tools.removeError",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }
                    })
                    .finally(() => {
                        this.table.selectedRows = [];
                        this.isDeleting = false;
                    });
            },
            filterList(filters) {
                this.filters = filters;
                this.getTypes();
            },
            changePage(page) {
                this.table.pagination.currentPage = page;
                this.getTools();
            },
            reload() {
                this.$refs.ToolsModal.close();
                this.getTools();
            },
        },
        created() {
            this.table.pagination.currentPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getTools();
        },
        computed: {
            showMultiDelete() {
                return this.table.selectedRows.length > 1;
            },
        },
    };
</script>