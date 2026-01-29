<template>
    <button v-if="showMultiDelete" class="btn btn-outline-danger btn-sm mb-2 ms-2" @click="openConfirmationMultiple">
        <LucideIcon icon="Trash2" :size="15" />
        {{ $t("common.delete") }}
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
            <template #cell-inputData="{ data }">
                {{ data.row.inputData }}
            </template>
            <template #cell-outputData="{ data }">
                {{ data.row.outputData }}
            </template>
            <template #cell-actions="{ data }">
                <ActionTableListComponent v-slot="{ actionClass }">
                    <a :class="actionClass" @click="openEditModal(data.row)" v-tooltip="$t('common.edit')">
                        <LucideIcon icon="SquarePen" />
                    </a>
                    <a :class="actionClass" class="text-danger" @click="openConfirmation(data.row)" v-tooltip="$t('common.delete')">
                        <LucideIcon icon="Trash2" />
                    </a>
                </ActionTableListComponent>
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
        message="common.thisActionCannotBeUndone"
        cancelText="common.cancel"
        confirmText="common.confirm"
        confirmVariant="primary"
        ref="DeleteDialog"
        :isLoading="isDeleting"
        @confirm="deleteTool"
    />
</template>

<script>
    import date from "@/helpers/date";
    import ToolsService from '@/services/tools/ToolsServices';
    import TableComponent from "@/components/global/TableComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import ToolsModal from "@/components/tools/ToolsModal.vue";    
    import ActionTableListComponent from "@/components/global/ActionTableListComponent.vue";

    export default {
        name: "ToolsTable",
        components: {
            TableComponent,
            ConfirmModal,
            ToolsModal,
            ActionTableListComponent
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "common.id" },
                    { key: "name", label: "common.name" },
                    { key: "toolType", label: "tools.type" },
                    { key: "inputData", label: "tools.entry" },
                    { key: "outputData", label: "common.output" },
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
            deleteTool() {
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