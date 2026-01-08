<template>
    <div class="card mb-3">
        <div class="card-body">
            <div>
                <div class="d-flex align-items-center gap-2 mb-3">
                    <h6 class="mb-0 fw-bold">{{ $t("dashboard.graphs.workflowsAutomaticGraphTitle") }}</h6>
                    <LucideIcon v-tooltip.right="$t('dashboard.graphs.workflowAutomaticTooltip')" icon="Info"
                        :size="17" />
                </div>
                <div class="card ms-4 me-4 mb-3">
                    <div class="card-body">
                        <h6>{{ $t("dashboard.graphs.workflowsAutomaticGraphTitle") }}</h6>
                        <h4 class="mb-0 fw-bold">{{ totalWorkflowsAutomatic }}</h4>
                        <span> {{ $t("dashboard.graphs.unitValue") }} {{ usageUnitWorkflowAutomatic }}</span>
                        <hr />
                        <span class="mt-1">{{ $t("dashboard.graphs.periodTotal") }}</span>
                        <h4 class="mb-0 fw-bold text-primary">{{ (totalWorkflowsAutomatic *
                            usageUnitWorkflowAutomatic).toFixed(5) }}
                        </h4>
                    </div>
                </div>
                <LoadingComponent v-if="isLoadingWorkflows" />
                <BarGraphComponent v-else :options="graph.options" :series="graph.series" />
            </div>
            <hr />
            <div>
                <div class="d-flex align-items-center gap-2 mb-3">
                    <h6 class="mb-0 fw-bold">{{ $t("dashboard.graphs.workflowsGraphTitle") }}</h6>
                    <LucideIcon v-tooltip.right="$t('dashboard.graphs.pagesTooltip')" icon="Info" :size="17" />
                </div>
                <div class="card ms-4 me-4 mb-3">
                    <div class="card-body">
                        <h6>{{ $t("dashboard.graphs.workflowsGraphTitle") }}</h6>
                        <h4 class="mb-0 fw-bold">{{ totalWorkflows }}</h4>
                        <span> {{ $t("dashboard.graphs.unitValue") }} {{ usageUnitWorkflow }}</span>
                        <hr />
                        <span class="mt-1">{{ $t("dashboard.graphs.periodTotal") }}</span>
                        <h4 class="mb-0 fw-bold text-primary">{{ (totalWorkflows * usageUnitWorkflow).toFixed(5) }}</h4>
                    </div>
                </div>
                <LoadingComponent v-if="isLoadingAutomation" />
                <BarGraphComponent v-else :options="graph2.options" :series="graph2.series" />
            </div>
        </div>
    </div>
</template>

<script>
import BarGraphComponent from '@/components/global/graphs/BarGraphComponent.vue';
import LoadingComponent from '@/components/global/LoadingComponent.vue';
import DashboardServices from '@/services/dashboard/DashboardServices';
import { ColTypeUsage } from '@/constants/ColTypeUsage';
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
    emits: ['setTotalExecution'],
    data: () => ({
        start: null,
        end: null,
        isLoadingWorkflows: false,
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
                    name: 'Executions',
                    data: []
                }
            ]
        },
        isLoadingAutomation: false,
        graph2: {
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
                    name: 'Executions',
                    data: []
                }
            ]
        },
    }),
    created() {
        this.getWorkflowsData();
        this.getWorkflowsAutomaticData();
    },
    watch: {
        usageUnits() {
            this.setTotalExecution();
        }
    },
    computed: {
        totalWorkflows() {
            return this.graph2.series[0].data.reduce((a, b) => a + b, 0);
        },
        totalWorkflowsAutomatic() {
            return this.graph.series[0].data.reduce((a, b) => a + b, 0);
        },
        usageUnitWorkflowAutomatic() {
            if (!Array.isArray(this.usageUnits) || this.usageUnits.length === 0) {
                return 0;
            }
            return this.usageUnits.find(item => item.usageTypeName === ColTypeUsage.Automation)?.value ?? 0;
        },
        usageUnitWorkflow() {
            if (!Array.isArray(this.usageUnits) || this.usageUnits.length === 0) {
                return 0;
            }
            return this.usageUnits.find(item => item.usageTypeName === ColTypeUsage.Execution)?.value ?? 0;
        }
    },
    methods: {
        getWorkflowsData() {
            let params = {
                start: this.start,
                end: this.end,
                usageType: ColTypeUsage.Execution
            };
            this.isLoadingWorkflows = true;
            DashboardServices.GetByUsageType(params)
                .then((response) => {
                    if (response && !response.error) {
                        this.graph2.options = {
                            ...this.graph2.options,
                            xaxis: {
                                categories: response.map(item => item.date)
                            }
                        };
                        this.graph2.series = [{
                            name: 'Workflows',
                            data: response.map(item => item.value)
                        }];
                    }
                })
                .finally(() => {
                    this.isLoadingWorkflows = false;
                    this.setTotalExecution();
                });
        },
        setTotalExecution() {
            let totalExecution = (this.usageUnitWorkflow * this.totalWorkflows)
                + (this.usageUnitWorkflowAutomatic * this.totalWorkflowsAutomatic);
            this.$emit('setTotalExecution', totalExecution);
        },
        getWorkflowsAutomaticData() {
            let params = {
                start: this.start,
                end: this.end,
                usageType: ColTypeUsage.Automation
            };
            this.isLoadingAutomation = true;
            DashboardServices.GetByUsageType(params)
                .then((response) => {
                    if (response && !response.error) {
                        this.graph.options = {
                            ...this.graph.options,
                            xaxis: {
                                categories: response.map(item => item.date)
                            }
                        };
                        this.graph.series = [{
                            name: 'Automatic Workflows',
                            data: response.map(item => item.value)
                        }];
                    }
                })
                .finally(() => {
                    this.isLoadingAutomation = false;
                });
        },
        updateGraph(start, end) {
            this.start = start;
            this.end = end;
            this.getWorkflowsData();
            this.getWorkflowsAutomaticData();
        }
    },
}
</script>
