<template>
    <div class="table-div shadow-sm">
        <p class="mx-1 my-1">
            <small>{{ $t(modalName) }} ({{ pagination.totalItems }})</small>
        </p>
        <table class="table table-hover table-sm table-responsive custom-table">
            <thead>
                <tr>
                    <th v-if="hasSelection">
                        <input type="checkbox" class="form-check-input" :checked="allSelected" @change="selectAllRow" />
                    </th>
                    <th
                        v-for="(column, index) in columns"
                        :key="index"
                        :class="{'text-end': column.key === 'actions'}"
                    >
                        <div v-if="column.key !== 'actions'" class="d-flex align-items-center gap-1">
                            <span>{{ $t(column.label) }}</span>
                            <div v-if="hasOrdering">
                                <button
                                    class="btn btn-link btn-sm table-btn p-0"
                                    style="line-height: 1"
                                    @click="setOrder(column.key)"
                                >
                                    <LucideIcon v-if="showOrderDescByColumn(column.key)" icon="MoveUp" :size="15" />
                                    <LucideIcon v-else-if="showOrderAscByColumn(column.key)" icon="MoveDown" :size="15" />
                                    <LucideIcon v-else icon="ArrowDownUp" :size="15" />
                                </button>
                            </div>
                        </div>
                        <span v-else class="ms-4">
                            {{ $t(column.label) }}
                        </span>
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
            <tbody v-else-if="data?.length > 0">
                <tr v-for="(row, index) in data" :key="index">
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
                        :class="{ 'text-end': column.key === 'actions' }"
                    >
                        <slot :name="`cell-${column.key}`" :data="{ row, column }">
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
        <PaginationComponent
            v-if="showPagination"
            class="mt-2"
            :current-page="pagination.currentPage"
            :total-pages="pagination.totalPages"
            :items-per-page="10"
            :total-items="pagination.totalItems"
            @change-page="changePage"
        />
    </div>
</template>

<script>
    import PaginationComponent from "@/components/global/PaginationComponent.vue";
    export default {
        props: {
            modalName: {
                type: String,
                required: true,
            },
            emptyMessage: {
                type: String,
                required: false,
                default: "No data available.",
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
            hasOrdering: {
                type: Boolean,
                default: false,
            },
            pagination: {
                type: Object,
                required: false,
                default: () => ({
                    currentPage: 1,
                    totalPages: 1,
                    itemsPerPage: 10,
                    totalItems: 0,
                }),
            },
        },
        components: {
            PaginationComponent,
        },
        data() {
            return {
                selectedRows: [],
                order: {},
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
                this.$emit("change-page", page);
            },
            setOrder(columnKey) {
                if(this.hasntOrderBeenSet(columnKey)) {
                    this.order[columnKey] = {
                      asc: false,
                      desc: false
                    };
                }

                if(!this.hasMultipleOrderingHeader) {
                    this.removeSecondOrderings(columnKey);
                }

                if(this.order[columnKey].asc === false && this.order[columnKey].desc === false) {
                    this.order[columnKey].asc = true;
                } else if (this.order[columnKey].asc) {
                    this.order[columnKey].asc = false;
                    this.order[columnKey].desc = true;
                } else {
                    this.order[columnKey].desc = false;
                    this.order[columnKey].asc = false;
                }

                this.$emit("orderColumn", this.order);
            },
            hasntOrderBeenSet(columnKey) {
                return this.order[columnKey] === undefined;
            },
            showOrderAscByColumn(columnKey) {
                return this.hasntOrderBeenSet(columnKey) ? false : this.order[columnKey].asc;
            },
            showOrderDescByColumn(columnKey) {
                return this.hasntOrderBeenSet(columnKey) ? false : this.order[columnKey].desc;
            },
            removeSecondOrderings(columnKey) {
                let cleanOrderings =  Object.keys(this.order)
                    .filter(key => key === columnKey)
                    .reduce((obj, key) => {
                        obj[key] = this.order[key];
                        return obj;
                    }, {});
                this.order = cleanOrderings;
            },
        },
        computed: {
            allSelected() {
                return this.data?.length > 0 && this.selectedRows.length === this.data.length;
            },
            showPagination() {
                return this.pagination.totalPages > 1;
            },
        },
        mounted() {
            this.cleanSelection();
        },
    };
</script>

<style scoped>
    .custom-table {
        border-collapse: separate;
        border-spacing: 0 1px;
        width: 100%;
    }

        .custom-table thead th {
            border-bottom: 1px solid var(--color-bg-table-outline) !important;
            background: var(--color-bg-table) !important;
        }

        .custom-table th,
        .custom-table td {
            vertical-align: middle;
            font-size: 12px;
            font-weight: 500;
            color: var(--color-table-text) !important;
            background: var(--color-bg-table) !important;
        }

    .table-div {
        border: 1px solid var(--color-bg-table-outline) !important;
        border-radius: 8px;
        background: var(--color-bg-table) !important;
        padding: 20px 24px;
        overflow-x: auto;
    }
</style>
