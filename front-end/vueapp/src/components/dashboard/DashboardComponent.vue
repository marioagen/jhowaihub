<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-2 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">
                            {{ $t("dashboard.title") }}
                        </h5>
                        <p>
                            <small class="text-muted">
                                {{ $t("dashboard.subtitle") }}
                            </small>
                        </p>
                    </div>
                    <div class="plan-box plan-right">
                        <small>
                            {{ $t("plan.current") }}
                        </small>
                        <br />
                        <span class="plan-title">
                            {{ plan }}
                        </span>
                        <br />
                        <span class="plan-subtitle">WTCs:{{ wtcIncluded }}</span>
                    </div>
                </div>
            </div>
            <div class="d-flex justify-content-between align-items-center mb-3">
                <div class="row position-relative w-50">
                    <div
                        class="col-4"
                        v-outsideClick="handleOutsideClick"
                    >
                        <div class="position-relative">
                            <button
                                class="form-select form-select-sm w-100 text-start date-filter-btn"
                                style="max-width: 200px"
                                @click="toggleDateFilter"
                            >
                                {{ presetDate() }}
                            </button>
                            <div
                                v-if="showDateFilter"
                                class="position-absolute"
                                style="z-index: 1050; width: 500px"
                            >
                                <DashboardDateFilter
                                    @close="showDateFilter = false"
                                    @filterData="filterData"
                                    :isLoading="isLoading"
                                />
                            </div>
                        </div>
                    </div>
                    <div class="col-4">
                        <select
                            v-model="selectedWorkflow"
                            class="form-select form-select-sm workflow-filter-select"
                            @change="onWorkflowSelected"
                            style="max-width: 250px"
                            multiple
                            size="1"
                        >
                            <option :value="null">
                                {{ $t("dashboard.filters.allWorkflows") }}
                            </option>
                            <option :value="-1">{{ $t("dashboard.filters.unclassified") }}</option>
                            <option
                                v-for="workflow in workflows"
                                :key="workflow.id"
                                :value="workflow.id"
                            >
                                {{ workflow.name }}
                            </option>
                        </select>
                    </div>
                    <div class="col-4">
                        <button
                            class="btn btn-primary btn-sm"
                            @click="proccessTenantMetrics()"
                        >
                            <LucideIcon
                                icon="RefreshCcw"
                                :size="17"
                                :class="{
                                    'animate-spin': isLoading,
                                }"
                            />
                            {{ $t("dashboard.update") }}
                        </button>
                    </div>
                </div>
                <div class="row">
                    <small class="text-muted">
                        {{ $t("dashboard.refreshText") }}
                    </small>
                </div>
            </div>
            <div class="card mb-3">
                <div class="card-body text-center">
                    <div class="d-inline-flex align-items-center justify-content-center gap-1 mb-2">
                        <span class="me-1">
                            {{ $t("dashboard.totalWTC") }}
                        </span>
                        <LucideIcon
                            v-popover.right="$t('dashboard.WTCText')"
                            icon="Info"
                            :size="17"
                            style="cursor: pointer"
                        />
                    </div>
                    <LoadingComponent v-if="isLoading" />
                    <h2
                        v-else
                        class="mb-0 fw-bold text-primary"
                    >
                        {{ Math.trunc(totalWTC) }}
                    </h2>
                </div>
            </div>
            <div class="row m-0">
                <div class="col-12 col-md-6 ps-0 pe-2">
                    <TokensGraph
                        :start="filters.start"
                        :end="filters.end"
                        :workflowIds="filters.workflowIds"
                        :key="datesChange"
                        :usageUnits="usageUnits"
                        :isLoading="isLoading"
                        @setTotalTokens="setTotalWTC"
                        ref="TokensGraph"
                    />
                </div>
                <div class="col-12 col-md-6 pe-0 ps-2">
                    <PagesProcessedGraph
                        :start="filters.start"
                        :end="filters.end"
                        :workflowIds="filters.workflowIds"
                        :key="datesChange"
                        :usageUnits="usageUnits"
                        :isLoading="isLoading"
                        @setTotalPages="setTotalWTC"
                        ref="PagesProcessedGraph"
                    />
                </div>
                <div class="col-12 col-md-6 ps-0 pe-2">
                    <WorkflowsAutomaticGraph
                        :start="filters.start"
                        :end="filters.end"
                        :workflowIds="filters.workflowIds"
                        :key="datesChange"
                        :usage-units="usageUnits"
                        @total-calculated="setTotalWTC"
                    />
                </div>
                <div class="col-12 col-md-6 pe-0 ps-2">
                    <WorkflowsExecutionGraph
                        :start="filters.start"
                        :end="filters.end"
                        :workflowIds="filters.workflowIds"
                        :key="datesChange"
                        :usage-units="usageUnits"
                        @total-calculated="setTotalWTC"
                    />
                </div>
            </div>
        </div>
    </main>
