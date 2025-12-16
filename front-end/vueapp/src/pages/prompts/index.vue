<template>
    <main>
        <div class="container-fluid">
            <div class="mt-3 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("prompts.title") }}</h5>
                        <p>
                            <small class="text-muted">{{ $t("prompts.subtitle") }}</small>
                        </p>
                    </div>
                    <div class="d-flex gap-2">
                        <button class="btn btn-primary btn-sm" @click="redirectToImport">
                            <LucideIcon icon="CloudDownload" :size="17" />
                            {{ $t("prompts.importPredefined") }}
                        </button>
                        <button class="btn btn-primary btn-sm" @click="redirectToNewPrompt">
                            <LucideIcon icon="Plus" :size="17" />
                            {{ $t("prompts.createPrompt") }}
                        </button>
                    </div>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <PromptFilters @filter="filterData"></PromptFilters>
                    </div>
                </div>
            </div>
            <div>
                <PromptComponent ref="PromptComponent" />
            </div>
        </div>
    </main>
</template>
<script>
    import PromptComponent from "@/components/prompts/PromptComponent.vue";
    import PromptFilters from "@/components/prompts/PromptFilter"
    export default {
        name: "PromptPage",
        emits: ['showAlertToast'],
        data() {
            return {
                entitySearch: {},
                resetInputSearch: false,
                sidebarData: "Type",
                queryPage: this.$route.query.page ? this.$route.query.page : 1,
                searchInput: "",
                searching: false,
                dataPrompt: [],
                loading: false,
                pagination: { currentPage: 0, count: 0, totalPages: 0 },
                modalAlertShow: false,
                modalEntity: {},
                isAscending: false,
                dataModal: {},
                colType: 2,
                selectedOption: 9,
                toastShow: false,
                toastColor: "",
                toastMessage: "",
                listIds: [],
                loadAllPrompts: true,
            }
        },
        components: {
            PromptComponent,
            PromptFilters,
        },
        watch: {
            filters: {
                handler(newFilters) {
                    this.$emit('filterData', newFilters);
                },
                deep: true
            },
            deep: true
        },
        methods: {
            redirectToNewPrompt: function () {
                this.$router.push({ name: "PromptNew" });
            },
            redirectToImport: function () {
                this.$router.push({ name: "PromptImport" });
            },
            reloadData() {
                this.$refs.PromptComponent.getList({ search: '', page: this.queryPage, type: null });
            },
            filterData(filters) {
                this.$refs.PromptComponent.filters = filters;
                this.reloadData();
            },
        },
}
</script>
