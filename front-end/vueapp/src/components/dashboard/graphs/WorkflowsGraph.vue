<template>
    <div class="card mb-3">
        <div class="card-body">
            <div>
                <div
                    class="d-flex align-items-center gap-2 mb-3"
                >
                    <h6 class="mb-0 fw-bold">
                        {{
                            $t(
                                "dashboard.graphs.workflowsAutomaticGraphTitle"
                            )
                        }}
                    </h6>
                    <LucideIcon
                        v-tooltip.right="
                            $t(
                                'dashboard.graphs.workflowAutomaticTooltip'
                            )
                        "
                        icon="Info"
                        :size="17"
                    />
                </div>
                <div class="card ms-4 me-4 mb-3">
                    <div class="card-body">
                        <h6>
                            {{
                                $t(
                                    "dashboard.graphs.workflowsAutomaticGraphTitle"
                                )
                            }}
                        </h6>
                        <h4 class="mb-0 fw-bold">
                            {{ totalWorkflowsAutomatic }}
                        </h4>
                        <span>
                            {{
                                $t(
                                    "dashboard.graphs.unitValue"
                                )
                            }}
                            {{ usageUnitWorkflowAutomatic }}
                        </span>
                        <hr />
                        <span class="mt-1">
                            {{
                                $t(
                                    "dashboard.graphs.periodTotal"
                                )
                            }}
                        </span>
                        <h4
                            class="mb-0 fw-bold text-primary"
                        >
                            {{
                                totalWorkflowsAutomatic *
                                usageUnitWorkflowAutomatic
                            }}
                        </h4>
                    </div>
                </div>
                <BarGraphComponent
                    v-if="isLoadedWorkflows"
                    :options="graph.options"
                    :series="graph.series"
                />
                <LoadingComponent v-else />
            </div>
            <hr />
            <div>
                <div
                    class="d-flex align-items-center gap-2 mb-3"
                >
                    <h6 class="mb-0 fw-bold">
                        {{
                            $t(
                                "dashboard.graphs.workflowsGraphTitle"
                            )
                        }}
                    </h6>
                    <LucideIcon
                        v-tooltip.right="
                            $t(
                                'dashboard.graphs.pagesTooltip'
                            )
                        "
                        icon="Info"
                        :size="17"
                    />
                </div>
                <div class="card ms-4 me-4 mb-3">
                    <div class="card-body">
                        <h6>
                            {{
                                $t(
                                    "dashboard.graphs.workflowsGraphTitle"
                                )
                            }}
                        </h6>
                        <h4 class="mb-0 fw-bold">
                            {{ totalWorkflows }}
                        </h4>
                        <span>
                            {{
                                $t(
                                    "dashboard.graphs.unitValue"
                                )
                            }}
                            {{ usageUnitWorkflow }}
                        </span>
                        <hr />
                        <span class="mt-1">
                            {{
                                $t(
                                    "dashboard.graphs.periodTotal"
                                )
                            }}
                        </span>
                        <h4
                            class="mb-0 fw-bold text-primary"
                        >
                            {{
                                totalWorkflows *
                                usageUnitWorkflow
                            }}
                        </h4>
                    </div>
                </div>
                <BarGraphComponent
                    v-if="isLoadedAutomation"
                    :options="graph2.options"
                    :series="graph2.series"
                />
                <LoadingComponent v-else />
            </div>
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
        emits: ["setTotalExecution"],
        data: () => ({
            isLoadedWorkflows: false,
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
                        name: "Executions",
                        data: [],
                    },
                ],
            },
            isLoadedAutomation: false,
            graph2: {
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
                        name: "Executions",
                        data: [],
                    },
                ],
            },
        }),
        created() {
            this.getWorkflowsData();
            this.getWorkflowsAutomaticData();
        },
        computed: {
            totalWorkflows() {
                if (
                    !this.isLoadedWorkflows ||
                    !this.graph2.series[0]?.data?.length
                ) {
                    return 0;
                }
                return this.graph2.series[0].data.reduce(
                    (a, b) => a + b,
                    0
                );
            },
            totalWorkflowsAutomatic() {
                if (
                    !this.isLoadedAutomation ||
                    !this.graph.series[0]?.data?.length
                ) {
                    return 0;
                }
                return this.graph.series[0].data.reduce(
                    (a, b) => a + b,
                    0
                );
            },
            usageUnitWorkflowAutomatic() {
                if (
                    !Array.isArray(this.usageUnits) ||
                    this.usageUnits.length === 0
                ) {
                    return 0;
                }
                return (
                    this.usageUnits.find(
                        (item) =>
                            item.usageTypeName ===
                            ColTypeUsage.Automation
                    )?.value ?? 0
                );
            },
            usageUnitWorkflow() {
                if (
                    !Array.isArray(this.usageUnits) ||
                    this.usageUnits.length === 0
                ) {
                    return 0;
                }
                return (
                    this.usageUnits.find(
                        (item) =>
                            item.usageTypeName ===
                            ColTypeUsage.Execution
                    )?.value ?? 0
                );
            },
        },
        methods: {
            getWorkflowsData() {
                let params = {
                    start: this.start,
                    end: this.end,
                    usageType: ColTypeUsage.Execution,
                };
                this.isLoadedWorkflows = false;
                DashboardServices.GetByUsageType(params)
                    .then((response) => {
                        if (response && !response.error) {
                            this.graph2.options = {
                                ...this.graph2.options,
                                xaxis: {
                                    categories:
                                        response.map(
                                            (item) =>
                                                item.date
                                        ),
                                },
                            };
                            this.graph2.series = [
                                {
                                    name: "Workflows",
                                    data: response.map(
                                        (item) => item.value
                                    ),
                                },
                            ];
                        }
                    })
                    .finally(() => {
                        this.isLoadedWorkflows = true;
                        this.checkAndEmitTotal();
                    });
            },
            checkAndEmitTotal() {
                if (
                    this.isLoadedWorkflows &&
                    this.isLoadedAutomation
                ) {
                    this.setTotalExecution();
                }
            },
            setTotalExecution() {
                let totalExecution =
                    this.usageUnitWorkflow *
                        this.totalWorkflows +
                    this.usageUnitWorkflowAutomatic *
                        this.totalWorkflowsAutomatic;

                this.$emit(
                    "setTotalExecution",
                    totalExecution
                );
            },
            getWorkflowsAutomaticData() {
                let params = {
                    start: this.start,
                    end: this.end,
                    usageType: ColTypeUsage.Automation,
                };
                this.isLoadedAutomation = false;
                DashboardServices.GetByUsageType(params)
                    .then((response) => {
                        if (response && !response.error) {
                            this.graph.options = {
                                ...this.graph.options,
                                xaxis: {
                                    categories:
                                        response.map(
                                            (item) =>
                                                item.date
                                        ),
                                },
                            };
                            this.graph.series = [
                                {
                                    name: "Automatic Workflows",
                                    data: response.map(
                                        (item) => item.value
                                    ),
                                },
                            ];
                        }
                    })
                    .finally(() => {
                        this.isLoadedAutomation = true;
                        this.checkAndEmitTotal();
                    });
            },
            updateGraph(start, end) {
                this.start = start;
                this.end = end;
                this.getWorkflowsData();
                this.getWorkflowsAutomaticData();
            },
        },
    };
</script>