</template>
<script>
    import TokensGraph from "@/components/dashboard/graphs/TokensGraph.vue";
    import PagesProcessedGraph from "@/components/dashboard/graphs/PagesProcessedGraph.vue";
    import WorkflowsAutomaticGraph from "@/components/dashboard/graphs/WorkflowsAutomaticGraph.vue";
    import WorkflowsExecutionGraph from "@/components/dashboard/graphs/WorkflowsExecutionGraph.vue";
    import DashboardDateFilter from "@/components/dashboard/DashboardDateFilter.vue";
    import DashboardServices from "@/services/dashboard/DashboardServices";
    import WorkflowService from "@/services/workflow/WorkflowService";
    import store from "@/store";
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    export default {
        components: {
            DashboardDateFilter,
            TokensGraph,
            WorkflowsAutomaticGraph,
            WorkflowsExecutionGraph,
            PagesProcessedGraph,
            LoadingComponent,
        },
        data() {
            const today = new Date();
            const first = new Date(today.getFullYear(), today.getMonth(), 1);
            return {
                isLoading: false,
                showDateFilter: false,
                datesChange: 0,
                filters: {
                    preset: "currentMonth",
                    start: first.toISOString().slice(0, 10),
                    end: today.toISOString().slice(0, 10),
                    workflowIds: [],
                },
                totalWTC: 0,
                usageUnits: [],
                plan: "",
                wtcIncluded: 0,
                workflows: [],
                selectedWorkflow: [null],
                previousWorkflow: [null],
            };
        },
        methods: {
            toggleDateFilter() {
                this.showDateFilter = !this.showDateFilter;
            },
            handleOutsideClick() {
                if (this.showDateFilter) {
                    this.showDateFilter = false;
                }
            },
            filterData(filters) {
                this.isLoading = true;
                this.filters = {
                    ...filters,
                    workflowIds: this.selectedWorkflow,
                };
                this.totalWTC = 0;
                this.datesChange++;

                setTimeout(() => {
                    this.isLoading = false;
                }, 500);
            },
            presetDate() {
                return this.$t(`dashboard.filters.${this.filters.preset}`);
            },
            onWorkflowSelected() {
                this.$nextTick(() => {
                    const added = this.selectedWorkflow.filter(
                        (item) => !this.previousWorkflow.includes(item)
                    );
                    const removed = this.previousWorkflow.filter(
                        (item) => !this.selectedWorkflow.includes(item)
                    );

                    if (added.includes(null) && this.selectedWorkflow.length > 1) {
                        this.selectedWorkflow = [null];
                        this.previousWorkflow = [null];
                        this.filterData(this.filters);
                        return;
                    }

                    if (
                        added.length > 0 &&
                        this.previousWorkflow.includes(null) &&
                        !added.includes(null)
                    ) {
                        this.selectedWorkflow = this.selectedWorkflow.filter(
                            (item) => item !== null
                        );
                        this.previousWorkflow = [...this.selectedWorkflow];
                        this.filterData(this.filters);
                        return;
                    }

                    if (this.selectedWorkflow.length === 0) {
                        this.selectedWorkflow = [null];
                        this.previousWorkflow = [null];
                        this.filterData(this.filters);
                        return;
                    }

                    this.previousWorkflow = [...this.selectedWorkflow];
                    this.filterData(this.filters);
                });
            },
            getDashboardData() {
                const filtersWithWorkflow = {
                    ...this.filters,
                    workflowIds: this.selectedWorkflow,
                };
                this.previousWorkflow = [...this.selectedWorkflow];
                this.filterData(filtersWithWorkflow);
                DashboardServices.GetUsageUnits(this.filters).then((response) => {
                    this.usageUnits = response;
                });
            },
            getWorkflows() {
                WorkflowService.getWorkflowCompleteList().then((response) => {
                    if (response && !response.error) {
                        this.workflows = response;
                    }
                });
            },
            getPlan() {
                DashboardServices.GetPlan(store.state.userProfile.tenant).then((response) => {
                    this.wtcIncluded = response.wtcIncluded;
                    this.plan = response.plan.toUpperCase();
                });
            },
            setTotalWTC(total) {
                const value = Number(total);
                if (!Number.isFinite(value)) return;
                this.totalWTC += value;
            },
            proccessTenantMetrics() {
                this.isLoading = true;
                DashboardServices.ProcessMetricsByTenant()
                    .then(() => {
                        this.filterData(this.filters);
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
        },
        mounted() {
            this.getWorkflows();
            this.getDashboardData();
            this.getPlan();
        },
    };
</script>
<style scoped>
    .plan-box {
        background: var(--color-bg-subscription-card) !important;
        border-radius: 12px;
        padding: 12px 20px;
        display: inline-block;
        text-align: right;
        border: 1px solid var(--color-border-subscription-card) !important;
    }

    .plan-title {
        color: var(--color-title-subscription-card) !important;
        font-weight: 600;
        font-size: 1rem;
    }

    .plan-subtitle {
        color: var(--color-text-subscription-card) !important;
        font-weight: 400;
        font-size: 0.8rem;
    }

    .plan-right {
        margin-left: auto;
        display: block;
        width: fit-content;
    }

    .animate-spin {
        animation: spin 1s linear infinite;
        color: white;
    }

    .date-filter-btn {
        background-color: var(--color-bg-form-control) !important;
        border-color: var(--color-border-form-control) !important;
        color: var(--color-body-content) !important;
    }

    .date-filter-btn:focus,
    .date-filter-btn:active {
        background-color: var(--color-bg-form-control) !important;
        border-color: var(--color-bg-form-outline) !important;
        color: var(--color-body-content) !important;
        box-shadow: 0 0 0 0.25rem var(--bs-blue-rgb) !important;
    }

    .workflow-filter-select {
        background-color: var(--color-bg-form-control) !important;
        border-color: var(--color-border-form-control) !important;
        color: var(--color-body-content) !important;
    }

    .workflow-filter-select:focus {
        background-color: var(--color-bg-form-control) !important;
        border-color: var(--color-bg-form-outline) !important;
        color: var(--color-body-content) !important;
        box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25) !important;
    }

    .workflow-filter-select option {
        background-color: var(--color-bg-dropdown-menu) !important;
        color: var(--color-dropdown-menu) !important;
    }

    @keyframes spin {
        from {
            transform: rotate(0deg);
        }

        to {
            transform: rotate(360deg);
        }
    }
</style>
