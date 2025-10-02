<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("quizzes.title") }}</h5>
                        <p>
                            <small class="text-muted">{{ $t("quizzes.subtitle") }}</small>
                        </p>
                    </div>
                    <button 
                        class="btn btn-primary btn-sm" 
                        @click="redirectToForm"
                    >
                        <LucideIcon icon="Plus" :size="17" />
                        {{ $t("quizzes.createBtn") }}
                    </button>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <SearchComponent 
                            :entity="entitySearch" 
                            :resetInput="resetInputSearch"
                            @search="filterList" 
                        />
                    </div>
                </div>

                <QuizzesTable 
                    ref="QuizzesTable"
                />
            </div>
        </div>
    </main>    
</template>

<script>
    import SearchComponent from "@/components/global/SearchComponent.vue";
    import QuizzesTable from "@/components/quizzes/QuizzesTable.vue";

    export default {
        name: "QuizzesPage",
        data() {
            return {
                crumbsData: [],
                entitySearch: {},
                resetInputSearch: false,
            };
        },
        components: {
            SearchComponent,
            QuizzesTable
        },
        methods: {
            setEntitySearch() {
                this.entitySearch = {
                    screen: "quizzes",
                    labelInput: this.$t("quizzes.filters.input"),
                    placeholderInput: this.$t("quizzes.filters.input"),
                    labelButton: this.$t("quizzes.createBtn"),
                };
            },
            filterList(obj) {
                this.$refs.QuizzesTable.filterList(obj.search);
            },
            redirectToForm() {
                this.$router.push({ name: "NewQuizz" });
            },
        },
        created() {
            this.setEntitySearch();
        },
    };
</script>

<style scoped>
    .content-center {
        align-items: center;
        display: flex;
        flex-direction: row;
        flex-wrap: wrap;
        justify-content: center;
    }

    tbody {
        background-color: #fff !important;
    }

    .content-left-middle {
        text-align: left;
        vertical-align: middle;
        max-width: 200px;
    }

    .content-right-middle {
        text-align: right;
        vertical-align: middle;
    }

    .content-center-middle {
        text-align: center;
        vertical-align: middle;
    }

    .bg-success {
        background-color: #edfef2 !important;
        color: #0eaa42 !important;
        font-weight: inherit !important;
        padding: 8px 12px !important;
    }

    .container-fluid {
        padding: 0 13px;
    }

    .scroll-area {
        display: list-item;
        overflow-y: auto;
    }

    @media (max-width: 768px) {
        .lines {
            display: none !important;
        }
    }
</style>