<template>
    <div class="card mb-3">
        <div class="card-body">
            <div class="d-flex align-items-center gap-2 mb-3">
                <h6 class="mb-0 fw-bold">
                    {{ $t("dashboard.graphs.workflowsAutomaticGraphTitle") }}
                </h6>
                <LucideIcon
                    v-tooltip.right="$t('dashboard.graphs.workflowAutomaticTooltip')"
                    icon="Info"
                    :size="17"
                />
            </div>
            <div class="card mb-3">
                <div class="card-body">
                    <h6>
                        {{ $t("dashboard.graphs.workflowsAutomaticGraphTitle") }}
                    </h6>
                    <h4 class="mb-0 fw-bold">
                        {{ totalWorkflowsAutomatic }}
                    </h4>
                    <span>
                        {{ $t("dashboard.graphs.unitValue") }}
                        {{ usageUnitWorkflowAutomatic }}
                    </span>
                    <hr />
                    <span class="mt-1">
                        {{ $t("dashboard.graphs.periodTotal") }}
                    </span>
                    <h4 class="mb-0 fw-bold text-primary">
                        {{ totalWorkflowsAutomatic * usageUnitWorkflowAutomatic }}
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
        emits: ["totalCalculated"],
        data: () => ({
            isLoaded: false,
            graph: {
                options: {
                    chart: {
                        id: "workflows-automatic-bar",
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
                        name: "Automatic Workflows",
                        data: [],
                    },
                ],
            },
        }),
        created() {
            this.getWorkflowsAutomaticData();
        },
        computed: {
            totalWorkflowsAutomatic() {
                if (!this.isLoaded || !this.graph.series[0]?.data?.length) {
                    return 0;
                }
                return this.graph.series[0].data.reduce((a, b) => a + b, 0);
            },
            usageUnitWorkflowAutomatic() {
                if (!Array.isArray(this.usageUnits) || this.usageUnits.length === 0) {
                    return 0;
                }
                return (
                    this.usageUnits.find((item) => item.usageTypeName === ColTypeUsage.Automation)
                        ?.value ?? 0
                );
            },
            calculatedTotal() {
                return this.usageUnitWorkflowAutomatic * this.totalWorkflowsAutomatic;
            },
        },
        watch: {
            start() {
                this.getWorkflowsAutomaticData();
            },
            end() {
                this.getWorkflowsAutomaticData();
            },
            workflowIds() {
                this.getWorkflowsAutomaticData();
            },
            calculatedTotal(newValue) {
                this.$emit("totalCalculated", newValue);
            },
        },
        methods: {
            getWorkflowsAutomaticData() {
                let params = {
                    start: this.start,
                    end: this.end,
                    usageType: ColTypeUsage.Automation,
                };

                if (this.workflowIds.length > 0 && !this.workflowIds.includes(null)) {
                    params.workflowIds = this.workflowIds;
                }

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
                                    name: "Automatic Workflows",
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
