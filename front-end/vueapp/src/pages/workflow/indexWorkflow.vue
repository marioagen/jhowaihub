<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("workflow.title") }}</h5>
                        <p>
                            <small class="text-muted">{{ $t("workflow.subtitle") }}</small>
                        </p>
                    </div>
                    <button 
                        class="btn btn-primary btn-sm" 
                        @click="redirectToForm"
                    >
                        <LucideIcon icon="Plus" size="17" />
                        {{ $t("workflow.createBtn") }}
                    </button>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <button 
                            class="btn btn-success btn-sm" 
                            @click="editWorkflow"
                        >
                            Edit
                        </button>
                        <button 
                            class="btn btn-danger btn-sm ms-2" 
                            @click="deleteWorkflow"
                        >
                            Delete
                        </button>
                    </div>
                </div>

            </div>
        </div>

        <FullscreenLoadingComponent 
            v-if="isDeleting"
        />
    </main>    
</template>

<script>
    import FullscreenLoadingComponent from "@/components/global/FullscreenLoadingComponent.vue";
    import WorkflowService from "@/services/workflow/WorkflowService";

    export default {
        name: "QuizzesPage",
        data() {
            return {
                crumbsData: [],
                entitySearch: {},
                resetInputSearch: false,
                isDeleting: false,
            };
        },
        components: {
            FullscreenLoadingComponent,
        },
        watch: {
            "$store.state.userProfile.language": function () {
                this.setEntitySearch();
            },
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
            redirectToForm() {
                this.$router.push({ name: "NewWorkflow" });
            },
            editWorkflow() {
                this.$router.push({ 
                    name: "EditWorkflow", 
                    params: {
                        id: 1,
                    }, 
                });
            },
            deleteWorkflow() {
                this.isDeleting = true;
                WorkflowService.deleteWorkflow(4)
                    .then((status) => {
                        console.log(status)
                        if(status) {
                            return this.$notify({
                                title: 'Workflow',
                                message: 'workflow.removeSuccess',
                                variant: 'success',
                                icon: 'CircleCheckBig',
                            });
                        }
                        this.$notify({
                            title: 'Workflow',
                            message: 'workflow.removeError',
                            variant: 'danger',
                            icon: 'CircleX',
                        });
                    })
                    .finally(() => {
                        this.isDeleting = false;
                    })
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