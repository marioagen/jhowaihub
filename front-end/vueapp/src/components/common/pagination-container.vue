<template>
    <div class="row mt-1" v-if="!loading && dataList.length > 0">
        <div class="col">
            <div class="d-inline-block lines">
                <p class="d-inline">{{ $t("labelLines") }}</p>
            </div>
            <div class="d-inline-block" style="margin-left: 1%">
                <select
                    class="form-select form-select-sm d-inline"
                    v-model="selectedOption"
                    @change="getList({ search: '', page: 1, type: null })"
                >
                    <option selected>10</option>
                    <option value="25">25</option>
                    <option value="50">50</option>
                    <option value="100">100</option>
                    <option value="0">{{ $t("labelAll") }}</option>
                </select>
            </div>
            <Pagination :paginationData="pagination" :dataList="dataList"></Pagination>
        </div>
        <div class="col-auto">
            <nav>
                <ul class="pagination justify-content-center">
                    <!-- Chevrons left -->
                    <li class="page-item" v-if="pagination.currentPage != 1">
                        <a
                            class="page-link"
                            @click="getList({ search: '', page: pagination.currentPage - 1, type: null })"
                        >
                            <i class="fas fa-chevron-left"></i>
                        </a>
                    </li>
                    <li class="page-item disabled" v-else>
                        <a class="page-link" tabindex="-1" aria-disabled="true">
                            <i class="fas fa-chevron-left"></i>
                        </a>
                    </li>
                    <!-- Pages -->
                    <li
                        :class="pagination.currentPage === i ? `page-item active` : `page-item`"
                        v-for="i in pagination.listPage"
                    >
                        <a
                            class="page-link"
                            @click="getList({ search: '', page: i, type: null })"
                            v-if="pagination.currentPage != i"
                        >
                            {{ i }}
                        </a>
                        <a class="page-link" v-else>{{ i }}</a>
                    </li>
                    <!-- Chevrons right -->
                    <li class="page-item" v-if="pagination.currentPage <= pagination.pageCount - 1">
                        <a
                            class="page-link"
                            @click="getList({ search: '', page: pagination.currentPage + 1, type: null })"
                        >
                            <i class="fas fa-chevron-right"></i>
                        </a>
                    </li>
                    <li class="page-item disabled" v-else>
                        <a class="page-link">
                            <i class="fas fa-chevron-right"></i>
                        </a>
                    </li>
                </ul>
            </nav>
        </div>
    </div>
</template>
<script>
    import Pagination from "@/components/common/pagination";

    export default {
        name: "PaginationContainer",
        props: {
            pagination: {
                type: Object,
                required: true,
            },
            dataList: {
                type: Array,
                required: true,
            },
            loading: {
                type: Boolean,
                default: false,
            },
        },
        data() {
            return {
                selectedOption: "10",
            };
        },
        components: {
            Pagination,
        },
        methods: {
            getList({ search = "", page = 1, type = null }) {
                this.$emit("update-list", {
                    search,
                    page,
                    type,
                    pageSize: this.selectedOption,
                });
            },
        },
    };
</script>
<style>
    @media (max-width: 768px) {
        .lines {
            display: none !important;
        }
    }
</style>
