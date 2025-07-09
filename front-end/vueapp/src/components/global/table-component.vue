<template>
    <div class="table-div shadow-sm">
        <p class="mx-2 my-2">
            <small>
                {{ $t(modalName) }} ({{ pagination.totalItems }})
            </small>
        </p>
        <table 
            class="table table-hover table-light table-sm table-responsive mt-2 mb-4 custom-table"
        >
            <thead>
                <tr>
                    <th v-if="hasSelection">
                        <input
                            type="checkbox"
                            class="form-check-input"
                            :checked="allSelected"
                            @change="selectAllRow"
                        />
                    </th>
                    <th 
                        v-for="(column, index) in columns" 
                        :key="index" 
                    >
                        {{ $t(column.label) }}
                    </th>
                </tr>
            </thead>
            <tbody v-if="isLoading">
                <tr>
                    <td 
                        :colspan="columns.length + (hasSelection ? 1 : 0)" 
                        class="text-center text-primary bg-primary/5 py-4 italic"
                    >
                        <div class="d-flex justify-content-center">
                            <div class="spinner-border" role="status">
                                <span class="visually-hidden">Loading...</span>
                            </div>
                        </div>
                    </td>
                </tr>
            </tbody>
            <tbody v-else-if="data.length > 0">
                <tr
                    v-for="(row, index) in data" 
                    :key="index"
                >
                    <td v-if="hasSelection">
                        <input
                            type="checkbox"
                            class="form-check-input"
                            :value="row"
                            :checked="isSelected(row)"
                            @change="selectRow(row)"
                        />
                    </td>
                    <td 
                        v-for="column in columns" 
                        :key="column.key"
                    >
                        <slot 
                            :name="`cell-${column.key}`" 
                            :data="{ row, column }" 
                        >
                            {{ row[column.key] }}
                        </slot>
                    </td>
                </tr>
            </tbody>
            <tbody v-else>
                <tr>
                    <td 
                        :colspan="columns.length + (hasSelection ? 1 : 0)" 
                        class="text-center text-primary bg-primary/5 py-4 italic"
                    >
                        {{ $t(emptyMessage) }}
                    </td>
                </tr>
            </tbody>
        </table>
        <pagination-component
            v-if="showPagination"
            class="mt-2" 
            :current-page="pagination.currentPage"
            :total-pages="pagination.totalPages"
            :items-per-page="pagination.itemsPerPage"
            :total-items="pagination.totalItems"
            @change-page="changePage"
        />
    </div>
</template>
  
<script>
    import PaginationComponent from "@/components/global/pagination-component.vue";
    export default {
        props: {
            modalName: {
                type: String,
                required: true
            },
            emptyMessage: {
                type: String,
                required: false,
                default: "No data available."
            },
            data: {
                type: Array,
                required: true,
            },
            columns: {
                type: Array,
                required: true,
            },
            isLoading: {
                type: Boolean,
                default: true,
            },
            hasSelection: {
                type: Boolean,
                default: true,
            },
            pagination: {
                type: Object,
                required: false,
                default: () => ({
                    currentPage: 1,
                    totalPages: 1,
                    itemsPerPage: 10,
                    totalItems: 0,
                })
            }
        },
        components: {
            PaginationComponent,
        },
        data() {
            return {
                selectedRows: [],
            };
        },
        methods: {
            selectAllRow() {
                if (this.allSelected) {
                    this.selectedRows = [];
                } else {
                    this.selectedRows = [...this.data];
                }
                this.$emit("selectedRows", this.selectedRows);
            },
            selectRow(row) {
                const index = this.selectedRows.indexOf(row);
                if (index === -1) {
                    this.selectedRows.push(row);
                } else {
                    this.selectedRows.splice(index, 1);
                }
                this.$emit("selectedRows", this.selectedRows);
            },
            isSelected(row) {
                return this.selectedRows.includes(row);
            },
            cleanSelection() {
                this.selectedRows = [];
            },
            changePage(page) {
                this.$emit('change-page', page)
            },
        },
        computed: {
            allSelected() {
                return this.data.length > 0 && this.selectedRows.length === this.data.length;
            },
            showPagination() {
                return this.pagination.totalPages > 1;
            }
        },
        mounted() {
            this.cleanSelection();
        },
    };
</script>

<style scoped>
    .custom-table {
        overflow: hidden;
        border-collapse: separate;
        border-spacing: 0 12px;
        width: 100%;
    }

.custom-table thead th {
  border-bottom: 1px solid #d3d3d3 !important;
  background:white;
}

        .custom-table th,
        .custom-table td {
            padding: 12px;
            vertical-align: middle;
            font-size: 14px;
            font-weight: 500;
            color: #343a40;
            background: white;
        }

    .table-div {
        border: 1px solid #d3d3d3;
        border-radius: 8px;
        overflow: hidden;
        background: white;
        padding: 20px 24px; /* AQUI! padding interno da linha */
    }
</style>