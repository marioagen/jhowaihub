<template>
    <div class="card mb-3">
        <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <div class="d-flex align-items-center gap-2">
                    <h6 class="mb-0 fw-bold">
                        {{ $t("dashboard.graphs.tokenGraphTitle") }}
                    </h6>
                    <LucideIcon
                        v-tooltip.right="$t('dashboard.graphs.tokensTooltip')"
                        icon="Info"
                        :size="17"
                    />
                </div>
                <div
                    v-if="currentIA"
                    class="d-flex align-items-center gap-2"
                >
                    <button
                        class="btn btn-outlined-primary btn-sm"
                        @click="previousIA"
                    >
                        <LucideIcon
                            icon="ChevronLeft"
                            :size="17"
                            :class="currentIAIndex === 0 ? 'disabled' : ''"
                            class="text-muted"
                        />
                    </button>
                    <span class="mb-0">
                        {{ currentIA.name }}
                    </span>
                    <button
                        class="btn btn-outlined-primary btn-sm"
                        @click="nextIA"
                    >
                        <LucideIcon
                            icon="ChevronRight"
                            :size="17"
                            :class="currentIAIndex === IAList.length - 1 ? 'disabled' : ''"
                            class="text-muted"
                        />
                    </button>
                </div>
            </div>
            <div class="card mb-3">
                <div class="card-body">
                    <h6>
                        {{ $t("dashboard.graphs.totalTokens") }}
                    </h6>
                    <h4 class="mb-0 fw-bold">
                        {{ totalTokens }}
                    </h4>
                    <span>
                        {{ $t("dashboard.graphs.unitValue") }}
                        {{ usageUnitTokens }}
                    </span>
                    <hr />
                    <span class="mt-1">
                        {{ $t("dashboard.graphs.periodTotal") }}
                    </span>
                    <h4 class="mb-0 fw-bold text-primary">
                        {{ formatDecimalValue(calculatedTotal) }}
                    </h4>
                </div>
            </div>
            <h6>
                {{ $t("dashboard.graphs.tokenGraphSubtitle") }}
            </h6>
            <BarGraphComponent
                v-if="isLoaded"
                :options="graph.options"
                :series="graph.series"
            />
            <LoadingComponent v-else />
        </div>
    </div>
</template>
<script>
    import BarGraphComponent from "@/components/global/graphs/BarGraphComponent.vue";
    import LoadingComponent from "@/components/global/LoadingComponent.vue";
    import DashboardServices from "@/services/dashboard/DashboardServices";
    import { formatDecimalValue } from "@/helpers/number";
    export default {
        components: {
            BarGraphComponent,
            LoadingComponent,
        },
        props: {
            start: {
                type: String,
                required: true,
            },
            end: {
                type: String,
                required: true,
            },
            workflowIds: {
                type: Array,
                required: false,
                default: () => [],
            },
            usageUnits: {
                type: Array,
                required: true,
            },
        },
        emits: ["setTotalTokens"],
        data: () => ({
            isLoaded: false,
            IAList: [],
            currentIAIndex: 0,
            previousTotalTokens: 0,
            totalCost: 0,
            graph: {
                options: {
                    chart: {
                        id: "sales-bar",
                        toolbar: {
                            show: false,
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
                        categories: [],
                    },
                    colors: ["#10315B"],
                },
                series: [
                    {
                        name: "Tokens",
                        data: [],
                    },
                ],
            },
        }),
        created() {
            this.getIAList();
        },
        watch: {
            workflowIds() {
                this.getTokensData();
            },
            start() {
                this.getTokensData();
            },
            end() {
                this.getTokensData();
            },
        },
        computed: {
            currentIA() {
                return this.IAList[this.currentIAIndex] ?? undefined;
            },
            totalTokens() {
                return this.graph.series[0].data.reduce((a, b) => a + b, 0);
            },
            usageUnitTokens() {
                if (!Array.isArray(this.usageUnits) || this.usageUnits.length === 0) {
                    return "0";
                }
                return (
                    this.usageUnits.find(
                        (item) =>
                            item.modelEmbeddingId === (this.IAList[this.currentIAIndex]?.id ?? 0)
                    )?.value ?? "0"
                );
            },
            calculatedTotal() {
                const unit = parseFloat(this.usageUnitTokens) || 0;
                const total = Number(this.totalTokens) || 0;
                return unit * total;
            },
        },
        methods: {
            formatDecimalValue,
            getIAList() {
                DashboardServices.GetUsedModels().then((response) => {
                    if (response && !response.error) {
                        this.IAList = response;
                        if (this.IAList.length > 0) {
                            this.currentIAIndex = 0;
                            this.getTokensData();
                            this.getTotalCost();
                        } else {
                            this.isLoaded = true;
                        }
                    }
                });
            },
            getTokensData() {
                if (!this.currentIA) return;
                this.isLoaded = false;
                let params = {
                    start: this.start,
                    end: this.end,
                    id: this.currentIA.id,
                };

                if (this.workflowIds.length > 0 && !this.workflowIds.includes(null)) {
                    params.workflowIds = this.workflowIds;
                }

                DashboardServices.GetTokensByModel(params)
                    .then((response) => {
                        if (response && !response.error) {
                            this.graph.options = {
                                ...this.graph.options,
                                xaxis: {
                                    categories: response.map((item) => item.date),
                                },
                            };
                            this.graph.series = [
                                {
                                    name: "Tokens",
                                    data: response.map((item) => item.value),
                                },
                            ];
                        }
                    })
                    .finally(() => {
                        this.isLoaded = true;
                    });
            },
            getTotalCost() {
                let paramsTotalCost = {
                    start: this.start,
                    end: this.end,
                };
                this.isLoaded = false;
                DashboardServices.GetTotalUsageCost(paramsTotalCost)
                    .then((response) => {
                        if (response && !response.error) {
                            this.totalCost = response;
                            this.setTotalTokens();
                        }
                    })
                    .finally(() => {
                        this.isLoaded = true;
                    });
            },
            setTotalTokens() {
                this.$emit("setTotalTokens", this.totalCost);
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

                this.currentIAIndex =
                    (this.currentIAIndex - 1 + this.IAList.length) % this.IAList.length;
                this.getTokensData();
            },
        },
    };
</script>
<style scoped>
    .disabled {
        opacity: 0.3;
    }
</style>
