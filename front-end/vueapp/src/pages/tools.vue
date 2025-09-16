<template>
    <main :key="changeLanguage">
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("tools.title") }}</h5>
                        <p>
                            <small class="text-muted">{{ $t("tools.subtitle") }}</small>
                        </p>
                    </div>
                    <button class="btn btn-primary btn-sm" @click="openModalTool">
                        <LucideIcon icon="Plus" :size="17" />
                        {{ $t("tools.createBtn") }}
                    </button>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <ToolFilters 
                            @filter="filterData"
                        />
                    </div>
                </div>
            </div>
            <ToolsTable 
                ref="ToolsTable"
            />
            <ToolsModal
                :isEdit="false" 
                :type="modalTool" 
                @reload="reloadData"
                ref="ToolsModal" 
            />
        </div>
    </main>
</template>

<script>
    import ToolFilters from "@/components/tools/ToolFilters.vue";
    import ToolsTable from "@/components/tools/ToolsTable.vue";
    import ToolsModal from "@/components/tools/ToolsModal.vue";

    export default {
        name: "DocumentsPage",
        data() {
            return {
                modalTool: {
                    name: "",
                },
            };
        },
        components: {
            ToolFilters,
            ToolsModal,
            ToolsTable,
        },
        methods: {
            openModalTool() {
                this.$refs.ToolsModal.open();
            },
            reloadData() {
                this.$refs.ToolsTable.getTools();
            },
            filterData(filters) {
                this.$refs.ToolsTable.filters = filters;
                this.reloadData();
            },
        },
    };
</script>

<style scoped>
    .team-list {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
    }

    .team-badge {
        background-color: #f1f1f1;
        border: 1px solid #ccc;
        border-radius: 12px;
        padding: 4px 10px;
        font-size: 0.85rem;
        color: #333;
        white-space: nowrap;
    }

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

    .navbar-container {
        padding-top: 0px;
        padding: 0;
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
