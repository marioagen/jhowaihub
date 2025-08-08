<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div class="d-flex justify-content-between align-items-center">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("labelWorkflowBoard") }}</h5>
                        <p>
                            <small class="text-muted">{{$t("labelWorkflowSubTitle")}}</small>
                        </p>
                    </div>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="flex flex-col items-start gap-4 flex-1 align-items-center">
                            <div>
                                <LucideIcon icon="Clock" size="14" class="me-2" />
                                <span>{{$t("labelWatchingWorkflow")}}</span>
                            </div>
                            <div class="dropdown">
                                <button class="btn btn-light border text-start"
                                        type="button"
                                        data-bs-toggle="dropdown"
                                        aria-expanded="false">
                                    <div class="fw-bold font-size-sm">Time Financeiro</div>
                                    <div class="text-muted font-size-xs">Processamento de Notas Fiscais</div>
                                </button>

                                <ul class="dropdown-menu">
                                    <li>
                                        <a class="dropdown-item" href="#" @click="selectTeam('RH', 'Folha de Pagamento')">
                                            <div class="fw-bold">Time RH</div>
                                            <div class="text-muted small">Folha de Pagamento</div>
                                        </a>
                                    </li>
                                    <li>
                                        <a class="dropdown-item" href="#" @click="selectTeam('TI', 'Infraestrutura')">
                                            <div class="fw-bold">Time TI</div>
                                            <div class="text-muted small">Infraestrutura</div>
                                        </a>
                                    </li>
                                </ul>
                            </div>
                            <div class="badge bg-secondary badge-custom">
                                <LucideIcon icon="Workflow" size="14" class="me-2" stroke="#0d6efd" />
                                <span>data.workflow</span>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="card mb-3 h-100">
                    <div class="card-body d-flex flex-column p-2 flex-md-row card-container">
                        <WorkflowCards>
                        </WorkflowCards>
                    </div>
                </div>
                <div class="card mb-3">
                    <div class="card-body">
                        <div class="flex flex-col items-start gap-4 flex-1 align-items-center">
                            <div>
                                <span class="me-1">Workflow:</span>
                                <b>data.workflow</b>
                            </div>
                            <div>
                                <span class="me-1">{{$t("labelTeam")}}:</span>
                                <b>data.team</b>
                            </div>
                            <div>
                                <span class="me-1">{{$t("labelTotalDocuments")}}</span>
                                <b>data.docs</b>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
</main>
</template>

<script>
    import WorkflowCards from "@/components/workflow/WorkFlowCards.vue";
    export default {
        name: "WorkflowDocuments",
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
        components:
        {
            WorkflowCards
        },
        watch: {
            "$store.state.userProfile.language": function () {
                this.setEntitySearch();
            },
        },
        methods: {}
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
        max-height: 70vh;
    }

</style>