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
                    :aria-label="$t('common.pagination.first')"
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
                    :aria-label="$t('common.pagination.previous')"
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
                    :aria-current="item.value === current ? 'page' : undefined"
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
                    :aria-label="$t('common.pagination.next')"
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
                    :aria-label="$t('common.pagination.last')"
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
        --pagination-link: var(--color-btn-outline-primary, #0d6efd);
        --pagination-current: var(--color-body-content, #24292f);
        --pagination-disabled: var(--color-page-link-disabled, #8c959f);
        --bs-pagination-margin-bottom: 0;
        --bs-pagination-padding-x: 0.35rem;
        --bs-pagination-padding-y: 0.2rem;
        --bs-pagination-font-size: 0.9375rem;
        --bs-pagination-color: var(--pagination-link);
        --bs-pagination-bg: transparent;
        --bs-pagination-border-color: transparent;
        --bs-pagination-border-radius: 6px;
        --bs-pagination-hover-color: var(--pagination-link);
        --bs-pagination-hover-bg: transparent;
        --bs-pagination-hover-border-color: transparent;
        --bs-pagination-focus-box-shadow: none;
        --bs-pagination-active-color: var(--pagination-current);
        --bs-pagination-active-bg: transparent;
        --bs-pagination-active-border-color: var(--pagination-link);
        --bs-pagination-disabled-color: var(--pagination-disabled);
        --bs-pagination-disabled-bg: transparent;
        --bs-pagination-disabled-border-color: transparent;
        margin-bottom: 0;
        align-items: center;
    }

    nav {
        margin-bottom: 0;
        padding-bottom: 0;
    }

    .page-item {
        margin: 0 0.35rem;
        display: flex;
        align-items: center;
    }

    .page-link {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 1.75rem;
        min-height: 1.75rem;
        padding: 0.15rem 0.4rem;
        box-sizing: border-box;
        border-radius: 6px;
        border: 1px solid transparent !important;
        font-weight: 500;
        line-height: 1;
        color: var(--pagination-link) !important;
        background-color: transparent !important;
        box-shadow: none !important;
        transition: color 0.15s ease, border-color 0.15s ease;
    }

    .page-item.active .page-link,
    .page-item.active .page-link:hover,
    .page-item.active .page-link:focus {
        color: var(--pagination-current) !important;
        background-color: transparent !important;
        border-color: var(--pagination-link) !important;
    }

    .page-item.disabled .page-link,
    .page-item.disabled .page-link:hover {
        pointer-events: none;
        color: var(--pagination-disabled) !important;
        background-color: transparent !important;
        border-color: transparent !important;
        opacity: 1;
    }

    .page-link:hover,
    .page-link:focus {
        color: var(--pagination-link) !important;
        background-color: transparent !important;
        border-color: transparent !important;
    }

    .page-link--icon {
        padding-left: 0.2rem;
        padding-right: 0.2rem;
        min-width: 1.5rem;
    }

    .page-link--icon :deep(svg) {
        display: block;
        flex-shrink: 0;
        stroke-width: 2.25;
    }

    .pagination .page-link--num {
        font-size: 0.9375rem;
        min-width: 1.75rem;
        padding-left: 0.45rem;
        padding-right: 0.45rem;
    }
</style>
