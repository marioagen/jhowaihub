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
                        <span class="plan-subtitle">
                            {{ wtcIncluded }}
                        </span>
                    </div>
                </div>
            </div>
            <div class="d-flex justify-content-between align-items-center mb-3">
                <div class="row position-relative">
                    <div
                        class="col"
                        v-outsideClick="handleOutsideClick"
                    >
                        <button
                            class="btn btn-outlined btn-sm d-flex align-items-center justify-content-between"
                            style="width: 200px"
                            @click="toggleDateFilter"
                        >
                            {{ presetDate() }}
                            <LucideIcon
                                v-if="showDateFilter"
                                icon="ChevronUp"
                                :size="17"
                                class="text-muted"
                            />
                            <LucideIcon
                                v-else
                                icon="ChevronDown"
                                :size="17"
                                class="text-muted"
                            />
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
                    <div class="col">
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
                            v-tooltip.right="$t('dashboard.WTCText')"
                            icon="Info"
                            :size="17"
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
                        :key="datesChange"
                        :usage-units="usageUnits"
                        @total-calculated="setTotalWTC"
                    />
                </div>
                <div class="col-12 col-md-6 pe-0 ps-2">
                    <WorkflowsExecutionGraph
                        :start="filters.start"
                        :end="filters.end"
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
                },
                totalWTC: 0,
                usageUnits: [],
                plan: "",
                wtcIncluded: 0,
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
                this.filters = filters;
                this.totalWTC = 0;
                this.datesChange++;

                setTimeout(() => {
                    this.isLoading = false;
                }, 500);
            },
            presetDate() {
                return this.$t(`dashboard.filters.${this.filters.preset}`);
            },
            getDashboardData() {
                this.filterData(this.filters);
                DashboardServices.GetUsageUnits(this.filters).then((response) => {
                    this.usageUnits = response;
                });
            },
            getPlan() {
                DashboardServices.GetPlan(store.state.userProfile.tenant).then((response) => {
                    this.wtcIncluded = response.wtcIncluded;
                    this.plan = response.plan.toUpperCase();
                });
            },
            setTotalWTC(total) {
                this.totalWTC += total;
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

    @keyframes spin {
        from {
            transform: rotate(0deg);
        }

        to {
            transform: rotate(360deg);
        }
    }
</style>
