<template>
    <div class="card mb-3">
        <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <div class="d-flex align-items-center gap-2">
                    <h6 class="mb-0 fw-bold">{{ $t("dashboard.graphs.tokenGraphTitle") }}</h6>
                    <LucideIcon v-tooltip.right="$t('dashboard.graphs.tokensTooltip')" icon="Info" :size="17" />
                </div>
                <div class="d-flex align-items-center gap-2">
                    <button class="btn btn-outlined-primary btn-sm" @click="previousIA">
                        <LucideIcon icon="ChevronLeft" :size="17" :class="currentIAIndex === 0 ? 'disabled' : ''" />
                    </button>
                    <span class="mb-0">
                        {{ currentIA.name }}
                    </span>
                    <button class="btn btn-outlined-primary btn-sm" @click="nextIA">
                        <LucideIcon icon="ChevronRight" :size="17"
                            :class="currentIAIndex === IAList.length - 1 ? 'disabled' : ''" />
                    </button>
                </div>
            </div>
            <div class="card ms-4 me-4 mb-3">
                <div class="card-body">
                    <h6>{{ $t("dashboard.graphs.totalTokens") }}</h6>
                    <h4 class="mb-0 fw-bold">{{ totalTokens }}</h4>
                    <span> {{ $t("dashboard.graphs.unitValue") }} {{ usageUnitTokens }}</span>
                    <hr />
                    <span class="mt-1">{{ $t("dashboard.graphs.periodTotal") }}</span>
                    <h4 class="mb-0 fw-bold text-primary">{{ (totalTokens * usageUnitTokens).toFixed(5) }}</h4>
                </div>
            </div>
            <h6>{{ $t("dashboard.graphs.tokenGraphSubtitle") }}</h6>
            <LoadingComponent v-if="isLoading" />
            <BarGraphComponent v-else :options="graph.options" :series="graph.series" />
        </div>
    </div>
</template>

<script>
import BarGraphComponent from '@/components/global/graphs/BarGraphComponent.vue';
import LoadingComponent from '@/components/global/LoadingComponent.vue';
import DashboardServices from '@/services/dashboard/DashboardServices';
export default {
    components: {
        BarGraphComponent,
        LoadingComponent,
    },
    props: {
        usageUnits: {
            type: Array,
            required: true,
        },
    },
    emits: ['setTotalTokens'],
    data: () => ({
        start: null,
        end: null,
        isLoading: false,
        IAList: [],
        currentIAIndex: 0,
        previousTotalTokens: 0,
        graph: {
            options: {
                chart: {
                    id: 'sales-bar',
                    toolbar: {
                        show: false
                    },
                },
                plotOptions: {
                    bar: {
                        borderRadius: 5,
                    },
                },
                dataLabels: {
                    enabled: false,
                },
                xaxis: {
                    categories: []
                },
                colors: ['#10315B']
            },
            series: [
                {
                    name: 'Tokens',
                    data: []
                }
            ]
        },
    }),
    created() {
        this.getIAList();
    },
    watch: {
        usageUnits() {
            this.setTotalTokens();
        }
    },
    computed: {
        currentIA() {
            return this.IAList[this.currentIAIndex] || { name: 'No IA selected', id: 0 };
        },
        totalTokens() {
            return this.graph.series[0].data.reduce((a, b) => a + b, 0);
        },
        usageUnitTokens() {
            if (this.usageUnits.length === 0) {
                return 0;
            }
            return this.usageUnits.find(item => item.modelEmbeddingId === (this.IAList[this.currentIAIndex]?.id ?? 0))?.value ?? 0;
        },
    },
    methods: {
        getTokensData() {
            if (!this.currentIA) return;
            this.isLoading = true;
            let params = {
                start: this.start,
                end: this.end,
                id: this.currentIA.id
            };
            DashboardServices.GetTokensByModel(params)
                .then((response) => {
                    if (response && !response.error) {
                        this.graph.options = {
                            ...this.graph.options,
                            xaxis: {
                                categories: response.map(item => item.date)
                            }
                        };
                        this.graph.series = [{
                            name: 'Tokens',
                            data: response.map(item => item.value)
                        }];
                    }
                })
                .finally(() => {
                    this.isLoading = false;
                    this.setTotalTokens();
                });
        },
        setTotalTokens() {
            var total = this.usageUnitTokens * this.totalTokens;
            this.$emit('setTotalTokens', total - this.previousTotalTokens);
            this.previousTotalTokens = total;
        },
        getIAList() {
            DashboardServices.GetUsedModels()
                .then((response) => {
                    if (response && !response.error) {
                        this.IAList = response;
                        if (this.IAList.length > 0) {
                            this.currentIAIndex = 0;
                            this.getTokensData();
                        }
                    }
                });
        },
        nextIA() {
            if (this.IAList.length === 0) return;
            if (this.currentIAIndex >= this.IAList.length - 1) return;

            this.currentIAIndex = (this.currentIAIndex + 1) % this.IAList.length;
            this.getTokensData();
        },
        previousIA() {
            if (this.IAList.length === 0) return;
            if (this.currentIAIndex <= 0) return;

            this.currentIAIndex = (this.currentIAIndex - 1 + this.IAList.length) % this.IAList.length;
            this.getTokensData();
        },
        updateGraph(start, end) {
            this.previousTotalTokens = 0;
            this.start = start;
            this.end = end;
            this.getIAList();
        }
    },
}
</script>
<style scoped>
.disabled {
    opacity: 0.3;
}
</style>