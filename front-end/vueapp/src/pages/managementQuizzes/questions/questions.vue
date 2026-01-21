<template>
    <main>
        <div class="container-fluid scroll-area mx-0 p-0">
            <div class="mt-3 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("questions.title") }}</h5>
                        <p>
                            <small class="text-muted">{{ $t("questions.subtitle") }}</small>
                        </p>
                    </div>
                    <button class="btn btn-primary btn-sm" @click="openModalQuestion">
                        <LucideIcon icon="Plus" :size="17" />
                        {{ $t("questions.createBtn") }}
                    </button>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <SearchComponent :entity="entitySearch" :resetInput="resetInputSearch" @search="filterList" />
                    </div>
                </div>

                <QuestionsTable ref="QuestionsTable" />
            </div>

            <QuestionsModal :isEdit="false" :type="modalQuestion" @reload="reloadTable" ref="QuestionsModal" />
        </div>
    </main>
</template>

<script>
import SearchComponent from "@/components/global/SearchComponent.vue";
import QuestionsModal from "@/components/questions/QuestionsModal.vue";
import QuestionsTable from "@/components/questions/QuestionsTable.vue";

export default {
    name: "QuestionsPage",
    data() {
        return {
            crumbsData: [],
            entitySearch: {},
            resetInputSearch: false,
            modalQuestion: {
                name: "",
            }
        };
    },
    components: {
        SearchComponent,
        QuestionsModal,
        QuestionsTable
    },
    methods: {
        setEntitySearch() {
            this.entitySearch = {
                screen: "question",
                labelInput: this.$t("filters.questionsInput"),
                placeholderInput: this.$t("filters.questionsInput"),
                labelButton: this.$t("questions.createBtn"),
            };
        },
        filterList(obj) {
            this.$refs.QuestionsTable.filterList(obj.search);
        },
        openModalQuestion() {
            this.$refs.QuestionsModal.open();
        },
        reloadTable() {
            this.$refs.QuestionsModal.close();
            this.$refs.QuestionsTable.reload();
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