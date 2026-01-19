<template>
    <main>
        <div class="container-fluid mx-2">
            <div>
                <div
                    class="d-flex justify-content-between align-items-center mb-1"
                >
                    <div>
                        <h5 class="mb-0 fw-bold">
                            {{ $t("template.title") }}
                        </h5>
                        <p class="mb-1">
                            <small class="text-muted">
                                {{
                                    $t("template.subtitle")
                                }}
                            </small>
                        </p>
                    </div>
                    <button
                        class="btn btn-primary btn-sm"
                        @click="redirectToNewTemplate"
                    >
                        <LucideIcon
                            icon="Plus"
                            :size="17"
                        />
                        {{ $t("template.createTemplate") }}
                    </button>
                </div>
                <div class="card mb-2">
                    <div class="card-body p-3">
                        <div class="row">
                            <div class="col-12">
                                <TemplateFilters
                                    @setFilters="setFilters"
                                    class="ms-auto"
                                />
                            </div>
                        </div>
                    </div>
                </div>
                <TemplateTable ref="TemplateTable" />
            </div>
        </div>
    </main>
</template>
<script>
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import TemplateFilters from "@/components/templates/TemplateFilters.vue";
    import TemplateTable from "@/components/templates/TemplateTable.vue";

    export default {
        name: "TemplatesPage",
        components: {
            LoadingComponent,
            TemplateFilters,
            TemplateTable,
        },
        methods: {
            redirectToNewTemplate() {
                this.$router.push({ name: "TemplateNew" });
            },
            setFilters(filters) {
                this.$refs.TemplateTable.filters = filters;
                this.$refs.TemplateTable.getTemplates();
            },
        },
        mounted() {
            this.$refs.TemplateTable.getTemplates();
        },
    };
</script>
<style scoped>
    .flex {
        display: flex;
    }

    .bg-secondary {
        background-color: #f5f7fa !important;
        color: gray;
        border-color: #f5f7fa !important;
    }

    .font-size-sm {
        font-size: small;
    }

    .font-size-xs {
        font-size: x-small;
    }

    .card-container {
        max-height: 75vh;
        overflow-y: auto;
    }
</style>
