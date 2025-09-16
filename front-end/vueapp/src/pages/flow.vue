<template>
    <main :key="changeLanguage">
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("types.title") }}</h5>
                        <p>
                            <small class="text-muted">{{ $t("types.subtitle") }}</small>
                        </p>
                    </div>
                    <button class="btn btn-primary btn-sm" @click="openModalType">
                        <LucideIcon icon="Plus" :size="17" />
                        {{ $t("types.createBtn") }}
                    </button>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <SearchComponent :entity="entitySearch" :resetInput="resetInputSearch" @search="filterList" />
                    </div>
                </div>

                <TypesTable ref="TypesTable" />
            </div>
            <TypesModal :isEdit="false" :type="modalType" @reload="reloadTable" ref="TypesModal" />
        </div>
    </main>
</template>

<script>
    import TypesTable from "@/components/types/TypesTable.vue";
    import SearchComponent from "@/components/global/SearchComponent.vue";
    import TypesModal from "@/components/types/TypesModal.vue";

    export default {
        name: "TypePage",
        emits: ["showAlertToast"],
        data() {
            return {
                crumbsData: [],
                entitySearch: {},
                resetInputSearch: false,
                modalType: {
                    name: "",
                },
                changeLanguage: false,
            };
        },
        components: {
            SearchComponent,
            TypesTable,
            TypesModal,
        },
        methods: {
            filterList(obj) {
                this.$refs.TypesTable.filterList(obj.search);
            },
            openModalType() {
                this.$refs.TypesModal.open();
            },
            reloadTable() {
                this.$refs.TypesModal.close();
                this.$refs.TypesTable.reload();
            },
        },
    };
</script>