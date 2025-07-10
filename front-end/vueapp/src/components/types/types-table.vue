<template>
    <div>
        <table-component
            modalName="Tipos"
            totalRows="100"
            emptyMessage="labelNoDocumentTypeWasFound"
            :data="table.data"
            :columns="table.columns"
            :isLoading="table.isLoading"
        >
            <template #cell-created="{ data }">
                {{ formatDate(data.row.created) }}
            </template>
            <template #cell-actions="{ data }">
                <button
                    class="btn btn-outline-success btn-sm table-btn"
                >
                    <LucideIcon
                        icon="SquarePen"
                    />
                </button>
                <button
                    class="btn btn-outline-danger btn-sm table-btn"
                >
                    <LucideIcon
                        icon="Trash2"
                    />
                </button>
            </template>
        </table-component>
    </div>
</template>

<script>
    import dates from "@/helpers/Dates";
    import TypesService from "@/services/types/TypesService";
    import TableComponent from "@/components/global/table-component.vue";
    export default {
        name: "TypesTable",
        components: {
            TableComponent,
        },
        data: () => ({
            table: {
                isLoading: true,
                columns: [
                    { key: "id", label: "Id" },
                    { key: "name", label: "labelName" },
                    { key: "created", label: "labelInclusionDate" },
                    { key: "emailCreator", label: "labelOwner" },
                    { key: "actions", label: "labelAction" },
                ],
                data: [],
                pagination: {},
            },
            queryPage: 1,
            searchInput: "",
            selectedOption: 10,
            isAscending: false,
            colType: 2,
        }),
        methods: {
            getTypes(obj) {
                this.table.isLoading = true;
                let params = {
                    search: this.searchInput.trim() ? this.searchInput.trim() : '',
                    page: obj.page,
                    pageSize: this.selectedOption,
                    isAscending: this.isAscending,
                    colType: this.colType
                }

                TypesService.getTypes(params)
                    .then((response) => {
                        this.table.data = response.content;
                        this.table.pagination = response.pagination;
                    })
                    .finally(() => {
                        this.table.isLoading = false;
                    });
            },
            formatDate(date) {
                return dates.formatDate(date);
            },
        },
        created() {
            this.queryPage = this.$route.query.page ? this.$route.query.page : 1;
            this.getTypes({ search: '', page: this.queryPage, type: null });
        },
    }
</script>