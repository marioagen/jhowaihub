<template>
    <main>
        <div class="container-fluid mx-2">
            <div>
                <div class="d-flex justify-content-between align-items-center mb-1">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("template.title") }}</h5>
                        <p class="mb-1">
                            <small class="text-muted">{{ $t("template.subtitle") }}</small>
                        </p>
                    </div>
                    <button class="btn btn-primary btn-sm" @click="redirectToNewTemplate">
                        <LucideIcon icon="Plus" :size="17" />
                        {{ $t("template.createBtn") }}
                    </button>
                </div>
                <div class="card mb-2">
                    <div class="card-body p-3">
                        <div class="row">
                            <div class="col-12">
                                <TemplateFilters @setFilters="setFilters" class="ms-auto" />
                            </div>
                        </div>
                    </div>
                </div>
                <div v-if="isLoading">
                    <LoadingComponent />
                </div>
                <div v-else-if="hasList">
                    <div class="card custom-height">
                        <div class="card-body d-flex flex-column p-2 card-container">
                            <div class="kanban-wrapper">
                                <!-- <KanbanBoard :kanbanData="kanbanCards" :users="users" @reload="reloadKanban" /> -->
                            </div>
                        </div>
                    </div>
                </div>
                <div v-else class="text-center">
                    <span class="text-primary">{{ $t("template.notFound") }}</span>
                </div>
            </div>
        </div>
    </main>
</template>

<script>
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import TemplateFilters from "@/components/templates/TemplateFilters.vue";
    import TemplateService from "@/services/template/TemplateService";

    export default {
        name: "TemplatesPage",
        data() {
            return {
                templates: [],
                filteredTemplates: [],
                selectedOption: {
                    id: 0,
                    name: "Select a template",
                    teamName: "Select a team",
                    teamId: 0,
                },
                isLoading: false,
                filters: {
                    orderBy: "",
                    input: null,
                    method: null,
                },
            };
        },
        components: {
            LoadingComponent,
            TemplateFilters,
        },
        computed: {
            hasList() {
                return this.templates.length > 0;
            },
        },
        methods: {
            getTemplates() {
                this.isLoading = true;
                const params = {
                    input: this.filters.input,
                    orderBy: this.filters.orderBy,
                    method: this.filters.method,
                };

                TemplateService.getTemplates(params).then((response) => {
                    console.log(response);
                    if (response.error !== undefined) {
                        this.$notify({
                            title: "template.title",
                            message: "template.notFound",
                            variant: "danger",
                            icon: "CircleX",
                        });
                    }

                    this.templates = response;
                    this.filteredTemplates = response;
                    this.isLoading = false;
                });
            },
            selectOption(workflow) {
                if (!workflow?.id) return;

                this.isLoaded = false;
                this.isLoadedUsers = false;

                this.$store.commit("setLastSelectedWorkflow", {
                    id: workflow.id,
                    name: workflow.name,
                });

                this.selectedOption = {
                    id: workflow.id,
                    name: workflow.name,
                };

                this.getWorkflowStepsById(workflow.id);
                this.getUsersByTeams(workflow.teams);
            },
            redirectToNewTemplate() {
                // this.$router.push({ name: "DocumentsUpload" });
            },
            setFilters(filters) {
                this.filters = filters;
                this.getTemplates();
            },
        },
        created() {
            this.getTemplates();
        },
        async mounted() {},
        beforeUnmount() {},
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
