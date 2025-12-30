<template>
    <div>
        <TableComponent
            modalName="template.tableTitle"
            emptyMessage="template.notFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
            :pagination="table.pagination"
            :hasSelection="false"
            @change-page="changePage"
        >
            <template #cell-method="{ data }">
                <div v-if="data.row.method">
                    <span class="method-badge" :class="`method-${data.row.method.toLowerCase()}`">
                        {{ data.row.method }}
                    </span>
                </div>
                <span v-else>-</span>
            </template>
            <template #cell-actions="{ data }">
                <DropdownComponent>
                    <li>
                        <a class="dropdown-item d-flex align-items-center gap-2" @click="redirectToEdit(data.row.id)">
                            <LucideIcon icon="SquarePen" />
                            {{ $t("common.edit") }}
                        </a>
                    </li>
                    <li>
                        <a class="dropdown-item d-flex align-items-center gap-2" @click="openConfirmation(data.row.id)">
                            <LucideIcon icon="Trash2" />
                            {{ $t("common.delete") }}
                        </a>
                    </li>
                </DropdownComponent>
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
        @confirm="deleteTemplate"
    />
</template>

<script>
    import TableComponent from "@/components/global/TableComponent.vue";
    import ConfirmModal from "@/components/global/ConfirmModal.vue";
    import DropdownComponent from "@/components/global/DropdownComponent.vue";
    import TemplateService from "@/services/template/TemplateService";
    import BadgeOutlinedComponent from "@/components/global/BadgeOutlinedComponent.vue";
    export default {
        name: "TemplateTable",
        components: {
            BadgeOutlinedComponent,
            DropdownComponent,
            TableComponent,
            ConfirmModal,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "method", label: "template.method" },
                    { key: "name", label: "template.name" },
                    { key: "url", label: "template.url" },
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
                orderBy: "created asc",
                input: null,
                method: null,
            },
            selectedTemplate: null,
            isDeleting: false,
        }),
        methods: {
            getTemplates() {
                this.isLoading = true;
                const params = {
                    pageSize: 10,
                    page: this.table.pagination.currentPage,
                    input: this.filters.input,
                    orderBy: this.filters.orderBy,
                    method: this.filters.method,
                };

                TemplateService.getTemplates(params)
                    .then((response) => {
                        if (response.error !== undefined) {
                            this.$notify({
                                title: "template.title",
                                message: "template.notFound",
                                variant: "danger",
                                icon: "CircleX",
                            });
                        }

                        this.table.data = response.content;
                        this.table.pagination = response.pagination;
                    })
                    .finally(() => {
                        this.table.isLoading = false;
                    });
            },
            redirectToEdit(id) {
                this.$router.push({
                    name: "TemplateEdit",
                    params: {
                        id: id,
                    },
                });
            },
            openConfirmation(id) {
                this.selectedTemplate = id;
                this.$refs.DeleteDialog.open();
            },
            deleteTemplate() {
                if (this.selectedTemplate === null) {
                    this.$notify({
                        title: this.$t("common.warning"),
                        message: this.$t("template.unselected"),
                        variant: "warning",
                        icon: "TriangleAlert",
                    });
                    return;
                }

                this.isDeleting = true;
                TemplateService.deleteTemplate(this.selectedTemplate)
                    .then(() => {
                        this.$refs.DeleteDialog.close();
                        this.getTemplates();
                        this.$notify({
                            title: this.$t("common.success"),
                            message: this.$t("template.removeSuccess"),
                            variant: "success",
                            icon: "CircleCheckBig",
                        });
                    })
                    .catch((error) => {
                        this.$notify({
                            title: this.$t("common.error"),
                            message: error.response?.data?.labelError ?? this.$t("template.removeError"),
                            variant: "danger",
                            icon: "CircleX",
                        });
                    })
                    .finally(() => {
                        this.selectedTemplate = null;
                        this.isDeleting = false;
                    });
            },
            changePage(page) {
                this.table.pagination.currentPage = page;
                this.getTemplates();
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getTemplates();
        },
    };
</script>
<style scoped>
    .method-badge {
        font-size: 0.75em;
        font-weight: 500;
        padding: 0.25em 0.5em;
        border-radius: 0.375rem;
        border: 1px solid currentColor;
        display: inline-block;
        white-space: nowrap;
        transition:
            color 0.2s,
            border-color 0.2s;
        cursor: default;
    }

    .method-get {
        color: #0d6efd;
        border-color: #0d6efd;
        background-color: rgba(13, 110, 253, 0.1);
    }

    .method-post {
        color: #198754;
        border-color: #198754;
        background-color: rgba(25, 135, 84, 0.1);
    }

    .method-put {
        color: #fd7e14;
        border-color: #fd7e14;
        background-color: rgba(253, 126, 20, 0.1);
    }

    .method-patch {
        color: #ffc107;
        border-color: #ffc107;
        background-color: rgba(255, 193, 7, 0.1);
    }

    .method-delete {
        color: #dc3545;
        border-color: #dc3545;
        background-color: rgba(220, 53, 69, 0.1);
    }
</style>
