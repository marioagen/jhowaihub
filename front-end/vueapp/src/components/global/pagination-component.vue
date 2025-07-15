<template>
    <nav v-if="totalPages > 1">
        <ul class="pagination justify-content-center">
            <li class="page-item" :class="{ disabled: current === 1 }">
                <a class="page-link" href="#" @click.prevent="changePage(current - 1)">
                    « {{ $t('labelPrevious') }}
                </a>
            </li>

            <li
                v-for="page in pages"
                :key="page"
                :class="{ active: page === current }"
                class="page-item"
            >
                <a class="page-link" href="#" @click.prevent="changePage(page)">
                    {{ page }}
                </a>
            </li>

            <li class="page-item" :class="{ disabled: current === totalPages }">
                <a class="page-link" href="#" @click.prevent="changePage(current + 1)">
                    {{ $t('labelNext') }} »
                </a>
            </li>
        </ul>
    </nav>
</template>

<script>
    export default {
        name: 'PaginationComponent',
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
        emits: ['change-page'],
        data() {
            return {
                current: this.currentPage,
            }
        },
        watch: {
            currentPage(newVal) {
                this.current = newVal
            },
        },
        computed: {
            pages() {
                const range = []
                let start = this.current - 1
                let end = this.current + 1

                if (start < 1) {
                    start = 1
                    end = Math.min(3, this.totalPages)
                }

                if (end > this.totalPages) {
                    end = this.totalPages
                    start = Math.max(1, end - 2)
                }

                for (let i = start; i <= end; i++) {
                    range.push(i)
                }
                return range
            },
        },
        methods: {
            changePage(page) {
                if (this.isValidPage(page)) {
                    this.current = page
                    this.$emit('change-page', page)
                }
            },
            isValidPage(page) {
                return page >= 1 && page <= this.totalPages && page !== this.current
            },
        },
    }
</script>

<style scoped>
    .pagination {
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
    }

    .page-item {
        margin: 0 4px;
    }

    .page-link {
        border-radius: 8px;
        border: 1px solid transparent;
        font-weight: 500;
        transition: background-color 0.2s ease;
    }

    .page-item.active .page-link {
        background-color: #eaeef2;
        border: 1px solid #d0d7de;
        color: #24292f;
    }

    .page-item.disabled .page-link {
        pointer-events: none;
        color: #8c959f;
        background-color: transparent;
        border: none;
    }

    .page-link:hover {
        background-color: #f6f8fa;
    }
</style>
