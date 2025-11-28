<template>
    <div class="card mb-3">
        <div class="card-body">
            <div class="d-flex align-items-center gap-2 mb-3">
                <h6 class="mb-0 fw-bold">{{ $t("dashboard.graphs.pagesGraphTitle") }}</h6>
                <LucideIcon v-tooltip.right="$t('dashboard.graphs.pagesTooltip')" icon="Info" :size="17" />
            </div>
            <div class="card ms-4 me-4 mb-3">
                <div class="card-body">
                    <h6>{{ $t("dashboard.graphs.totalPages") }}</h6>
                    <h4 class="mb-0 fw-bold">{{ totalPages }}</h4>
                    <span> {{ $t("dashboard.graphs.unitValue") }} {{ usageUnitPages }}</span>
                    <hr />
                    <span class="mt-1">{{ $t("dashboard.graphs.periodTotal") }}</span>
                    <h4 class="mb-0 fw-bold text-primary">{{ (totalPages * usageUnitPages).toFixed(5) }}</h4>
                </div>
            </div>
            <h6>{{ $t("dashboard.graphs.pagesGraphSubtitle") }}</h6>
            <LoadingComponent v-if="isLoading" />
            <BarGraphComponent v-else :options="graph.options" :series="graph.series" />
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
        LoadingComponent
    },
    props: {
        usageUnits: {
            type: Array,
            required: true,
        },
    },
    emits: ['setTotalPages'],
    data: () => ({
        start: null,
        end: null,
        isLoading: true,
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
                    name: 'Pages',
                    data: []
                }
            ]
        },
    }),
    computed: {
        totalPages() {
            return this.graph.series[0].data.reduce((a, b) => a + b, 0);
        },
        usageUnitPages() {
            if (this.usageUnits.length === 0) {
                return 0;
            }
            return this.usageUnits.find(item => item.usageTypeId === ColTypeUsage.Ocr)?.value ?? 0;
        }
    },
    created() {
        this.getPagesData();
    },
    watch: {
        usageUnits() {
            this.setTotalPages();
        }
    },
    methods: {
        getPagesData() {
            this.isLoading = true;
            let params = {
                start: this.start,
                end: this.end,
                id: ColTypeUsage.Ocr
            };
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
                            name: 'Pages',
                            data: response.map(item => item.value)
                        }];
                    }
                })
                .finally(() => {
                    this.isLoading = false;
                    this.setTotalPages();
                });
        },
        setTotalPages() {
            this.$emit('setTotalPages', this.usageUnitPages * this.totalPages);
        },
        updateGraph(start, end) {
            this.start = start;
            this.end = end;
            this.getPagesData();
        }
    },
}
</script>
