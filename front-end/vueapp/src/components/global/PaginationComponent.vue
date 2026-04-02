<template>
    <nav v-if="totalPages && totalPages > 1">
        <ul class="pagination justify-content-center">
            <li
                class="page-item"
                :class="{ disabled: current === 1 }"
            >
                <a
                    class="page-link page-link--icon"
                    href="#"
                    aria-label="First page"
                    @click.prevent="changePage(1)"
                >
                    <LucideIcon
                        icon="ChevronsLeft"
                        :size="16"
                    />
                </a>
            </li>
            <li
                class="page-item"
                :class="{ disabled: current === 1 }"
            >
                <a
                    class="page-link page-link--icon"
                    href="#"
                    aria-label="Previous page"
                    @click.prevent="changePage(current - 1)"
                >
                    <LucideIcon
                        icon="ChevronLeft"
                        :size="16"
                    />
                </a>
            </li>

            <li
                v-for="item in pages"
                :key="item.key"
                class="page-item"
                :class="{ active: item.value === current }"
            >
                <a
                    class="page-link page-link--num"
                    href="#"
                    @click.prevent="changePage(item.value)"
                >
                    {{ item.value }}
                </a>
            </li>

            <li
                class="page-item"
                :class="{ disabled: current === totalPages }"
            >
                <a
                    class="page-link page-link--icon"
                    href="#"
                    aria-label="Next page"
                    @click.prevent="changePage(current + 1)"
                >
                    <LucideIcon
                        icon="ChevronRight"
                        :size="16"
                    />
                </a>
            </li>
            <li
                class="page-item"
                :class="{ disabled: current === totalPages }"
            >
                <a
                    class="page-link page-link--icon"
                    href="#"
                    aria-label="Last page"
                    @click.prevent="changePage(totalPages)"
                >
                    <LucideIcon
                        icon="ChevronsRight"
                        :size="16"
                    />
                </a>
            </li>
        </ul>
    </nav>
</template>
<script>
    export default {
        name: "PaginationComponent",
        props: {
            currentPage: {
                type: Number,
                required: true,
            },
            totalPages: {
                type: Number,
                required: true,
            },
            itemsPerPage: {
                type: Number,
                required: true,
            },
            totalItems: {
                type: Number,
                required: true,
            },
        },
        emits: ["change-page"],
        data() {
            return {
                current: this.currentPage,
            };
        },
        watch: {
            currentPage(newVal) {
                this.current = newVal;
            },
        },
        computed: {
            pages() {
                const total = this.totalPages;
                if (!total || total <= 1) {
                    return [];
                }

                const c = this.current;
                let start;
                let end;

                if (total <= 3) {
                    start = 1;
                    end = total;
                } else if (c <= 2) {
                    start = 1;
                    end = 3;
                } else if (c >= total - 1) {
                    start = total - 2;
                    end = total;
                } else {
                    start = c - 1;
                    end = c + 1;
                }

                const items = [];
                for (let i = start; i <= end; i++) {
                    items.push({
                        value: i,
                        key: `page-${i}`,
                    });
                }

                return items;
            },
        },
        methods: {
            changePage(page) {
                if (this.isValidPage(page)) {
                    this.current = page;
                    this.$emit("change-page", page);
                }
            },
            isValidPage(page) {
                return page >= 1 && page <= this.totalPages && page !== this.current;
            },
        },
    };
</script>
<style scoped>
    .pagination {
        --bs-pagination-margin-bottom: 0;
        --bs-pagination-padding-x: 0.6rem;
        --bs-pagination-padding-y: 0.35rem;
        --bs-pagination-font-size: 0.875rem;
        --bs-pagination-color: #0969da;
        --bs-pagination-bg: transparent;
        --bs-pagination-border-color: transparent;
        --bs-pagination-border-radius: 6px;
        --bs-pagination-hover-color: #0969da;
        --bs-pagination-hover-bg: #f6f8fa;
        --bs-pagination-hover-border-color: #d0d7de;
        --bs-pagination-focus-box-shadow: none;
        --bs-pagination-active-color: #24292f;
        --bs-pagination-active-bg: #eaeef2;
        --bs-pagination-active-border-color: #d0d7de;
        --bs-pagination-disabled-color: #8c959f;
        --bs-pagination-disabled-bg: transparent;
        --bs-pagination-disabled-border-color: transparent;
        margin-bottom: 0;
    }

    nav {
        margin-bottom: 0;
        padding-bottom: 0;
    }

    .page-item {
        margin: 0 4px;
    }

    .page-link {
        border-radius: 8px;
        border: 1px solid transparent;
        font-weight: 500;
        transition: background-color 0.2s ease;
        background-color: var(--color-card-content) !important;
    }

    .page-item.active .page-link {
        background-color: var(--color-page-link-active) !important;
        color: var(--color-body-content);
    }

    .page-item.disabled .page-link {
        pointer-events: none;
        color: var(--color-body-content) !important;
        background-color: transparent;
        border: none;
    }

    .page-link:hover {
        background-color: var(--color-sidebar-li-collapsed-hover) !important;
        border-color: var(--color-sidebar-li-collapsed-hover) !important;
    }

    .page-link--icon {
        display: inline-flex;
        align-items: center;
        justify-content: center;
    }

    .pagination .page-link--num {
        font-size: 0.75rem;
        line-height: 1.2;
        padding: 0.2rem 0.4rem;
        min-width: 1.55rem;
        display: inline-flex;
        align-items: center;
        justify-content: center;
    }
</style>
