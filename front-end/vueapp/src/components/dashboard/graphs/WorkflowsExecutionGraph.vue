<template>
    <div class="card mb-3">
        <div class="card-body">
            <div class="d-flex align-items-center gap-2 mb-3">
                <h6 class="mb-0 fw-bold">
                    {{ $t("dashboard.graphs.workflowsGraphTitle") }}
                </h6>
                <LucideIcon
                    v-tooltip.right="$t('dashboard.graphs.pagesTooltip')"
                    icon="Info"
                    :size="17"
                />
            </div>
            <div class="card mb-3">
                <div class="card-body">
                    <h6>
                        {{ $t("dashboard.graphs.workflowsGraphTitle") }}
                    </h6>
                    <h4 class="mb-0 fw-bold">
                        {{ totalWorkflows }}
                    </h4>
                    <span>
                        {{ $t("dashboard.graphs.unitValue") }}
                        {{ usageUnitWorkflow }}
                    </span>
                    <hr />
                    <span class="mt-1">
                        {{ $t("dashboard.graphs.periodTotal") }}
                    </span>
                    <h4 class="mb-0 fw-bold text-primary">
                        {{ totalWorkflows * usageUnitWorkflow }}
                    </h4>
                </div>
            </div>
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
    import { ColTypeUsage } from "@/constants/ColTypeUsage";
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
            usageUnits: {
                type: Array,
                required: true,
            },
        },
        emits: ["totalCalculated"],
        data: () => ({
            isLoaded: false,
            graph: {
                options: {
                    chart: {
                        id: "workflows-execution-bar",
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
                        name: "Workflows",
                        data: [],
                    },
                ],
            },
        }),
        created() {
            this.getWorkflowsData();
        },
        computed: {
            totalWorkflows() {
                if (!this.isLoaded || !this.graph.series[0]?.data?.length) {
                    return 0;
                }
                return this.graph.series[0].data.reduce((a, b) => a + b, 0);
            },
            usageUnitWorkflow() {
                if (!Array.isArray(this.usageUnits) || this.usageUnits.length === 0) {
                    return 0;
                }
                return (
                    this.usageUnits.find((item) => item.usageTypeName === ColTypeUsage.Execution)
                        ?.value ?? 0
                );
            },
            calculatedTotal() {
                return this.usageUnitWorkflow * this.totalWorkflows;
            },
        },
        watch: {
            start() {
                this.getWorkflowsData();
            },
            end() {
                this.getWorkflowsData();
            },
            calculatedTotal(newValue) {
                this.$emit("totalCalculated", newValue);
            },
        },
        methods: {
            getWorkflowsData() {
                let params = {
                    start: this.start,
                    end: this.end,
                    usageType: ColTypeUsage.Execution,
                };
                this.isLoaded = false;
                DashboardServices.GetByUsageType(params)
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
                                    name: "Workflows",
                                    data: response.map((item) => item.value),
                                },
                            ];
                        }
                    })
                    .finally(() => {
                        this.isLoaded = true;
                    });
            },
        },
    };
</script>
