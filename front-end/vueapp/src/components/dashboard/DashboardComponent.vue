<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-2 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("dashboard.title") }}</h5>
                        <p>
                            <small class="text-muted">{{ $t("dashboard.subtitle") }}</small>
                        </p>
                    </div>
                    <div class="plan-box plan-right">
                        <small class="text-muted">{{ $t("plan.current") }}</small><br>
                        <span class="plan-title">{{ $t("plan.enterprise") }}</span>
                    </div>
                </div>
            </div>
            <div class="d-flex justify-content-between align-items-center mb-3">
                <div class="row position-relative">
                    <div class="col" v-outsideClick="handleOutsideClick">
                        <button
                            class="btn btn-outlined-light btn-sm border d-flex align-items-center justify-content-between"
                            style="width: 200px;"
                            @click="toggleDateFilter"
                        >
                            {{ presetDate() }}
                            <LucideIcon v-if="showDateFilter" icon="ChevronUp" :size="17" />
                            <LucideIcon v-else icon="ChevronDown" :size="17" />
                        </button>
                        <div 
                            v-if="showDateFilter" 
                            class="position-absolute" 
                            style="z-index: 1050; width: 500px;"
                        >
                            <DashboardDateFilter 
                                @close="showDateFilter = false"
                                @filterData="filterData"
                            />
                        </div>
                    </div>
                    <div class="col">
                        <button class="btn btn-primary btn-sm">
                            <LucideIcon icon="RefreshCcw" :size="17" />
                            Atualizar
                        </button>
                    </div>
                </div>
                <!-- <button class="btn btn-outlined-primary btn-sm">
                    <LucideIcon icon="ArrowDownToLine" :size="17" />
                    {{ $t("dashboard.exportBtn") }}
                </button> -->
            </div>
            <div class="card mb-3">
                <div class="card-body text-center">
                    <div class="d-inline-flex align-items-center justify-content-center gap-1 mb-2">
                        <span class="me-1">{{ $t("dashboard.totalWTC") }}</span>
                        <LucideIcon v-tooltip.right="$t('dashboard.WTCText')" icon="Info" :size="17" />
                    </div>
                    <h2 class="mb-0 fw-bold text-primary">{{ totalWTC }}</h2>
                </div>
            </div>
            <TokensGraph
                :key="datesChange"
                :rangeDates="dateRange"
            />
            <PagesProcessedGraph
                :key="datesChange"
                :rangeDates="dateRange"
            />
            <WorkflowsGraph
                :key="datesChange"
                :rangeDates="dateRange"
            />
        </div>
    </main>
</template>

<script>
    import TokensGraph from '@/components/dashboard/graphs/TokensGraph.vue';
    import PagesProcessedGraph from '@/components/dashboard/graphs/PagesProcessedGraph.vue';
    import WorkflowsGraph from '@/components/dashboard/graphs/WorkflowsGraph.vue';
    import DashboardDateFilter from '@/components/dashboard/DashboardDateFilter.vue';
    import DashboardServices from '@/services/dashboard/DashboardServices';
    export default {
        components: {
            DashboardDateFilter,
            TokensGraph,
            WorkflowsGraph,
            PagesProcessedGraph,
        },
        data: () => ({
            showDateFilter: false,
            datesChange: 0,
            filters: {
                preset: "currentMonth",
                start: "",
                end: "",
            },
            totalWTC: 0,
        }),
        computed: {
            dateRange() {
                return {
                    start: this.filters.start,
                    end: this.filters.end,
                }
            },
        },
        methods: {
            toggleDateFilter() {
                this.showDateFilter = !this.showDateFilter;
            },
            filterData(filters) {
                this.filters = filters;
                this.datesChange++;
            },
            handleOutsideClick() {
                if (this.showDateFilter) {
                    this.showDateFilter = false;
                }
            },
            presetDate() {
                return this.$t(`dashboard.filters.${this.filters.preset}`);
            },
            getDashboardData() {
                DashboardServices.getMainDashboardData()
                    .then((response) => {
                        console.log(response)
                    });
            }
        },
        created() {
            this.getDashboardData();
        }
    }
</script>

<style scoped>
    .plan-box {
        background: #eef3ff;
        border-radius: 12px;
        padding: 12px 20px;
        display: inline-block;
        text-align: right;
        border: 1px solid #d5e0ff;
    }

    .plan-title {
        color: #0056d2;
        font-weight: 600;
        font-size: 1rem;
    }

    .plan-right {
        margin-left: auto;
        display: block; /* auto funciona apenas com block ou flex item */
        width: fit-content;
    }
</style>